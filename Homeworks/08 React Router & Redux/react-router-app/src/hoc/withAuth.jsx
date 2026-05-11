import { useSelector } from 'react-redux';
import { Navigate } from 'react-router-dom';

const withAuth = (WrappedComponent, requireAuth = true) => {
  return function AuthComponent(props) {
    const isAuthenticated = useSelector((state) => state.user.isAuthenticated);

    if (requireAuth && !isAuthenticated) {
      return <Navigate to="/login" replace />;
    }

    if (!requireAuth && isAuthenticated) {
      return <Navigate to="/" replace />;
    }

    return <WrappedComponent {...props} />;
  };
};

export default withAuth;