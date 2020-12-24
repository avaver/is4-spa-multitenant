import { useEffect, useState } from 'react';
import './App.css';

const Login = () => {
  const [tenant, setTenant] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const login = async () => {
    const returnUrl = new URLSearchParams(window.location.search).get('ReturnUrl');
    const response = await fetch('/api/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password, returnUrl })
    });

    const data = await response.json();
    if (data && data.ok) {
      window.location = data.redirectUrl;
    }
  }

  useEffect(() => {
    const returnUrl = new URLSearchParams(window.location.search).get('ReturnUrl');
    if (!returnUrl) {
      window.location.assign('/error?message=invalid login request');
    } 
    const acrValues = new URLSearchParams(decodeURI(returnUrl!)).get('acr_values');
    const acrTenant = acrValues?.toLowerCase().replace('tenant:', '').split(' ')[0];
    if (!acrTenant) {
      window.location.assign('/error?message=tenant is missing');
    }
    setTenant(acrTenant!);
  }, []);

  return (
  <div className="App">
    <header className="App-header">
      <span style={{color: 'white'}}>Signing into {tenant}</span>
      <div>
        <input type="text" name="username" id="username" value={username} onChange={(e) => setUsername(e.target.value)} />
      </div>
      <div>
        <input type="password" name="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      </div>
      <div>
        <button onClick={login}>Login</button>
      </div>
  </header>
</div>)
}

export default Login;