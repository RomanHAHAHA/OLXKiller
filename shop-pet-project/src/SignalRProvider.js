import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "./AuthProvider";

const SignalRContext = createContext({
  connection: null,
  connectionId: null,
  connectionState: 'Disconnected'
});

export const SignalRProvider = ({ children }) => {
  const [connection, setConnection] = useState(null);
  const [connectionId, setConnectionId] = useState(null);
  const [connectionState, setConnectionState] = useState('Disconnected');
  const { user } = useAuth();

  useEffect(() => {
    let newConnection;
    let isMounted = true;

    const createConnection = async () => {
      try {
        if (connection) {
          await connection.stop();
          if (isMounted) {
            setConnection(null);
            setConnectionId(null);
            setConnectionState('Disconnected');
          }
        }

        newConnection = new HubConnectionBuilder()
          .withUrl("https://localhost:7072/notifications-hub", { 
            withCredentials: true,
            transport: 1 | 2, 
            accessTokenFactory: () => {
              return user?.token ? Promise.resolve(user.token) : Promise.resolve('');
            }
          })
          .configureLogging(LogLevel.Warning)
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
              return retryContext.elapsedMilliseconds < 10000 ? 2000 : 5000;
            }
          })
          .build();

        newConnection.onclose(error => {
          if (isMounted) setConnectionState('Disconnected');
          console.log('Connection closed. Trying to reconnect...', error);
        });

        newConnection.onreconnecting(error => {
          if (isMounted) setConnectionState('Reconnecting');
          console.log('Reconnecting...', error);
        });

        newConnection.onreconnected(connectionId => {
          if (isMounted) {
            setConnectionState('Connected');
            setConnectionId(connectionId);
          }
          console.log('Reconnected with ID:', connectionId);
        });

        if (isMounted) setConnectionState('Connecting');

        await newConnection.start();
        const id = await newConnection.invoke("GetConnectionId");

        if (isMounted) {
          setConnection(newConnection);
          setConnectionId(id);
          setConnectionState('Connected');
        }
        console.log("SignalR connected. ID:", id);

      } catch (err) {
        if (isMounted) setConnectionState('Failed');
        console.error("Connection failed:", err);
        setTimeout(createConnection, 5000);
      }
    };

    createConnection();

    return () => {
      isMounted = false;
      if (newConnection) {
        newConnection.stop().catch(err => 
          console.log("Error while stopping connection:", err)
        );
      }
    };
  }, [user]); 

  return (
    <SignalRContext.Provider value={{ connection, connectionId, connectionState }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalR must be used within a SignalRProvider');
  }
  return context;
};