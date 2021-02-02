import { useEffect, useState } from 'react';

const Logout = () => {
  const [iframe, setIframe] = useState('');

  useEffect(() => {
    const logoutId = new URLSearchParams(window.location.search).get('logoutId');
    (async () => {
      const response = await fetch(`/auth/logout?logoutId=${logoutId}`);
      const data = await response.json();

      if (data.iframeUrl) {
        setIframe(data.iframeUrl);
      }

      window.location = data.redirectUrl || '/';
    })();
  }, []);
  return iframe ? <iframe title="logout" width={0} height={0} src={iframe}></iframe> : <></>;
};

export default Logout;
