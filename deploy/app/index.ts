import * as workloads from "./workloads";
import * as pgsql from "./pgsql";

const pgsql_config = pgsql.deploy();
const apps = workloads.deploy(pgsql_config);

export const image = apps.api.deployment.spec.template.spec.containers[0].image;
