// index.js (or the main entry point)
import React from 'react';
import ReactDOM from 'react-dom/client'; // React 18 uses `createRoot`
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

const root = ReactDOM.createRoot(document.getElementById('root'));

root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

// Report web vitals
reportWebVitals();
