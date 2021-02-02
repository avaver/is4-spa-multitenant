import { useEffect, useState } from 'react';
import useWebAuthn from './webAuthn/useWebAuthn';
import { arr2b64url } from './webAuthn/utils';

const Login = () => {
  const { requestCredential } = useWebAuthn();

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
    if (data?.keys?.length > 0) {
      await requestCredential(
        arr2b64url(crypto.getRandomValues(new Uint8Array(32))),
        'discouraged',
        undefined,
        data.keys
      );
    }
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
    <div className="text-center content-center-outer">
      <div className="content-center-inner" style={{ minWidth: '300px' }}>
        <form onSubmit={(e) => e.preventDefault()} method="POST">
          <h1 className="h2 mb-5">Welcome to {tenant}</h1>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="64"
            height="64"
            fill="currentColor"
            className="bi bi-file-earmark-person mb-3"
            viewBox="0 0 16 16"
          >
            <path d="M11 8a3 3 0 1 1-6 0 3 3 0 0 1 6 0z" />
            <path d="M14 14V4.5L9.5 0H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2zM9.5 3A1.5 1.5 0 0 0 11 4.5h2v9.255S12 12 8 12s-5 1.755-5 1.755V2a1 1 0 0 1 1-1h5.5v2z" />
          </svg>
          <h1 className="h3 mb-3 fw-normal">Please sign in</h1>
          <input
            autoFocus
            required
            className="form-control"
            placeholder="Username"
            type="text"
            name="username"
            id="username"
            value={username}
            autoComplete="username"
            onChange={(e) => setUsername(e.target.value)}
          />
          <input
            required
            className="form-control mb-3"
            placeholder="Password"
            type="password"
            name="password"
            id="password"
            value={password}
            autoComplete="current-password"
            onChange={(e) => setPassword(e.target.value)}
          />
          <button onClick={login} className="btn btn-lg btn-primary w-100">
            Sign in
          </button>
          <div style={{ marginTop: '12px' }}>
            <span style={{ color: 'orange' }}>{status}</span>
          </div>
          <p className="mt-5 muted">© Plandent 2021</p>
        </form>
      </div>
    </div>
  );
};

export default Login;
