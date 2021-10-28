import React from 'react';
import { connect } from 'react-redux';
import AsyncComponent from './AsyncComponent';

let rawDispatch = null;
function proxyDipsatch(arg) {
    return new Promise((resolve, reject) => {
        arg.promiseHook = {
            resolve,
            reject
        };
        rawDispatch(arg);
    });
}

function model(...deps) {
    return function wrapComponent(target) {
        const cacheRender = connect(function mapStateToProps(state) {
            return deps.reduce((mapState, dep) => {
                mapState[dep] = state[dep];
                return mapState;
            }, {});
            
        }, null, (stateProps, dispatchProps, ownProps) => {
            if (!rawDispatch) {
                rawDispatch = dispatchProps.dispatch;
            }
            return { ...ownProps, ...stateProps, ...dispatchProps, dispatch: proxyDipsatch };
        })(target);
        return (props) => {
            return (
                <AsyncComponent
                    deps={deps}
                    {...props}
                >
                    {cacheRender}
                </AsyncComponent>
            )
        };
    }
}

export default model;
