import { Service } from "@pulumi/kubernetes/core/v1";
import { ServiceMonitor } from "pulumi-crds-prometheus-operator/monitoring/v1";

export function deploy(name: string, service: Service, portName: string) {
    return new ServiceMonitor(name, {
        metadata: {
            name: service.metadata.name,
            namespace: service.metadata.namespace,
            labels: service.metadata.labels
        },
        spec: {
            selector: {
                matchLabels: service.metadata.labels
            },
            endpoints: [{
                port: portName,
                path: "/metrics",
                interval: "5s"
            }]
        }
    });
}
