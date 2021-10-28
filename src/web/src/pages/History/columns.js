import React from 'react';
import { renderDate } from '../Job/renderHelper';

const columns = [{
    title: '序号',
    dataIndex: 'sortIndex',
    width: 60,
}, {
    title: '名称',
    dataIndex: 'name',
}, {
    title: '设定执行时间',
    dataIndex: 'schedule',
    render: renderDate,
}, {
    title: '开始执行时间',
    dataIndex: 'startedAt',
    render: renderDate,
}, {
    title: '结束执行时间',
    dataIndex: 'finishedAt',
    render: renderDate,
}, {
    title: '错误详情',
    dataIndex: 'reason',
}, {
    title: 'URL',
    width: 220,
    dataIndex: 'command',
    render({ url }) {
        return (
            <span
                title={url}
                className="nowrap"
                style={{ width: '200px' }}
            >
                {url}
            </span>
        );
    },
}, {
    title: '运行状态',
    dataIndex: 'status',
    width: 100,
}, {
    title: '操作',
    width: 50,
}];

export default columns;
