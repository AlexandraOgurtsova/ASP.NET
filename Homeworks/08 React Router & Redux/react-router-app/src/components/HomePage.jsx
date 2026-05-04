import { useSelector, useDispatch } from 'react-redux';
import { logout } from '../store/userSlice';
import {
  Container,
  Paper,
  Typography,
  Button,
  Box,
  Card,
  CardContent,
  Avatar
} from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';

const HomePage = () => {
  const userData = useSelector((state) => state.user.userData);
  const dispatch = useDispatch();

  return (
    <Container maxWidth="md">
      <Paper elevation={3} sx={{ p: 4, mt: 4 }}>
        <Typography variant="h3" component="h1" gutterBottom align="center">
          Welcome to Home Page
        </Typography>
        
        {userData && (
          <Card sx={{ maxWidth: 400, mx: 'auto', mt: 4 }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <Avatar sx={{ 
                width: 80, 
                height: 80, 
                mx: 'auto', 
                mb: 2, 
                bgcolor: 'primary.main' 
              }}>
                <PersonIcon sx={{ fontSize: 40 }} />
              </Avatar>
              <Typography variant="h6" gutterBottom>
                {userData.name}
              </Typography>
              <Typography variant="body1" color="text.secondary" gutterBottom>
                {userData.email}
              </Typography>
            </CardContent>
          </Card>
        )}
        
        <Box sx={{ textAlign: 'center', mt: 4 }}>
          <Button
            variant="contained"
            color="error"
            startIcon={<LogoutIcon />}
            onClick={() => dispatch(logout())}
            size="large"
          >
            Logout
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default HomePage;