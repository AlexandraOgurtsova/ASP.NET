import React from 'react';
import './ErrorDisplay.css';

const ErrorDisplay = ({ message }) => {
  return (
    <div className="error-display">
      <h3>❌ Произошла ошибка:</h3>
      <p>{message}</p>
    </div>
  );
};

export default ErrorDisplay;