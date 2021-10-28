/* eslint-disable no-unsafe-finally */
import { message } from 'antd';

function excuteFunc(callback, ...args) {
    if (typeof callback === 'function') {
        return callback(...args);
    }
    return null;
}

function initMethod(instance) {
    return async function callServiceMethod(payload, methodName, successCallBack, errorCallBack, finallyCallBack) {
        let response;
        try {
            response = await instance[methodName](payload);
            if (response.errors) {
                message.error(response.title);
                excuteFunc(errorCallBack, response);
            } else {
                excuteFunc(successCallBack, response);
            }
        } catch (error) {
            message.error(error.response.data ? error.response.data.title : error.message);
            excuteFunc(errorCallBack, error);
        } finally {
            excuteFunc(finallyCallBack);
            return response;
        }
    };
}

function isEmptyObject(obj) {
    if (!obj) {
        return true;
    }
    return Object.keys(obj).length === 0;
}

export {
    isEmptyObject,
};

export default initMethod;
