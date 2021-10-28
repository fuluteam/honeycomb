import * as pulumi from "@pulumi/pulumi";
import { ConfigMap, Secret, Service, ServiceSpecType } from "@pulumi/kubernetes/core/v1";
import { service_name, namespace_name, shared_labels } from "./core";
import { image_version, cors_origin } from "./configs";
import { Deployment } from "@pulumi/kubernetes/apps/v1";
import { Role, RoleBinding } from "@pulumi/kubernetes/rbac/v1";
import { ServiceAccount } from "@pulumi/kubernetes/core/v1";
import { Ingress } from "@pulumi/kubernetes/networking/v1beta1";
import { ServiceMonitor } from "pulumi-crds-prometheus-operator/monitoring/v1";
import { PgSQLConfig } from "./pgsql";


const otenvs = [{
    name: "OTEL_EXPORTER_OTLP_ENDPOINT",
    value: "http://grafana-agent-traces.tempo.svc.cluster.local:55680",
}]

export function deploy(pgsql_config: PgSQLConfig) {
    const sa = deploy_rbac();
    const secret = deploy_secret(pgsql_config);
    const configmap = deploy_configmap();
    const api = deploy_api(configmap, secret);
    const server = deploy_server(configmap, secret, sa);
    deploy_ingress(api.service);
    deploy_monitoring();
    return { api, server }
}

function deploy_rbac() {
    const sa_name = "honeycomb-server";
    const service_account = new ServiceAccount(sa_name, {
        metadata: {
            name: sa_name,
            namespace: namespace_name
        }
    });
    const subject = {
        kind: service_account.kind,
        name: service_account.metadata.name,
        namespace: service_account.metadata.namespace
    };

    const pod_reader_role = new Role("pod_reader", {
        metadata: {
            name: "pod_reader",
            namespace: subject.namespace
        },
        rules: [{
            apiGroups: [""],
            resources: ["pods"],
            verbs: ["get", "list", "watch"]
        }]
    });
    const pod_reader_rolebinding = new RoleBinding(sa_name, {
        metadata: { name: sa_name, namespace: subject.namespace },
        roleRef: {
            apiGroup: "",
            kind: pod_reader_role.kind,
            name: pod_reader_role.metadata.name,
        },
        subjects: [subject]
    });

    return service_account;
}


function deploy_secret(pgsql_config: PgSQLConfig) {
    const secret = new Secret(service_name, {
        metadata: {
            namespace: namespace_name,
            name: service_name,
            labels: shared_labels
        },
        type: "Opaque",
        stringData: {
            "PostgreSQL__ConnectionString": pulumi.interpolate`Host=${pgsql_config.host};Port=${pgsql_config.port};Database=honeycomb;Username=postgres;password=${pgsql_config.password};Multiplexing=true`,
        }
    });
    return secret;
}

function deploy_configmap() {
    const configmap = new ConfigMap(service_name, {
        metadata: {
            namespace: namespace_name,
            name: service_name,
            labels: shared_labels
        },
        data: {
            "Cluster__ClusterId": service_name,
            "Cluster__ServiceId": service_name,
        }
    });
    return configmap;
}

function deploy_api(configmap: ConfigMap, secret: Secret) {
    const app_name = "honeycomb-api";
    const image = `registry.cn-hangzhou.aliyuncs.com/surac/honeycomb.api:${image_version}`
    const labels: { [key: string]: string } = {
        app: app_name,
        ...shared_labels
    };
    const deployment = new Deployment(app_name, {
        metadata: {
            name: app_name,
            namespace: namespace_name,
            labels: labels,
        },
        spec: {
            selector: {
                matchLabels: labels
            },
            template: {
                metadata: {
                    labels: labels,
                },
                spec: {
                    containers: [{
                        name: app_name,
                        image: image,
                        ports: [{
                            name: 'http',
                            containerPort: 80
                        }],
                        livenessProbe: {
                            httpGet: {
                                path: "/health",
                                port: 80
                            }
                        },
                        readinessProbe: {
                            httpGet: {
                                path: "/health",
                                port: 80
                            }
                        },
                        env: [{
                            name: "ASPNETCORE_ENVIRONMENT",
                            value: pulumi.getStack()
                        }, ...otenvs],
                        envFrom: [{
                            secretRef: { name: secret.metadata.name }
                        }, {
                            configMapRef: { name: configmap.metadata.name }
                        }],
                    }],
                }
            }
        }
    });

    const service = new Service(app_name, {
        metadata: {
            name: app_name,
            namespace: namespace_name,
            labels: {
                app: app_name,
                ...shared_labels
            }
        },
        spec: {
            ports: [{
                name: "http",
                port: 80
            }],
            selector: deployment.spec.template.metadata.labels,
            type: ServiceSpecType.ClusterIP
        }
    });

    return { deployment, service };
}


