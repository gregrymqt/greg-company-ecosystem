import { useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AppRoutes } from './routes/AppRoutes';
import { useAuth } from './features/auth/hooks/useAuth';
import { socketService } from './shared/services/socket.service';
import { AppHubsCSharp } from './shared/enums/hub/hub.enums';
import './App.css';

function App() {
  const { isAuthenticated } = useAuth();

  // Inicialização de Sockets para o Portal
  const initGlobalSockets = async () => {
    await socketService.connect(AppHubsCSharp.Payment);
    await socketService.connect(AppHubsCSharp.Video);
    await socketService.connect(AppHubsCSharp.Refund);
  };

  useEffect(() => {
    if (isAuthenticated) {
      initGlobalSockets();
    } else {
      socketService.disconnect();
    }
  }, [isAuthenticated]);

  return (
    <BrowserRouter>
      <div className="app-container">
        <AppRoutes />
      </div>
    </BrowserRouter>
  );
}

export default App;
