import { useEffect, useState } from 'react';

const Login = () => {
  const [tenant, setTenant] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [status, setStatus] = useState('');

  const login = async () => {
    const returnUrl = new URLSearchParams(window.location.search).get('ReturnUrl');
    const response = await fetch('/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password, tenant, returnUrl: returnUrl || '/account' }),
    });

    if (response.status !== 200) {
      setStatus('Login failed: ' + response.statusText);
      return;
    }

    const data = await response.json();
    if (data?.redirectUrl) {
      window.location.href = data.redirectUrl;
    }
  };

  useEffect(() => {
    const returnUrl = new URLSearchParams(window.location.search).get('ReturnUrl');
    // if (!returnUrl) {
    //   window.location.assign('/error?message=invalid login request');
    //   return;
    // }
    const urlTenant = new URLSearchParams(window.location.search).get('tenant');
    const acrValues = new URLSearchParams(decodeURI(returnUrl!)).get('acr_values');
    const acrTenant = acrValues?.toLowerCase().replace('tenant:', '').split(' ')[0];
    const tenant = urlTenant || acrTenant;
    if (!tenant) {
      window.location.assign('/error?message=tenant is missing');
      return;
    }
    setTenant(tenant);
  }, []);

  return (
    <form onSubmit={(e) => e.preventDefault()} method="POST">
      <span style={{ color: 'white' }}>Signing into {tenant}</span>
      <div>
        <input
          autoFocus
          type="text"
          name="username"
          id="username"
          value={username}
          autoComplete="username"
          onChange={(e) => setUsername(e.target.value)}
        />
      </div>
      <div>
        <input
          type="password"
          name="password"
          id="password"
          value={password}
          autoComplete="current-password"
          onChange={(e) => setPassword(e.target.value)}
        />
      </div>
      <div style={{ marginTop: '8px' }}>
        <button onClick={login}>Login</button>
      </div>
      <div style={{ marginTop: '12px' }}>
        <span style={{ color: 'orange' }}>{status}</span>
      </div>
    </form>
  );
};

export default Login;
