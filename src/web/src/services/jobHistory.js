import axios from '../shared/axios';
import requestHost from '../shared/apiConfig';

const concatReqUrl = (url) => {
    return `${requestHost}/${url.startsWith('/') ? url.slice(1) : url}`;
};

const fetchJobHistoryList = (params) => {
    return axios.get(concatReqUrl(`/Apps/${params.appId}/Reminders`), { params });
};

const fetchHistoryDetail = (params) => {
    return axios.get(concatReqUrl(`/Apps/${params.appId}/Reminders/${params.name}`), { params });
};

export {
    fetchHistoryDetail,
    fetchJobHistoryList,
};
