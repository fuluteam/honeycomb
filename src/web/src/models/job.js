import * as service from '../services/job';
import initMethod, { isEmptyObject } from './helper';

const callServiceMethod = initMethod(service);

const initalState = {
    jobList: [],
    logList: [],
    logTotal: 0,
    jobListTotal: 0,
    logLoading: false,
    jobLoading: false,
};

export default {
    namespace: 'job',
    state: initalState,
    effects: {
        resetState(payload, updateStore) {
            updateStore(isEmptyObject(payload) ? initalState : payload);
        },
        fetchJobList(payload, updateStore) {
            updateStore({
                loading: true,
            });
            const newStore = {
                loading: false,
            };
            return callServiceMethod(payload, 'fetchJobList', (response) => {
                const { data: { items, totalCount } } = response;
                newStore.jobList = items;
                newStore.jobListTotal = totalCount;
            }, null, () => {
                updateStore(newStore);
            });
        },
        commitJob(payload, updateStore) {
            updateStore({
                jobLoading: true,
            });
            return callServiceMethod(payload, 'commitJob', null, null, () => {
                updateStore({
                    jobLoading: false,
                });
            });
        },
        deleteJob(payload, updateStore) {
            updateStore({
                loading: true,
            });
            return callServiceMethod(payload, 'deleteJob', null, null, () => {
                updateStore({
                    loading: false,
                });
            });
        },
        startJob(payload, updateStore) {
            updateStore({
                loading: true,
            });
            return callServiceMethod(payload, 'startJob', null, null, () => {
                updateStore({
                    loading: false,
                });
            });
        },
        pauseJob(payload, updateStore) {
            updateStore({
                loading: true,
            });
            return callServiceMethod(payload, 'pauseJob', null, null, () => {
                updateStore({
                    loading: false,
                });
            });
        },
        fetchLogList(payload, updateStore) {
            updateStore({
                logLoading: true,
            });
            const newStore = {
                logLoading: false,
            };
            return callServiceMethod(payload, 'fetchLogList', (response) => {
                const { data: { items, totalCount } } = response;
                newStore.logList = items;
                newStore.logTotal = totalCount;
            }, null, () => {
                updateStore(newStore);
            });
        },
        fetchLogDetail(payload) {
            return callServiceMethod(payload, 'fetchLogDetail');
        },
    },
};
