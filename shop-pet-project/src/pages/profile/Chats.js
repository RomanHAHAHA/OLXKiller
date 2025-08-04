import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../AuthProvider';
import { API_BASE_URL } from '../../apiConfig';
import styles from '../../Styles/ChatList.module.css';
import avatarPlaceholder from "../../asserts/default_avatar_image.png";
import { useChatSignalR } from "../../ChatSignalRProvider";

const Chats = () => {
  const { user } = useAuth();
  const { chatConnection } = useChatSignalR();
  const navigate = useNavigate();
  const [chats, setChats] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [muting, setMuting] = useState({});

  const fetchChats = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}chats-api/api/chats/my`, { 
        credentials: 'include' 
      });

      if (!response.ok) throw new Error('Failed to fetch chats');
      
      const data = await response.json();
      setChats(data.data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleUserStatusChange = useCallback((userId, isOnline) => {
    setChats(prevChats => prevChats.map(chat => 
      chat.otherUser.id === userId
        ? { 
            ...chat, 
            otherUser: { 
              ...chat.otherUser, 
              isOnline,
              lastOnlineAt: isOnline ? null : new Date().toISOString() 
            } 
          }
        : chat
    ));
  }, []);

  useEffect(() => {
    if (!chatConnection) return;

    const handleUserOnline = (userId) => handleUserStatusChange(userId, true);
    const handleUserOffline = (userId) => handleUserStatusChange(userId, false);

    chatConnection.on("UserOnline", handleUserOnline);
    chatConnection.on("UserOffline", handleUserOffline);

    return () => {
      chatConnection.off("UserOnline", handleUserOnline);
      chatConnection.off("UserOffline", handleUserOffline);
    };
  }, [chatConnection, handleUserStatusChange]);

  const toggleMute = async (userId, isCurrentlyMuted, e) => {
    e.stopPropagation();
    try {
      setMuting(prev => ({ ...prev, [userId]: true }));
            setChats(prevChats => prevChats.map(chat => 
        chat.otherUser.id === userId 
          ? { 
              ...chat, 
              isMuted: !isCurrentlyMuted,
              otherUser: chat.otherUser 
            } 
          : chat
      ));

      const endpoint = isCurrentlyMuted ? 'unmute' : 'mute';
      const response = await fetch(
        `${API_BASE_URL}chats-api/api/users/${userId}/${endpoint}`, 
        {
          method: 'POST',
          credentials: 'include'
        }
      );

      if (!response.ok) throw new Error(`Failed to ${endpoint} user`);
      
    } catch (err) {
      setError(err.message);
      setChats(prevChats => prevChats.map(chat => 
        chat.otherUser.id === userId 
          ? { ...chat, isMuted: isCurrentlyMuted } 
          : chat
      ));
    } finally {
      setMuting(prev => ({ ...prev, [userId]: false }));
    }
  };

  const formatLastOnline = (lastOnlineAt) => {
    if (!lastOnlineAt) return 'Online';
    
    const now = new Date();
    const lastOnline = new Date(lastOnlineAt);
    const diffInMinutes = Math.floor((now - lastOnline) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes} min ago`;
    if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)} hours ago`;
    
    return lastOnline.toLocaleDateString();
  };

  useEffect(() => {
    if (user) fetchChats();
  }, [user]);

  if (loading) return <div className={styles.loading}>Loading chats...</div>;
  if (error) return <div className={styles.error}>{error}</div>;

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Your Chats</h2>
      
      <div className={styles.chatList}>
        {chats.length === 0 ? (
          <div className={styles.noChats}>You don't have any chats yet</div>
        ) : (
          chats.map(chat => (
            <div 
              key={chat.id} 
              className={styles.chatItem}
              onClick={() => navigate(chat.id)}
            >
              <div className={styles.avatarContainer}>
                <img 
                  src={chat.otherUser.avatarImageName ? 
                    `${API_BASE_URL}user-images/${chat.otherUser.avatarImageName}` : 
                    avatarPlaceholder} 
                  alt={chat.otherUser.nickName}
                  className={styles.avatar}
                />
                <div className={`${styles.statusIndicator} ${
                  chat.otherUser.isOnline ? styles.online : styles.offline
                }`}></div>
                
                {chat.unreadMessages > 0 && (
                  <span className={styles.unreadBadge}>{chat.unreadMessages}</span>
                )}
              </div>
              
              <div className={styles.chatInfo}>
                <div className={styles.userName}>
                  {chat.otherUser.nickName}
                  <span className={`${styles.statusText} ${
                    chat.otherUser.isOnline ? styles.online : ''
                  }`}>
                    {chat.otherUser.isOnline 
                      ? 'Online' 
                      : `Last seen ${formatLastOnline(chat.otherUser.lastOnlineAt)}`
                    }
                  </span>
                </div>
                <div className={styles.lastMessageTime}>
                  {new Date(chat.lastMessageSentAt).toLocaleString()}
                </div>
              </div>

              <button 
                className={`${styles.chatActions} ${chat.isMuted ? styles.muted : ''} ${muting[chat.otherUser.id] ? styles.muting : ''}`}
                onClick={(e) => toggleMute(chat.otherUser.id, chat.isMuted, e)}
                disabled={muting[chat.otherUser.id]}
                aria-label={chat.isMuted ? "Unmute notifications" : "Mute notifications"}
              >
                {muting[chat.otherUser.id] ? (
                  <div className={styles.spinner}></div>
                ) : (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
                    {chat.isMuted ? (
                      <path d="M3 3L21 21M12 6V4M12 20V18M8 14C8 16.209 9.791 18 12 18C14.209 18 16 16.209 16 14V10C16 7.791 14.209 6 12 6C11.113 6 10.295 6.292 9.625 6.783" 
                        stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                    ) : (
                      <path d="M12 6V18M9 9L5 5M15 9L19 5M8 14C8 16.209 9.791 18 12 18C14.209 18 16 16.209 16 14V10C16 7.791 14.209 6 12 6C9.791 6 8 7.791 8 10V14Z" 
                        stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                    )}
                  </svg>
                )}
              </button>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Chats;