import React from 'react';
import ReactDOM from 'react-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';
// import './index.css';
// import './App.css';
// import reportWebVitals from './reportWebVitals';
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

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
// reportWebVitals();
