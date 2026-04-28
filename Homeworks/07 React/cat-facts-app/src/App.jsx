import React, { useState } from 'react';
import axios from 'axios';
import CatFact from './components/CatFact';
import ErrorDisplay from './components/ErrorDisplay';
import './App.css';

function App() {
  const [catFact, setCatFact] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const fetchCatFact = async () => {
    setLoading(true);
    setCatFact('');
    setError('');
    
    try {
      const response = await axios.get('https://catfact.ninja/facts');   

      if (response.status >= 200 && response.status < 300) {
        const randomNumber = Math.floor(Math.random() * 9) + 0;
        const fact = response.data.data[randomNumber]?.fact || 'Факт не найден';
        setCatFact(fact);
      } else {
        setError(`Неожиданный статус ответа: ${response.status}`);
      }
    } catch (err) {
      if (err.response) {
        setError(`Ошибка сервера: ${err.response.status} - ${err.response.statusText}`);
      } else if (err.request) {
        setError('Ошибка сети: сервер не отвечает');
      } else {
        setError(`Ошибка запроса: ${err.message}`);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app">
      <h1>🐱 Факты о котах</h1>
      
      <button 
        onClick={fetchCatFact} 
        disabled={loading}
        className="fetch-button"
      >
        {loading ? 'Загрузка...' : 'Получить факт о котах'}
      </button>

      {catFact && <CatFact fact={catFact} />}
      {error && <ErrorDisplay message={error} />}
    </div>
  );
}

export default App;