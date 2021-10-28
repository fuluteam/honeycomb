import axios from '../shared/axios';
import requestHost from '../shared/apiConfig';

/**
 * @desc 查询应用列表
 */
const fetchAppList = () => {
    return axios.get(`${requestHost}/apps`);
};

export { fetchAppList };
