import * as service from '../services/jobHistory';
import initMethod, { isEmptyObject } from './helper';

const callServiceMethod = initMethod(service);

const initalState = {
    loading: false,
    historyList: [],
    historyListTotal: 0,
    historyDetail: null,
    detailLoading: false,
};

export default {
    namespace: 'jobHistory',
    state: initalState,
    effects: {
        resetState(payload, updateStore) {
            updateStore(isEmptyObject(payload) ? initalState : payload);
        },
        fetchJobHistoryList(payload, updateStore) {
            updateStore({
                loading: true,
            });
            const newStore = {
                loading: false,
            };
            return callServiceMethod(payload, 'fetchJobHistoryList', (response) => {
                const { data: { items, totalCount } } = response;
                newStore.historyList = items;
                newStore.historyListTotal = totalCount;
            }, null, () => {
                updateStore(newStore);
            });
        },
        fetchHistoryDetail(payload, updateStore) {
            updateStore({
                detailLoading: true,
            });
            const newStore = {
                detailLoading: false,
            };
            return callServiceMethod(payload, 'fetchHistoryDetail', ({ data }) => {
                newStore.historyDetail = data;
            }, null, () => {
                updateStore(newStore);
            });
        },
    },
};
