import { useEffect, useState } from 'react';

const Account = () => {
  const [info, setInfo] = useState('');
  useEffect(() => {
    (async () => {
      const response = await fetch('/account/authcookieclaims');
      if (response.redirected) {
        window.location.href = response.url;
      } else {
        setInfo(response.status !== 200 ? response.statusText : await response.text());
      }
    })();
  }, []);
  return (
    <>
      <div>Account Page</div>
      <pre style={{ textAlign: 'left' }}>{info}</pre>
    </>
  );
};

export default Account;
