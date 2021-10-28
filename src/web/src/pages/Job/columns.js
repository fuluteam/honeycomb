import React from 'react';
import { renderDate } from './renderHelper';

const columns = [{
    width: 60,
    title: '序号',
    dataIndex: 'sortIndex',
}, {
    title: '名称',
    dataIndex: 'displayName',
}, {
    title: '开始时间',
    dataIndex: 'notBefore',
    render: renderDate,
}, {
    title: '结束时间',
    dataIndex: 'expirationTime',
    render: renderDate,
}, {
    title: 'cron表达式',
    dataIndex: 'schedule',
}, {
    title: 'URL',
    dataIndex: 'url',
    width: 220,
    render(_, { command }) {
        return (
            <span
                className="nowrap"
                title={command.url}
                style={{ width: '200px' }}
            >
                {command.url}
            </span>
        );
    },
}, {
    title: '运行状态',
    dataIndex: 'suspend',
    width: 100,
    render: (suspend) => {
        if (suspend) {
            return (<span className="status-cell status-paused">暂停</span>);
        }
        return (<span className="status-cell status-normal">运行中</span>);
    },
}, {
    width: 150,
    title: '操作',
    dataIndex: 'op',
}];

export default columns;
