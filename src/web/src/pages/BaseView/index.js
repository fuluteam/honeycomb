import React from 'react';
import { Select } from 'antd';

class BaseView extends React.PureComponent {
    didMount() {

    }
    didUpdate() {

    }
    fetchAppList() {
        const { dispatch } = this.props;
        dispatch({
            type: 'global/fetchAppList',
        });
    }
    componentDidMount() {
        this.didMount();
        this.fetchAppList();
    }
    getDataFormNameSpace(dataKey, defaultVal = []) {
        return this.props[this.namespace][dataKey] || defaultVal;
    }
    resetStore(payload) {
        this.props.dispatch({
            type: `${this.namespace}/resetState`,
            payload,
        });
    }
    onAppSelectChange() {
        
    }
    getAppId() {
        return this.props.form.getFieldValue('appId');
    }
    renderAppList() {
        const { appList = [] } = this.props.global;
        return (
            <Select
                showSearch
                style={{
                    width: 250
                }}
                filterOption={(inputValue, option) => {
                    if (inputValue) {
                        return option.props.children.includes(inputValue);
                    }
                    return true;
                }}
                onChange={this.onAppSelectChange}
            >
                {
                    Array.isArray(appList) && appList.map(({ key, displayName }) => {
                        return (
                            <Select.Option key={key}>{displayName}</Select.Option>
                        );
                    })
                }
            </Select>
        );
    }
}

export default BaseView;
