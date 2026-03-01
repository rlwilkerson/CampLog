import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#DC7147',        // terracotta
      light: '#E3AA99',       // warm blush
      dark: '#B85A36',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#D8A748',        // amber
      light: '#E8C078',
      dark: '#B8873A',
      contrastText: '#ffffff',
    },
    background: {
      default: '#faf7f4',     // warm paper
      paper: '#fffdfb',
    },
    text: {
      primary: '#4e3025',     // ink
      secondary: '#7a5446',
    },
    error: {
      main: '#d32f2f',
    },
  },
  typography: {
    fontFamily: '"Inter", "Segoe UI", system-ui, sans-serif',
    h1: { fontWeight: 700, letterSpacing: '-0.02em' },
    h2: { fontWeight: 700, letterSpacing: '-0.01em' },
    h3: { fontWeight: 600 },
    h4: { fontWeight: 600 },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
    body1: { lineHeight: 1.6 },
    body2: { lineHeight: 1.5 },
  },
  shape: {
    borderRadius: 12,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          borderRadius: 8,
          padding: '10px 20px',
        },
        containedPrimary: {
          boxShadow: '0 2px 8px rgba(220, 113, 71, 0.3)',
          '&:hover': {
            boxShadow: '0 4px 12px rgba(220, 113, 71, 0.4)',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 16,
          boxShadow: '0 1px 4px rgba(78, 48, 37, 0.08), 0 2px 12px rgba(78, 48, 37, 0.06)',
          border: '1px solid rgba(78, 48, 37, 0.06)',
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          boxShadow: '0 1px 0 rgba(78, 48, 37, 0.08)',
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        },
      },
    },
    MuiFab: {
      styleOverrides: {
        root: {
          boxShadow: '0 4px 16px rgba(220, 113, 71, 0.4)',
        },
      },
    },
  },
});
