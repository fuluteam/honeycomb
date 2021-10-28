import axios from 'axios';

axios.defaults.timeout = 30000;
axios.defaults.withCredentials = true;
axios.defaults.headers.post['Content-Type'] = 'application/json';

export default axios;
