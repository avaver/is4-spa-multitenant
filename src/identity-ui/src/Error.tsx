import { useEffect, useState } from 'react';
import './App.css';

const Error = () => {
  const [message, setMessage] = useState('unknown error');

  useEffect(() => {
    const errorId = new URLSearchParams(window.location.search).get('errorId');
    const urlMessage = new URLSearchParams(window.location.search).get('message')
    if (errorId) {
      (async () => {
        const response = await fetch('/auth/errorredirect?errorId=' + errorId);
        const data = await response.json();
        setMessage(data.message);
      })()
    } else if (urlMessage) {
      setMessage(urlMessage);
    }
  }, []);
  return (
    <div className="App">
      <header className="App-header">
        <div>Error: {message}</div>
      </header>
    </div>);
}

export default Error;