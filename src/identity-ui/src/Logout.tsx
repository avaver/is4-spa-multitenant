import { useEffect, useState } from 'react';

const Logout = () => {
  const [iframe, setIframe] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const logoutId = new URLSearchParams(window.location.search).get('logoutId');
    (async () => {
      const response = await fetch(`/auth/logout?logoutId=${logoutId}`);
      const data = await response.json();

      if (data.iframeUrl) {
        setIframe(data.iframeUrl);
      }

      if (data.redirectUrl) {
        window.location = data.redirectUrl;
      } else {
        setMessage('logged out.');
      }
    })();
  }, []);
  return iframe ? <iframe title="logout" width={0} height={0} src={iframe}></iframe> : <div>{message}</div>;
};

export default Logout;
