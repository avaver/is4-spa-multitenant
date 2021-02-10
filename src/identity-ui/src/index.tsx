import React from 'react';
import ReactDOM from 'react-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import Login from './Login';
import Logout from './Logout';
import Error from './Error';
import Account from './Account';
import Home from './Home';

ReactDOM.render(
  <React.StrictMode>
    <Router>
      <Switch>
        <Route path="/login" exact component={Login}></Route>
        <Route path="/logout" exact component={Logout}></Route>
        <Route path="/error" exact component={Error}></Route>
        <Route path="/account" exact component={Account}></Route>
        <Route path="/" exact component={Home}></Route>
      </Switch>
    </Router>
  </React.StrictMode>,
  document.getElementById('root')
);
