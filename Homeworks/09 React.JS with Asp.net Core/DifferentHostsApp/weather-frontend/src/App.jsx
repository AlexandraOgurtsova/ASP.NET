import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [forecasts, setForecasts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchWeather = async () => {
      try {
        const apiUrl = 'https://localhost:7245/weatherforecast'
        
        console.log('Отправляем запрос к:', apiUrl)
        
        const response = await fetch(apiUrl)
        
        if (!response.ok) {
          throw new Error(`Ошибка HTTP: ${response.status} ${response.statusText}`)
        }
        
        const data = await response.json()
        console.log('Получены данные:', data)
        setForecasts(data)
      } catch (err) {
        console.error('Ошибка при загрузке погоды:', err)
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }

    fetchWeather()
  }, [])

  if (loading) {
    return (
      <div className="container">
        <h1>Прогноз погоды</h1>
        <p>⏳ Загрузка данных...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="container">
        <h1>Прогноз погоды</h1>
        <div className="error-box">
          <p>❌ Ошибка при загрузке данных:</p>
          <p className="error-message">{error}</p>
          <p>Проверьте, что бекенд запущен и CORS настроен правильно.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="container">
      <h1>Прогноз погоды</h1>
      <table>
        <thead>
          <tr>
            <th>Дата</th>
            <th>Температура (°C)</th>
            <th>Температура (°F)</th>
            <th>Описание</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map((forecast, index) => (
            <tr key={index}>
              <td>{forecast.date}</td>
              <td>{forecast.temperatureC}</td>
              <td>{forecast.temperatureF}</td>
              <td>{forecast.summary}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default App