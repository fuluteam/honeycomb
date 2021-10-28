import model from "@/model";
import React, { Fragment } from "react";
import { Spin, Form, Button, Table, Input, Icon, message, Popconfirm } from 'antd';
import columns from "./columns";
import BaseView from "../BaseView";
import JobModal from "./JobModal";
import LogModal from "./LogModal";
import pause from "../../assets/icon/pause.svg";
import detail from "../../assets/icon/detail.svg";
import deleteDetail from "../../assets/icon/delete.svg";
import editSquare from "../../assets/icon/edit-square.svg";
import caretRight from "../../assets/icon/caret-right.svg";
import { MODE_ADD, MODE_EDIT, PAGE_SIZE } from "../../shared/constants";
import "./index.less";

const namespace = 'job';

@model('job', 'global')
class Job extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            jobLog: null, // 作业日志对象
            pageIndex: 1,
            editJob: null,
            showJobModal: false,
            pageSize: PAGE_SIZE,
            showLogModal: false,
            jobModalMode: MODE_ADD,
        };
        this.namespace = namespace;
        this.initColumns(columns);
    }
    componentWillUnmount() {
        this.resetStore({
            jobList: [],
            logList: [],
            logTotal: 0,
            jobListTotal: 0,
            logLoading: false,
            jobLoading: false,
        });
    }
    /**
     * @desc 查询作业列表
     * @param{boolean} reset 是否重置pageIndex
     */
    fetchJobList = (resetPageIndex = false) => {
        if (resetPageIndex) {
            this.setState({
                pageIndex: 1,
            }, () => {
                this.execFetchJobList();
            });
        } else {
            this.execFetchJobList();
        }
    };
    execFetchJobList() {
        const appId = this.getAppId();
        if (!appId) {
            return;
        }
        const { pageIndex, pageSize } = this.state;
        const { form, dispatch } = this.props;
        dispatch({
            type: 'job/fetchJobList',
            payload: {
                appId,
                pageSize,
                pageIndex,
                search: form.getFieldValue('search'),
            },
        });
    }
    /**
     * @desc  初始化表格数据头
     * @param {array} cols
     */
    initColumns(cols) {
        cols[0].render = (_, __, i) => {
        const { pageIndex, pageSize } = this.state;
            return <span>{(pageIndex - 1) * pageSize + (i + 1)}</span>;
        };
        // 操作列渲染图标
        cols[cols.length - 1].render = (_, record) => {
        const { suspend } = record;
        return (
                <Fragment>
                    <Icon
                        title="日志"
                        component={detail}
                        data-name={record.name}
                        onClick={this.handleLog}
                    />
                    <Icon
                        title="编辑"
                        component={editSquare}
                        data-name={record.name}
                        onClick={this.handleEdit}
                    />
                    {this.renderStatusIcon(suspend, record)}
                    <Popconfirm
                        title="确定删除?"
                        onConfirm={(e) => {
                            this.handleDelete(e, { name: record.name });
                        }}
                    >
                        <Icon title="删除" component={deleteDetail} />
                    </Popconfirm>
                </Fragment>
            );
        };
    }
    /**
     * @desc 根据作业运行状态渲染启动/暂停图标
     * @param {boolean} isPause
     */
    renderStatusIcon(isPause, record) {
        let title = '执行';
        let icon = caretRight;
        let clickHandler = this.handleStart;
        if (!isPause) {
            title = '暂停';
            icon = pause;
            clickHandler = this.handlePause;
        }
        return (
            <Icon
                title={title}
                component={icon}
                onClick={clickHandler}
                data-name={record.name}
            />
        );
    }
    /**
     * @desc  根据点击事件目标对象获取行数据
     * @param {object} target
     */
    getDataByTarget(target) {
        let targetName = target.nodeName.toLowerCase();
        if (targetName === 'path') {
            target = target.parentNode;
            targetName = target.nodeName.toLowerCase();
        }
        if (targetName === 'svg') {
            target = target.parentNode;
        }
        const name = target.dataset.name;
        // 是操作列中的图标被点击
        if (name) {
            return {
                name,
            };
        }
        return null;
    }
    /**
     * @desc  查看日志详情
     * @param {*} e
     */
    handleLog = (e) => {
        e.stopPropagation();
        const target = this.getDataByTarget(e.target);
        if (target) {
            this.setState({
                jobLog: target,
                showLogModal: true,
            });
        }
    };
    /**
     * @desc 编辑作业详情
     * @param {*} e
     */
    handleEdit = (e) => {
        e.stopPropagation();
        const jobData = this.getDataByTarget(e.target);
        if (jobData) {
            const editJob = this.props.job.jobList.find(({ name }) => {
                return name === jobData.name;
            });
            this.setState({
                editJob,
                showJobModal: true,
                jobModalMode: MODE_EDIT,
            });
        }
    };
    /**
     * @desc  暂停作业
     * @param {*} e
     */
    handlePause = (e) => {
        e.stopPropagation();
        const jobData = this.getDataByTarget(e.target);
        if (jobData) {
            this.props.dispatch({
                type: 'job/pauseJob',
                payload: {
                    name: jobData.name,
                    appId: this.getAppId(),
                },
            }).then((response) => {
                this.dispatchCallBack(response, '暂停成功');
            });
        }
    };
    handleDelete = (e, jobData) => {
        e.stopPropagation();
        this.props.dispatch({
            type: 'job/deleteJob',
            payload: {
                name: jobData.name,
                appId: this.getAppId(),
            },
        }).then((response) => {
            this.dispatchCallBack(response, '删除成功', true);
        });
    };
    /**
     * 启动指定作业
     */
    handleStart = (e) => {
        e.stopPropagation();
        const jobData = this.getDataByTarget(e.target);
        // 启动单条数据
        if (jobData) {
            this.props.dispatch({
                type: 'job/startJob',
                payload: {
                    name: jobData.name,
                    appId: this.getAppId(),
                },
            }).then((response) => {
                this.dispatchCallBack(response, '启动成功');
            });
        }
    };
    dispatchCallBack(response, tips, resetPageIndex = false) {
        if (response && !response.errors) {
            message.success(tips);
            this.fetchJobList(resetPageIndex);
        }
    }
    /**
     * @desc 新增弹窗切换事件
     */
    toggleJobModalVisible = () => {
        if (!this.getAppId()) {
            return;
        }
        const { job } = this.props;
        if (job.jobLoading) {
            return;
        }
        const { showJobModal } = this.state;
        if (showJobModal) {
            // 即将关闭弹窗，重置状态
            this.resetStore({
                jobLoading: false,
            });
        }
        this.setState({
            editJob: null,
            jobModalMode: MODE_ADD,
            showJobModal: !showJobModal,
        });
    };
    closeLogModal = () => {
        const { showLogModal } = this.state;
        this.setState({
            jobLog: null,
            showLogModal: !showLogModal,
        });
    };
    /**
     * @desc 新增/编辑作业提交事件
     * @param {object} payload
     */
    commitJob = (payload) => {
        const { jobModalMode } = this.state;
        this.props.dispatch({
            type: 'job/commitJob',
            payload,
        }).then((response) => {
            if (response && !response.errors) {
                message.success(
                    `${jobModalMode === MODE_EDIT ? "编辑" : "新增"}成功`
                );
                this.toggleJobModalVisible();
                this.fetchJobList(true);
            }
        });
    };
    handleSubmit = (e) => {
        e.preventDefault();
        const { form } = this.props;
        const { validateFields } = form;
        validateFields((err) => {
            if (!err) {
                this.setState({
                    pageIndex: 1,
                }, () => {
                    this.fetchJobList();
                });
            }
        });
    };
    onAppSelectChange = (appId) => {
        if (appId) {
            this.setState({
                pageIndex: 1,
                editJob: null,
                selectRowKeys: [],
            });
            this.fetchJobList(true);
        }
    };
    render() {
        const {
            jobLog,
            editJob,
            pageSize,
            pageIndex,
            jobModalMode,
            showLogModal,
            showJobModal,
        } = this.state;
        const { form, job, dispatch } = this.props;
        const { getFieldDecorator } = form;
        const btnDisabled = !this.getAppId();
        const {
            logLoading,
            jobList = [],
            logList = [],
            logTotal = 0,
            loading = false,
            jobListTotal = 0,
            jobLoading = false,
        } = job;
        return (
            <div className="wrapper job-manage">
                <Spin spinning={loading}>
                    <Form layout="inline" onSubmit={this.handleSubmit}>
                        <Form.Item>
                            {getFieldDecorator("appId")(this.renderAppList())}
                        </Form.Item>
                        <Form.Item>
                            {getFieldDecorator("search")(
                                <Input placeholder="输入查询名称" />
                            )}
                        </Form.Item>
                        <Form.Item>
                        <Button
                            type="primary"
                            htmlType="submit"
                            disabled={btnDisabled}
                        >
                            查询
                        </Button>
                        </Form.Item>
                        <Form.Item className="btn-group">
                        <Button
                            ghost
                            type="primary"
                            className="btn-add"
                            disabled={btnDisabled}
                            onClick={this.toggleJobModalVisible}
                        >
                            新建
                        </Button>
                        </Form.Item>
                    </Form>
                    <Table
                        bordered
                        size="small"
                        rowKey="name"
                        columns={columns}
                        dataSource={jobList}
                        style={{
                            marginTop: "20px",
                        }}
                        pagination={{
                            current: pageIndex,
                            pageSize: pageSize,
                            total: jobListTotal,
                            showSizeChanger: true,
                            showTotal(total) {
                                return `共${total}条`;
                            },
                            onChange: (pIndex, pSize) => {
                                this.setState({
                                    pageSize: pSize,
                                    pageIndex: pIndex,
                                }, this.fetchJobList);
                            },
                            pageSizeOptions: ["10", "20", "30", "50"],
                            onShowSizeChange: (_, pSize) => {
                                this.setState({
                                    pageIndex: 1,
                                    pageSize: pSize,
                                }, this.fetchJobList);
                            },
                        }}
                    />
                    <JobModal
                        editJob={editJob}
                        dispatch={dispatch}
                        mode={jobModalMode}
                        loading={jobLoading}
                        visible={showJobModal}
                        appId={this.getAppId()}
                        commitJob={this.commitJob}
                        toggleJobModalVisible={this.toggleJobModalVisible}
                    />
                    <LogModal
                        jobLog={jobLog}
                        logList={logList}
                        logTotal={logTotal}
                        dispatch={dispatch}
                        loading={logLoading}
                        visible={showLogModal}
                        appId={this.getAppId()}
                        closeLogModal={this.closeLogModal}
                    />
                </Spin>
            </div>
        );
    }
}

export default Form.create()(Job);
