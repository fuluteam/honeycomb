import { check } from 'k6';
import { Options } from 'k6/options';
import http from 'k6/http';
import { access_token, base_url } from './constants';


export let options: Options = {
    // vus: 200,
    // duration: '60s',
    discardResponseBodies: true,
    scenarios: {
        contacts: {
            executor: 'shared-iterations',
            vus: 100,
            iterations: 2000,
            maxDuration: '300s',
        },
    },
};


export var create_cron_reminder = () => {
    const url = `${base_url}/Apps/10000275/CronReminders`
    const req_body= {
        "name": `BB01_${__VU}.${__ITER}`,
        "displayName": `BB01_${__VU}.${__ITER}`,
        "schedule": "0 2/5 * * * *",
        "command": {
            // "url": "http://pre-loadapi-api.suuyuu.cn/LoadNum",
            // "httpMethod": "GET"
            "url": "http://newhoneycomb-api.gdrc.svc.cluster.local/health",
            "httpMethod": "GET"
        },
        "suspend": false
    };
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${access_token}`
        }
    };
    const res = http.post(url, JSON.stringify(req_body), params);
    check(res, {
        'status is 201': () => res.status === 201,
    });
};
