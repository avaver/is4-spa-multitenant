import './App.css';

const Error = () => {
  return (
    <div className="App">
      <header className="App-header">
        <div>Error: {new URLSearchParams(window.location.search).get('message')}</div>
      </header>
    </div>);
}

export default Error;