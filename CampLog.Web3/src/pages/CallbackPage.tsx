import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { LoadingScreen } from '../components/LoadingScreen';

export function CallbackPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!auth.isLoading && auth.isAuthenticated) {
      navigate('/trips', { replace: true });
    }
  }, [auth.isLoading, auth.isAuthenticated, navigate]);

  return <LoadingScreen />;
}
