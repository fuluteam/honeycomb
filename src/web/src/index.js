import "core-js/stable";
import ReactDOM from 'react-dom';
import { Layout, Menu } from 'antd';
import "regenerator-runtime/runtime";
import AppLauncher from '@/AppLauncher';
import { HashRouter, Link } from 'react-router-dom';
import Router from './Router';

const { Content, Sider, Header } = Layout;

function App() {
    return (
        <AppLauncher>
            <HashRouter>
                <Layout>
                    <Header>
                        <h2 style={{ color: '#fff', marginLeft: '-28px' }}>蜂巢-任务调度管理系统</h2>
                    </Header>
                    <Layout>
                        <Sider
                            style={{
                                background: '#fff',
                            }}
                        >
                            <Menu
                                theme="light"
                                mode="inline"
                                defaultSelectedKeys={[location.hash.slice(1)]}
                            >
                                <Menu.Item key="/">
                                    <Link to="/">作业管理</Link>
                                </Menu.Item>
                                <Menu.Item key="/history">
                                    <Link to="/history">历史记录</Link>
                                </Menu.Item>
                            </Menu>
                        </Sider>
                        <Layout>
                            <Content
                                style={{
                                    margin: '15px',
                                    padding: '10px',
                                    background: '#fff',
                                }}
                            >
                                <Router />
                            </Content>
                        </Layout>
                    </Layout>
                </Layout>
            </HashRouter>
        </AppLauncher>
    );
}

ReactDOM.render(<App />, document.querySelector('#root'));