import { Switch, Route } from 'react-router-dom';
import Job from './pages/Job';
import History from './pages/History';

function Router() {
    return (
        <Switch>
            <Route exact path="/">
                <Job />
            </Route>
            <Route path="/history">
                <History />
            </Route>
        </Switch>
    );
}

export default Router;