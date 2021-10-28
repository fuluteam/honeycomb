import React, { useState, useEffect } from 'react';
import { useStore } from 'react-redux';

function AsyncComponent({ deps, children, ...rest }) {
    const store = useStore();
    const [modelLoaded, setModelLoaded] = useState(!Array.isArray(deps) && deps.length === 0);
    useEffect(() => {
        if(!modelLoaded) {
            Promise.all(deps.map((dep) => runImportTask(dep))).then(() => {
                setModelLoaded(true);
            });
        }
    }, []);
    function runImportTask(dep) {
        if (!store.getState().hasOwnProperty(dep)) {
            return new Promise((resolve, reject) => {
                import(`models/${dep}.js`).then((module) => {
                    const { namespace, state: initalState = {}, effects } = module.default;
                    store.dispatch({
                        type: '@@redux/register',
                        payload: {
                            effects,
                            initalState,
                            namespace: namespace || dep,
                        }
                    });
                    resolve();
                }).catch(reject);
            });
        }
    }
    if (modelLoaded) {
        return (
            <React.Fragment>
                {React.createElement(children, rest)}
            </React.Fragment>
        );
    }
    return null;
}

export default AsyncComponent;
