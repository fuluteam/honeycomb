/* eslint-disable react/jsx-closing-tag-location */
import React, { PureComponent } from 'react';
import { Modal, Form, Input, DatePicker, Select, Spin, Switch, message } from 'antd';
import moment from 'moment';
import 'moment/locale/zh-cn';
import jobNameValidator from './renderHelper';
import { MODE_EDIT, DTAE_FORMAT, DTAE_NOMAL_FORMAT } from '../../shared/constants';

moment.locale('zh-cn');
const { Option } = Select;
const { TextArea } = Input;

const formItemLayout = {
    labelCol: {
        xs: { span: 6 },
        sm: { span: 6 },
    },
    wrapperCol: {
        xs: { span: 16 },
        sm: { span: 16 },
    },
};

const urlRE = /(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?/;

function isObject(objLike) {
    return Object.prototype.toString.call(objLike) === '[object Object]';
}
@Form.create()
class JobModal extends PureComponent {
    componentDidUpdate(prevProps) {
        if (!prevProps.visible && this.props.visible && this.props.mode === MODE_EDIT) {
            this.setFormFields();
        }
    }

    setFormFields() {
        const { form, editJob } = this.props;
        const { scheduledJobs, command, appId, reason, ...formValues } = editJob;
        formValues.url = command.url;
        if (isObject(command.headers)) {
            formValues.headers = Object.entries(command.headers).reduce((arr, [k, v]) => {
                arr.push(`${k}:${v}`);
                return arr;
            }, []).join('\n');
        } else {
            formValues.headers = command.headers;
        }
        formValues.httpMethod = command.httpMethod;
        formValues.needAuth = command.needAuth;
        formValues.payloadJson = command.payloadJson;
        if (editJob.notBefore) {
            formValues.notBefore = moment(editJob.notBefore, DTAE_FORMAT);
        }
        if (editJob.expirationTime) {
            formValues.expirationTime = moment(editJob.expirationTime, DTAE_FORMAT);
        }
        form.setFieldsValue(formValues);
    }
    handleFormateDate = (d) => {
        return d && d.toDate().toISOString();
    };

    handleAfterClose = () => {
        const { form: { resetFields } } = this.props;
        resetFields();
    }

    handleSubmit = (e) => {
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        const {
            form,
            mode,
            appId,
            editJob,
            commitJob,
        } = this.props;
        if (mode === MODE_EDIT && !editJob) {
            message.error('获取编辑原数据失败，不能保存');
            return;
        }
        const { validateFields } = form;
        validateFields((err, values) => {
            if (err) {
                return;
            }
            const { url, httpMethod, payloadJson, needAuth, headers, ...payload } = values;
            if (payload.notBefore && payload.expirationTime && payload.notBefore > payload.expirationTime) {
                message.error('结束时间不能小于当前时间');
                return;
            }
            payload.notBefore = this.handleFormateDate(payload.notBefore);
            payload.expirationTime = this.handleFormateDate(payload.expirationTime);
            payload.command = {
                url,
                needAuth,
                httpMethod,
                payloadJson,
            };
            if (headers) {
                payload.command.headers = headers.split(/\n/).reduce((obj, item) => {
                    if (item.indexOf(':') > 0) {
                        const [k, v] = item.split(':');
                        obj[k.trim()] = v.trim();
                    }
                    return obj;
                }, {});
            }
            if (mode === MODE_EDIT) {
                payload.name = editJob.name; // name就是id
            }
            payload.appId = appId;
            commitJob(payload);
        });
    }

    validateUrl(_, value, callback) {
        if (value && !(urlRE).test(value)) {
            callback('URL格式不合法!');
        }
        callback();
    }

    render() {
        const {
            form,
            mode,
            loading,
            visible,
            toggleJobModalVisible,
        } = this.props;
        const {
            getFieldValue,
            setFieldsValue,
            getFieldDecorator,
        } = form;
        return (
            <Modal
                centered
                width={608}
                okText="完成"
                cancelText="取消"
                visible={visible}
                maskClosable={false}
                onOk={this.handleSubmit}
                onCancel={toggleJobModalVisible}
                afterClose={this.handleAfterClose}
                title={mode === MODE_EDIT ? '编辑' : '新建'}
                wrapClassName="job-manage-modal job-relate-modal"
            >
                <Spin spinning={loading}>
                    <Form onSubmit={this.handleSubmit} {...formItemLayout}>
                        <Form.Item label="key">
                            {getFieldDecorator('name', {
                                rules: [{
                                    required: true,
                                    message: '请输入任务名称!',
                                }, {
                                    validator: jobNameValidator,
                                }],
                            })(<Input disabled={mode === MODE_EDIT} />)}
                        </Form.Item>
                        <Form.Item label="任务名称">
                            {getFieldDecorator('displayName')(<Input />)}
                        </Form.Item>
                        <Form.Item
                            label="时间"
                            style={{
                                marginBottom: 0,
                            }}
                        >
                            <Form.Item className="job-time-item">
                                {getFieldDecorator('notBefore', {
                                })(<DatePicker
                                    showTime="timePicker"
                                    style={{ minWidth: 'inherit', maxWidth: '100%', width: '100%' }}
                                    format={DTAE_FORMAT}
                                    placeholder="开始时间"
                                    onChange={() => {
                                        setFieldsValue({
                                            expirationTime: null,
                                        });
                                    }}
                                />)}
                            </Form.Item>
                            <span
                                style={{
                                    display: 'inline-block',
                                    margin: '0px 6px 0 7px',
                                    textAlign: 'center',
                                }}
                            >~</span>
                            <Form.Item className="job-time-item">
                                {getFieldDecorator('expirationTime')(<DatePicker
                                    showTime="timePicker"
                                    style={{
                                        minWidth: 'inherit',
                                        maxWidth: '100%',
                                        width: '100%',
                                    }}
                                    format={DTAE_FORMAT}
                                    placeholder="结束时间"
                                    disabledDate={(curDate) => {
                                        const notBefore = getFieldValue('notBefore');
                                        if (notBefore && this.handleFormateDate(notBefore)) {
                                            if (notBefore.format(DTAE_NOMAL_FORMAT) === curDate.format(DTAE_NOMAL_FORMAT)) {
                                                return false;
                                            }
                                            return curDate < notBefore;
                                        }
                                        return false;
                                    }}
                                />)}
                            </Form.Item>
                        </Form.Item>
                        <Form.Item label="cron表达式">
                            {
                                getFieldDecorator('schedule', {
                                    rules: [{
                                        required: true,
                                        message: '请输入cron表达式!',
                                    }],
                                })(<Input style={{ width: '90%', borderRadius: '4px 0 0 4px' }} />)
                            }
                            <a
                                target="_blank"
                                rel="noopener noreferrer"
                                className="preview-link"
                                href="http://cron.qqe2.com/"
                            >
                                预览
                            </a>
                        </Form.Item>
                        <Form.Item label="URL">
                            {getFieldDecorator('url', {
                                rules: [{
                                    required: true,
                                    message: '请输入URL!',
                                }, {
                                    validator: this.validateUrl,
                                }],
                            })(<Input />)}
                        </Form.Item>
                        <Form.Item label="HttpMethod">
                            {getFieldDecorator('httpMethod', {
                                initialValue: 'GET',
                            })(<Select>
                                <Option value="POST">POST</Option>
                                <Option value="GET">GET</Option>
                            </Select>)}
                        </Form.Item>
                        <Form.Item label="Headers">
                            {getFieldDecorator('headers')(<Input.TextArea placeholder="键值对形式，多个请求头请换行" rows={5} />)}
                        </Form.Item>
                        <Form.Item label="RequestBody">
                            {getFieldDecorator('payloadJson')(<TextArea />)}
                        </Form.Item>
                        <Form.Item label="是否暂停">
                            {getFieldDecorator('suspend', { valuePropName: 'checked' })(<Switch />)}
                        </Form.Item>
                    </Form>
                </Spin>
            </Modal>
        );
    }
}

export default JobModal;
