import { HubConnectionBuilder } from "@microsoft/signalr";
import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "./AuthProvider";
import { useNotifications } from './context/NotificationContext';

const ChatSignalRContext = createContext({
  chatConnection: null,
  chatConnectionId: null,
  chatConnectionState: 'Disconnected'
});

export const ChatSignalRProvider = ({ children }) => {
  const [chatConnection, setChatConnection] = useState(null);
  const [chatConnectionId, setChatConnectionId] = useState(null);
  const [chatConnectionState, setChatConnectionState] = useState('Disconnected');
  const notificationsContext = useNotifications();
  const addNotification = notificationsContext?.addNotification || (() => {
    console.warn('NotificationProvider not available');
  });
  const { user } = useAuth();

  useEffect(() => {
    let newConnection;
    let isMounted = true;

    const createConnection = async () => {
      try {
        if (chatConnection) {
          await chatConnection.stop();
          if (isMounted) {
            setChatConnection(null);
            setChatConnectionId(null);
            setChatConnectionState('Disconnected');
          }
        }

        newConnection = new HubConnectionBuilder()
          .withUrl("https://localhost:7072/chats-hub", { 
            withCredentials: true,
            transport: 1 | 2, 
            accessTokenFactory: () => {
              return user?.token ? Promise.resolve(user.token) : Promise.resolve('');
            }
          })
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
              return retryContext.elapsedMilliseconds < 10000 ? 2000 : 5000;
            }
          })
          .build();

        newConnection.onclose(error => {
          if (isMounted) setChatConnectionState('Disconnected');
          console.log('Chat connection closed. Trying to reconnect...', error);
        });

        newConnection.onreconnecting(error => {
          if (isMounted) setChatConnectionState('Reconnecting');
          console.log('Chat reconnecting...', error);
        });

        newConnection.onreconnected(connectionId => {
          if (isMounted) {
            setChatConnectionState('Connected');
            setChatConnectionId(connectionId);
          }
          console.log('Chat reconnected with ID:', connectionId);
        });

        newConnection.on("ReceiveMessageNotification", (message) => {
          if (!isMounted) return;
          
          addNotification({
              chatId: message.chatId,
              content: message.content,
              senderName: message.senderNickName,
              senderAvatar: message.senderAvatarPath,
            });
        });

        if (isMounted) setChatConnectionState('Connecting');

        await newConnection.start();
        const id = await newConnection.invoke("GetConnectionId");

        if (isMounted) {
          setChatConnection(newConnection);
          setChatConnectionId(id);
          setChatConnectionState('Connected');
        }
        console.log("Chat SignalR connected. ID:", id);

      } catch (err) {
        if (isMounted) setChatConnectionState('Failed');
        console.error("Chat connection failed:", err);
        setTimeout(createConnection, 5000);
      }
    };

    createConnection();

    return () => {
      isMounted = false;
      if (newConnection) {
        newConnection.stop().catch(err => 
          console.log("Error while stopping chat connection:", err)
        );
      }
    };
  }, [user, addNotification]); 

  return (
    <ChatSignalRContext.Provider value={{ 
      chatConnection, 
      chatConnectionId, 
      chatConnectionState 
    }}>
      {children}
    </ChatSignalRContext.Provider>
  );
};

export const useChatSignalR = () => {
  const context = useContext(ChatSignalRContext);
  if (!context) {
    throw new Error('useChatSignalR must be used within a ChatSignalRProvider');
  }
  return context;
};