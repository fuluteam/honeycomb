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
            vus: 200,
            iterations: 5000,
            maxDuration: '120s',
        },
        // contacts: {
        //     executor: 'constant-arrival-rate',
        //     rate: 300, // 200 RPS, since timeUnit is the default 1s
        //     duration: '20s',
        //     preAllocatedVUs: 50,
        //     maxVUs: 1000,
        // },
    },
};


export var create_reminder = () => {
    const url = `${base_url}/Apps/10000275/Reminders`
    const req_body = {
            "name": `BB06_${__VU}.${__ITER}`,
            "schedule": "2021-09-30T12:08:00.000Z",
            "command": {
                // "url": "http://pre-loadapi-api.suuyuu.cn/LoadNum",
                // "httpMethod": "GET"
                "url": "http://newhoneycomb-api.gdrc.svc.cluster.local/health",
                "httpMethod": "GET"
            }
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
