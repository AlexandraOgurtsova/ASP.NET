import { Outlet, Link as RouterLink } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button,
  Container,
  Box
} from '@mui/material';

const Layout = () => {
  const isAuthenticated = useSelector((state) => state.user.isAuthenticated);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="static">
        <Toolbar>
          <Typography 
            variant="h6" 
            component={RouterLink} 
            to="/" 
            sx={{ flexGrow: 1, textDecoration: 'none', color: 'white' }}
          >
            My App
          </Typography>
          <Box>
            {!isAuthenticated ? (
              <>
                <Button color="inherit" component={RouterLink} to="/login">
                  Login
                </Button>
                <Button color="inherit" component={RouterLink} to="/register">
                  Register
                </Button>
              </>
            ) : (
              <Button color="inherit" component={RouterLink} to="/">
                Home
              </Button>
            )}
          </Box>
        </Toolbar>
      </AppBar>
      <Container component="main" sx={{ mt: 4, mb: 4, flex: 1 }}>
        <Outlet />
      </Container>
    </Box>
  );
};

export default Layout;