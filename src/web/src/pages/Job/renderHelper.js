import React from 'react';
import moment from 'moment';
import { DTAE_FORMAT } from '../../shared/constants';

const nameRE = /^[a-z0-9-]+$/;

function jobNameValidator(_, value, callback) {
    if (value && !(nameRE).test(value)) {
        return callback('名称只允许包含 小写字母、数字、短线');
    }
    return callback();
}


function renderDate(dateISOString) {
    if (dateISOString) {
        return (
            <span>
                {
                    moment(dateISOString).format(DTAE_FORMAT)
                }
            </span>
        );
    }
    return null;
}

export { renderDate };
export default jobNameValidator;
