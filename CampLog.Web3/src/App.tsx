import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useEffect } from 'react';
import { setApiToken } from './api/client';
import { AppShell } from './components/AppShell';
import { TripsPage } from './pages/TripsPage';
import { TripDetailPage } from './pages/TripDetailPage';
import { CallbackPage } from './pages/CallbackPage';
import { LoadingScreen } from './components/LoadingScreen';

export default function App() {
  const auth = useAuth();

  useEffect(() => {
    setApiToken(auth.user?.access_token ?? null);
  }, [auth.user]);

  if (auth.isLoading) return <LoadingScreen />;

  if (!auth.isAuthenticated) {
    auth.signinRedirect();
    return <LoadingScreen />;
  }

  return (
    <Routes>
      <Route path="/callback" element={<CallbackPage />} />
      <Route element={<AppShell />}>
        <Route path="/" element={<Navigate to="/trips" replace />} />
        <Route path="/trips" element={<TripsPage />} />
        <Route path="/trips/:tripId" element={<TripDetailPage />} />
      </Route>
    </Routes>
  );
}
