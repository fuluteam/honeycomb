import { sleep, check } from 'k6';
import * as cron_reminder_create from './cron_reminder_create';
import * as reminder_create from './reminder_create';

// /* @ts-ignore */
// import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.1.0/index.js';
import http from 'k6/http';

export let options = cron_reminder_create.options;
export default cron_reminder_create.create_cron_reminder;

// export let options = reminder_create.options;
// export default reminder_create.create_reminder;

// export default () => {
//     const res = http.post('https://httpbin.org/status/400');
//     check(res, {
//         'status is 400': () => res.status === 400,
//     });
//     sleep(randomIntBetween(1, 5));
// };
