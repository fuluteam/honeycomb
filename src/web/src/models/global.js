import * as service from '../services/global';

export default {
    namespace: 'global',
    state: {
        appList: [],
    },
    effects: {
        async fetchAppList(_, updateStore) {
            const result = await service.fetchAppList();
            updateStore({
                appList: result.data.items || [],
            });
        },
    },
};
