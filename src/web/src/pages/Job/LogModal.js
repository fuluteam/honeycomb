/**
 * @desc    作业执行日志详情
 * @version 1.0
 */
import { Modal, Spin, Table } from 'antd';
import React, { PureComponent } from 'react';
import { renderDate } from './renderHelper';

function renderCell({ command }, renderKey) {
    const value = command[renderKey] || '';
    if (renderKey === 'httpMethod') {
        return (
            <span>{value}</span>
        );
    }
    return (
        <span
            title={value}
            className="nowrap"
            style={{ width: 250 }}
        >
            {value}
        </span>
    );
}

const columns = [
    ...['url', 'httpMethod', 'payloadJson'].map((k) => {
        return {
            title: k,
            dataIndex: k,
            width: k === 'httpMethod' ? 100 : 280,
            render(_, record) {
                return renderCell(record, k);
            },
        };
    }), {
        width: 200,
        title: '执行时间',
        dataIndex: 'startedAt',
        render: renderDate,
    }, {
        width: 120,
        title: '是否执行成功',
        dataIndex: 'status',
    }];

const defaultState = {
    pageIndex: 1,
    pageSize: 10,
};
class LogModal extends PureComponent {
    constructor(props) {
        super(props);
        this.state = defaultState;
    }

    componentDidUpdate(prevProps) {
        if (!prevProps.visible && this.props.visible) {
            this.fetchLogList();
        }
    }

    handleAfterClose = () => {
        const { dispatch } = this.props;
        dispatch({
            type: 'job/resetState',
            payload: {
                logTotal: 0,
                logList: [],
            },
        });
        this.setState(defaultState);
    }

    createRowKey(record, index) {
        const { createdAt } = record;
        return `${createdAt}-${index}`;
    }

    fetchLogList() {
        const { pageIndex, pageSize } = this.state;
        const { jobLog, dispatch, appId } = this.props;
        const { name } = jobLog;
        dispatch({
            type: 'job/fetchLogList',
            payload: {
                name,
                appId,
                pageSize,
                pageIndex,
            },
        });
    }

    render() {
        const { pageSize, pageIndex } = this.state;
        const {
            visible,
            logList,
            logTotal,
            closeLogModal,
            loading = false,
        } = this.props;
        return (
            <Modal
                centered
                title="日志"
                width={1024}
                footer={null}
                visible={visible}
                onOk={closeLogModal}
                onCancel={closeLogModal}
                afterClose={this.handleAfterClose}
                wrapClassName="job-relate-modal job-manage-modal"
            >
                <Spin spinning={loading}>
                    <Table
                        bordered
                        size="small"
                        columns={columns}
                        dataSource={logList}
                        rowKey={this.createRowKey}
                        pagination={{
                            pageSize,
                            total: logTotal,
                            current: pageIndex,
                            showTotal(total) {
                                return `共${total}条`;
                            },
                            onChange: (pIndex, pSize) => {
                                this.setState({
                                    pageIndex: pIndex,
                                    pageSize: pSize,
                                }, this.fetchLogList);
                            },
                            onShowSizeChange: (_, pSize) => {
                                this.setState({
                                    pageIndex: 1,
                                    pageSize: pSize,
                                }, this.fetchLogList);
                            },
                        }}
                    />
                </Spin>
            </Modal>
        );
    }
}

export default LogModal;
