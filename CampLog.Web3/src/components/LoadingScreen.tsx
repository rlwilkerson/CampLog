import { Box, CircularProgress, Typography } from '@mui/material';
import LuggageIcon from '@mui/icons-material/Luggage';

export function LoadingScreen() {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        bgcolor: 'background.default',
        gap: 2,
      }}
    >
      <LuggageIcon sx={{ fontSize: 48, color: 'primary.main' }} />
      <Typography variant="h6" color="text.secondary">Loading CampLog...</Typography>
      <CircularProgress color="primary" size={32} />
    </Box>
  );
}
