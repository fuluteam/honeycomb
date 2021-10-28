/**
 * @desc    作业管理历史列表视图
 * @version 1.0
 */
 import model from "@/model";
import React, { Fragment } from 'react';
import { Form, Icon, Input, Button, Spin, Table } from 'antd';
import columns from './columns';
import BaseView from '../BaseView';
import detail from '../../assets/icon/detail.svg';
import { PAGE_SIZE } from '../../shared/constants';
import HistoryDetailModal from './HistoryDetailModal';
import './index.less';
 
const namespace = 'jobHistory';

@model(namespace, 'global')
class History extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            pageIndex: 1,
            selectHistory: null,
            pageSize: PAGE_SIZE,
            showDetailModal: false,
        };
        this.namespace = namespace;
        this.initColumns(columns);
    }
    componentWillUnmount() {
        this.resetStore({
            loading: false,
            historyList: [],
            historyListTotal: 0,
            historyDetail: null,
            detailLoading: false,
        });
    }
    fetchJobHistoryList(resetPageIndex = false) {
        if (resetPageIndex) {
            this.setState({
                pageIndex: 1,
            }, () => {
                this.execFetchJobHistoryList();
            });
        } else {
            this.execFetchJobHistoryList();
        }
    }
    execFetchJobHistoryList() {
        const values = this.props.form.getFieldsValue();
        if (!values.appId) {
            return;
        }
        const { pageIndex, pageSize } = this.state;
        this.props.dispatch({
            type: 'jobHistory/fetchJobHistoryList',
            payload: {
                ...values,
                pageSize,
                pageIndex,
            },
        });
    }
    initColumns(cols) {
        cols[0].render = (_, __, i) => {
            const { pageIndex, pageSize } = this.state;
            return (
                <span>{((pageIndex - 1) * pageSize) + (i + 1)}</span>
            );
        };
        // 操作列渲染图标
        cols[cols.length - 1].render = (_, selectHistory) => {
            return (
                <Fragment>
                    <Icon
                        component={detail}
                        onClick={() => {
                            this.setState({
                                selectHistory,
                            });
                            this.toggleLogModal();
                        }}
                        title="详情"
                    />
                </Fragment>
            );
        };
    }

    toggleLogModal = () => {
        const { showDetailModal } = this.state;
        this.setState({
            showDetailModal: !showDetailModal,
        }, () => {
            if (!this.state.showDetailModal) {
                this.resetStore({
                    historyDetail: null,
                    detailLoading: false,
                });
            }
        });
    }

    handleSubmit = (e) => {
        e.preventDefault();
        const { form } = this.props;
        const { validateFields } = form;
        validateFields((err) => {
            if (!err) {
                this.fetchJobHistoryList(true);
            }
        });
    }
    onAppSelectChange = (appId) => {
        this.setState({
            pageIndex: 1,
        });
        if (appId) {
            this.fetchJobHistoryList(true);
        }
    }
    render() {
        const {
            pageSize,
            pageIndex,
            selectHistory,
            showDetailModal,
        } = this.state;
        const { form, jobHistory, dispatch } = this.props;
        const { getFieldDecorator } = form;
        const {
            historyDetail,
            detailLoading,
            loading = false,
            historyList = [],
            historyListTotal = 0,
        } = jobHistory;
        return (
            <div className="wrapper job-history">
                <Spin spinning={loading}>
                    <Form layout="inline" onSubmit={this.handleSubmit}>
                        <Form.Item>
                            {
                                getFieldDecorator('appId', {
                                    rules: [{
                                        required: true,
                                        message: '请选择应用',
                                    }]
                                })(this.renderAppList())
                            }
                        </Form.Item>
                        <Form.Item>
                            {
                                getFieldDecorator('search')(<Input placeholder="输入查询名称" />)
                            }
                        </Form.Item>
                        <Form.Item>
                            <Button
                                type="primary"
                                htmlType="submit"
                                disabled={!this.getAppId()}
                            >
                                查询
                            </Button>
                        </Form.Item>
                    </Form>
                    <Table
                        bordered
                        size="small"
                        rowKey="name"
                        columns={columns}
                        dataSource={historyList}
                        style={{
                            marginTop: '20px'
                        }}
                        pagination={{
                            pageSize,
                            current: pageIndex,
                            total: historyListTotal,
                            showSizeChanger: true,
                            showTotal(total) {
                                return `共${total}条`;
                            },
                            onChange: (pIndex, pSize) => {
                                this.setState({
                                    pageIndex: pIndex,
                                    pageSize: pSize,
                                }, this.fetchJobHistoryList);
                            },
                            onShowSizeChange: (_, pSize) => {
                                this.setState({
                                    pageIndex: 1,
                                    pageSize: pSize,
                                }, this.fetchJobHistoryList);
                            },
                        }}
                    />
                    <HistoryDetailModal
                        dispatch={dispatch}
                        appId={this.getAppId()}
                        loading={detailLoading}
                        visible={showDetailModal}
                        selectHistory={selectHistory}
                        historyDetail={historyDetail}
                        onCancel={this.toggleLogModal}
                    />
                </Spin>
            </div>
        );
    }
}
 
export default Form.create()(History);
 