function deploy_server(configmap: ConfigMap, secret: Secret, service_account: ServiceAccount) {
    const app_name = "honeycomb-server";
    const image = `registry.cn-hangzhou.aliyuncs.com/surac/honeycomb.server:${image_version}`
    const labels: { [key: string]: string } = {
        app: app_name,
        "orleans/serviceId": service_name,
        "orleans/clusterId": service_name,
        ...shared_labels
    };
    const deployment = new Deployment(app_name, {
        metadata: {
            name: app_name,
            namespace: namespace_name,
            labels: labels,
        },
        spec: {
            selector: {
                matchLabels: labels
            },
            template: {
                metadata: {
                    labels: labels,
                },
                spec: {
                    serviceAccountName: service_account.metadata.name,
                    containers: [{
                        name: app_name,
                        image: image,
                        ports: [{
                            name: 'silo',
                            containerPort: 11111
                        }, {
                            name: 'gateway',
                            containerPort: 30000
                        }, {
                            name: 'metrics',
                            containerPort: 80
                        }],
                        env: [{
                            name: "ASPNETCORE_ENVIRONMENT",
                            value: pulumi.getStack()
                        }, {
                            name: "DOTNET_SHUTDOWNTIMEOUTSECONDS",
                            value: "120"
                        }, {
                            name: "ORLEANS_SERVICE_ID",
                            valueFrom: {
                                fieldRef: {
                                    fieldPath: "metadata.labels['orleans/serviceId']"
                                }
                            }
                        }, {
                            name: "ORLEANS_CLUSTER_ID",
                            valueFrom: {
                                fieldRef: {
                                    fieldPath: "metadata.labels['orleans/clusterId']"
                                }
                            }
                        }, {
                            name: "POD_NAMESPACE",
                            valueFrom: {
                                fieldRef: {
                                    fieldPath: "metadata.namespace"
                                }
                            }
                        }, {
                            name: "POD_NAME",
                            valueFrom: {
                                fieldRef: {
                                    fieldPath: "metadata.name"
                                }
                            }
                        }, {
                            name: "POD_IP",
                            valueFrom: {
                                fieldRef: {
                                    fieldPath: "status.podIP"
                                }
                            }
                        }, ...otenvs],
                        envFrom: [{
                            secretRef: { name: secret.metadata.name }
                        }, {
                            configMapRef: { name: configmap.metadata.name }
                        }],
                    }],
                }
            }
        }
    });

    const service = new Service(app_name, {
        metadata: {
            name: app_name,
            namespace: namespace_name,
            labels: {
                app: app_name,
                ...shared_labels
            }
        },
        spec: {
            ports: [{
                name: "metrics",
                port: 80
            }],
            selector: deployment.spec.template.metadata.labels,
            type: ServiceSpecType.ClusterIP
        }
    });

    return { deployment, service };
}

function deploy_ingress(service: Service) {
    const host = "honeycomb.s.ichnb.com";
    const ingress_name = "honeycomb";
    return new Ingress(ingress_name, {
        metadata: {
            name: ingress_name,
            namespace: namespace_name,
            labels: shared_labels,
            annotations: {
                "cert-manager.io/cluster-issuer": "letsencrypt",
                "nginx.ingress.kubernetes.io/cors-allow-headers": "DNT,X-CustomHeader,Keep-Alive,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Authorization,x-requestid,developerid",
                "nginx.ingress.kubernetes.io/cors-allow-origin": cors_origin,
                "nginx.ingress.kubernetes.io/enable-cors": "true",
            }
        },
        spec: {
            ingressClassName: "nginx",
            tls: [
                {
                    hosts: [host],
                    secretName: `${ingress_name}-tls-secret`
                }
            ],
            rules: [
                {
                    host: host,
                    http: {
                        paths: [
                            {
                                path: "/",
                                backend: {
                                    serviceName: service.metadata.name,
                                    servicePort: "http"
                                }
                            },
                        ],
                    },
                }
            ]
        }
    });
}

function deploy_monitoring() {
    const name = "honeycomb";
    return new ServiceMonitor(name, {
        metadata: {
            name: name,
            namespace: namespace_name,
            labels: shared_labels
        },
        spec: {
            selector: {
                matchLabels: shared_labels
            },
            endpoints: [{
                port: "http",
                path: "/metrics",
                interval: "5s"
            }, {
                port: "metrics",
                path: "/metrics",
                interval: "5s"
            }]
        }
    });

}

