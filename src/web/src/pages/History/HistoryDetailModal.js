import React from 'react';
import { Divider, Modal, Spin, Button } from 'antd';
import { renderDate } from '../Job/renderHelper';

function renderNest(data, key) {
    if (data) {
        const value = data[key] || '';
        return (<span>{typeof value === 'object' ? JSON.stringify(value) : value}</span>);
    }
    return null;
}

const cellRenderMap = [
    {
        label: '名称',
        key: 'name',
    }, {
        label: '任务名称',
        key: 'displayName',
    }, {
        label: 'URL',
        key: 'command',
        render(data) {
            return renderNest(data, 'url');
        },
    }, {
        label: 'HttpMethod',
        key: 'command',
        render(data) {
            return renderNest(data, 'httpMethod');
        },
    }, {
        label: 'Headers',
        key: 'command',
        render(data) {
            return renderNest(data, 'headers');
        },
    }, {
        label: 'payloadJson',
        key: 'command',
        render(data) {
            return renderNest(data, 'payloadJson');
        },
    }, {
        label: '通知配置',
        key: 'notificationSettingNames',
    }, {
        label: '创建时间',
        key: 'createdAt',
        render: renderDate,
    }, {
        label: '设定执行时间',
        key: 'schedule',
        render: renderDate,
    }, {
        label: '开始执行时间',
        key: 'startedAt',
        render: renderDate,
    }, {
        label: '结束执行时间',
        key: 'finishedAt',
        render: renderDate,
    }, {
        label: '运行状态',
        key: 'status',
    }, {
        label: '错误详情',
        key: 'reason',
    }, {
        label: '执行结果',
        key: 'result',
        render(data) {
            if (data) {
                const { responseBody, statusCode } = data;
                return (
                    <React.Fragment>
                        <div><strong>statusCode: </strong>{statusCode}</div>
                        <div style={{ wordBreak: 'break-word' }}><strong>responseBody: </strong>{responseBody}</div>
                    </React.Fragment>
                );
            }
            return null;
        },
    },
];

class HistoryDetailModal extends React.PureComponent {
    componentDidUpdate(prevProps) {
        if (!prevProps.visible && this.props.visible) {
            this.fetchHistoryDetail();
        }
    }

    fetchHistoryDetail() {
        const { dispatch, appId, selectHistory } = this.props;
        dispatch({
            type: 'jobHistory/fetchHistoryDetail',
            payload: {
                appId,
                name: selectHistory.name,
            },
        });
    }

    renderContent() {
        const { historyDetail } = this.props;
        if (historyDetail) {
            return cellRenderMap.map(({ label, key, render }) => {
                return (
                    <tr key={label}>
                        <td className="cell-title">{label}</td>
                        <td>{render ? render(historyDetail[key]) : historyDetail[key]}</td>
                    </tr>
                );
            });
        }
        return null;
    }

    render() {
        const { visible, loading, onCancel } = this.props;
        return (
            <Modal
                width={650}
                footer={null}
                title="历史详情"
                visible={visible}
                onCancel={onCancel}
                wrapClassName="history-detail-modal"
            >
                <Spin spinning={loading}>
                    <table className="detail-table">
                        <colgroup>
                            <col style={{ width: 200 }} />
                            <col />
                        </colgroup>
                        <tbody>
                            {this.renderContent()}
                        </tbody>
                    </table>
                    <Divider />
                    <div style={{ textAlign: 'right' }}>
                        <Button
                            type="primary"
                            onClick={onCancel}
                        >
                            确定
                        </Button>
                    </div>
                </Spin>
            </Modal>
        );
    }
}

export default HistoryDetailModal;
