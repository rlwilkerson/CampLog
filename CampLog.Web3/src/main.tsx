import React from 'react';
import ReactDOM from 'react-dom/client';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from 'react-oidc-context';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import { theme } from './theme';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
});

const oidcConfig = {
  authority: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'camplog-web',
  redirect_uri: window.location.origin + '/callback',
  post_logout_redirect_uri: window.location.origin,
  scope: 'openid profile email',
};

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider {...oidcConfig}>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <App />
          </ThemeProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </AuthProvider>
  </React.StrictMode>
);
