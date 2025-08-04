import ReactDOM from 'react-dom/client';
import App from './App';
import 'bootstrap/dist/css/bootstrap.min.css';
import "./Styles/Index.css";
import { AuthProvider } from "./AuthProvider"; 
import { SignalRProvider } from './SignalRProvider';
import { ChatSignalRProvider } from './ChatSignalRProvider'; 
import { BrowserRouter } from 'react-router-dom';
import { NotificationProvider } from './context/NotificationContext';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <BrowserRouter>
    <AuthProvider>
      <NotificationProvider> 
        <SignalRProvider>
          <ChatSignalRProvider> 
            <App />
          </ChatSignalRProvider>
        </SignalRProvider>
      </NotificationProvider>
    </AuthProvider>
  </BrowserRouter>
);
