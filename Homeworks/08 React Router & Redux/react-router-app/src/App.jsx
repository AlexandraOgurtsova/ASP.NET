import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Login from './components/Login';
import Register from './components/Register';
import HomePage from './components/HomePage';
import NotFound from './components/NotFound';
import withAuth from './hoc/withAuth';

// Применяем HOC для защищенных маршрутов
const ProtectedHomePage = withAuth(HomePage);
const LoginPage = withAuth(Login, false); // Защита от авторизованных пользователей
const RegisterPage = withAuth(Register, false); // Защита от авторизованных пользователей

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<ProtectedHomePage />} />
          <Route path="login" element={<LoginPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;