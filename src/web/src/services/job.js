import axios from '../shared/axios';
import requestHost from '../shared/apiConfig';

const concatReqUrl = (url) => {
    return `${requestHost}/${url.startsWith('/') ? url.slice(1) : url}`;
};

/**
 * @desc 查询作业列表
 * @param {*} params
 */
const fetchJobList = (params) => {
    return axios.get(concatReqUrl(`/Apps/${params.appId}/CronReminders`), { params });
};

/**
 * @desc 查询应用列表
 * @param {*} params
 */
const fetchAppList = () => {
    return axios.get(`${window.configs.host.udev}/api/App/GetOpenApps?pageSize=999&pageIndex=1`);
};

/**
 * @desc  新增/编辑作业
 * @param {object} data
 */
const commitJob = (data) => {
    return axios.post(concatReqUrl(`/Apps/${data.appId}/CronReminders`), data);
};

const deleteJob = (data) => {
    return axios.delete(concatReqUrl(`/Apps/${data.appId}/CronReminders/${data.name}`), { data });
};

const startJob = (data) => {
    return axios.delete(concatReqUrl(`/Apps/${data.appId}/CronReminders/${data.name}/suspension`), { data });
};

const pauseJob = (data) => {
    return axios.post(concatReqUrl(`/Apps/${data.appId}/CronReminders/${data.name}/suspension`), data);
};

/**
 * 查询日志列表
 * @param {*} params
 */
const fetchLogList = (params) => {
    return axios.get(concatReqUrl(`/Apps/${params.appId}/CronReminders/${params.name}/items`), { params });
};

const fetchLogDetail = (params) => {
    return axios.get(concatReqUrl(`/Apps/${params.appId}/CronReminders/${params.name}/items/${params.schedule}`), { params });
};


export {
    startJob,
    pauseJob,
    commitJob,
    deleteJob,
    fetchJobList,
    fetchLogList,
    fetchAppList,
    fetchLogDetail,
};
