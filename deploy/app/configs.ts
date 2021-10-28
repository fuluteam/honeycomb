import * as pulumi from "@pulumi/pulumi";

const config = new pulumi.Config();

// export const pgsql_connstr = config.requireSecret("pgsql_connstr");

export const image_version = "96";

export const cors_origin = config.require("cors_origin");
