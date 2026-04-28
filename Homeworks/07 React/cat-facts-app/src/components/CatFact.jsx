import React from 'react';
import './CatFact.css';

const CatFact = ({ fact }) => {
  return (
    <div className="cat-fact">
      <h3>🐱 Интересный факт о котах:</h3>
      <p>{fact}</p>
    </div>
  );
};

export default CatFact;