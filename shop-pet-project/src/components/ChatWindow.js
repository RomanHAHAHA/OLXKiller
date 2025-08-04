import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthProvider';
import { useChatApi } from '../hooks/useChatApi';
import styles from '../Styles/ChatWindow.module.css';
import avatarPlaceholder from "../asserts/default_avatar_image.png";
import { useChatSignalR } from "../ChatSignalRProvider";
import Message from "./Message.js";
import { HubConnectionState } from "@microsoft/signalr";
import { API_BASE_URL } from '../apiConfig.js';

const ChatWindow = () => {
  const MemoizedMessage = React.memo(Message);
  const { chatId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { chatConnection } = useChatSignalR();
  const { fetchChatHeader, fetchMessages, sendMessage, markMessageAsRead, apiError, isLoading: apiLoading } = useChatApi();
  
  const [chatHeader, setChatHeader] = useState(null);
  const [messages, setMessages] = useState([]);
  const [cursor, setCursor] = useState(null);
  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
  const [newMessage, setNewMessage] = useState('');
  const [initialLoad, setInitialLoad] = useState(true);
  
  const messagesEndRef = useRef(null);
  const messagesContainerRef = useRef(null);
  const [userScrolledUp, setUserScrolledUp] = useState(false);
  const scrollPositionRef = useRef(0);

  const loadData = async () => {
    try {
      const [header, messagesData] = await Promise.all([
        fetchChatHeader(chatId),
        fetchMessages(chatId)
      ]);
      
      setChatHeader(header);
      setMessages(messagesData.messages);
      setCursor(messagesData.cursor);
      setHasMore(messagesData.hasMore);
      
      // Устанавливаем флаг, что начальная загрузка завершена
      setInitialLoad(false);
      
      if (chatConnection?.state === HubConnectionState.Connected) {
        await chatConnection.invoke("JoinChatGroup", chatId);
      }
    } catch (err) {
      console.error("Error loading chat data:", err);
      setInitialLoad(false);
    }
  };

  useEffect(() => {
    if (!chatId) return;    
    
    loadData();
    
    return () => {
      if (chatConnection?.state === HubConnectionState.Connected) {
        chatConnection.invoke("LeaveChatGroup", chatId);
      }
    };
  }, [chatId, chatConnection, fetchChatHeader, fetchMessages]);

  useEffect(() => {
    if (!initialLoad && messages.length > 0 && messagesContainerRef.current) {
      messagesContainerRef.current.scrollTop = messagesContainerRef.current.scrollHeight;
    }
  }, [initialLoad, messages.length]);

  const handleScroll = useCallback(() => {
    if (!messagesContainerRef.current) return;
    
    const container = messagesContainerRef.current;
    const scrollTop = container.scrollTop;
    scrollPositionRef.current = scrollTop;
    
    // Определяем, прокрутил ли пользователь вверх
    const isScrolledUp = scrollTop < container.scrollHeight - container.clientHeight - 100;
    setUserScrolledUp(isScrolledUp);
    
    // Если близко к верху и есть еще сообщения - подгружаем
    if (scrollTop < 100 && hasMore && !isLoadingMessages) {
      setIsLoadingMessages(true);
      const prevScrollHeight = container.scrollHeight;
      
      fetchMessages(chatId, cursor)
        .then(data => {
          setMessages(prev => [...data.messages, ...prev]);
          
          // Сохраняем позицию скролла после добавления новых сообщений
          requestAnimationFrame(() => {
            if (messagesContainerRef.current) {
              messagesContainerRef.current.scrollTop = 
                messagesContainerRef.current.scrollHeight - prevScrollHeight;
            }
          });
          
          setCursor(data.cursor);
          setHasMore(data.hasMore);
        })
        .finally(() => setIsLoadingMessages(false));
    }
  }, [chatId, cursor, hasMore, isLoadingMessages, fetchMessages]);

  // Подписка на события SignalR
  useEffect(() => {
    if (!chatConnection) return;

    const handleNewMessage = (message) => {
      setMessages(prev => {
        if (prev.some(m => m.id === message.id)) return prev;
        return [...prev, message];
      });
      
      // Скроллим вниз только если пользователь не прокручивал вверх
      if (!userScrolledUp) {
        setTimeout(() => {
          messagesContainerRef.current?.scrollTo({
            top: messagesContainerRef.current.scrollHeight,
            behavior: 'smooth'
          });
        }, 0);
      }
    };

    const handleMessageRead = (messageId) => {
      setMessages(prev => prev.map(msg => 
        msg.id === messageId ? { ...msg, isRead: true } : msg
      ));
    };

    const handleUserStatus = (userId, isOnline) => {
      if (chatHeader?.id === userId) {
        setChatHeader(prev => ({
          ...prev,
          isOnline,
          lastOnlineAt: isOnline ? null : new Date().toISOString()
        }));
      }
    };

    chatConnection.on("ReceiveMessage", handleNewMessage);
    chatConnection.on("MessageRead", handleMessageRead);
    chatConnection.on("UserOnline", (userId) => handleUserStatus(userId, true));
    chatConnection.on("UserOffline", (userId) => handleUserStatus(userId, false));

    return () => {
      chatConnection.off("ReceiveMessage", handleNewMessage);
      chatConnection.off("MessageRead", handleMessageRead);
      chatConnection.off("UserOnline");
      chatConnection.off("UserOffline");
    };
  }, [chatConnection, chatHeader, userScrolledUp]);

  const handleSendMessage = async () => {
    if (!newMessage.trim()) return;
    
    try {
      await sendMessage(chatId, newMessage);
      setNewMessage('');
    } catch (err) {
      console.error("Error sending message:", err);
    }
  };

  const handleMarkMessageAsRead = async (messageId) => {
    try {
      await markMessageAsRead(messageId);
      setMessages(prev => prev.map(msg => 
        msg.id === messageId ? { ...msg, isRead: true } : msg
      ));
    } catch (err) {
      console.error('Error marking message as read:', err);
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

  const checkInViewport = useCallback((element) => {
    if (!element || !messagesContainerRef.current) return false;
    
    const container = messagesContainerRef.current;
    const containerRect = container.getBoundingClientRect();
    const elementRect = element.getBoundingClientRect();
    
    return (
      elementRect.top >= containerRect.top &&
      elementRect.bottom <= containerRect.bottom
    );
  }, []);

  const leaveChat = () => {
    if (chatConnection) {
      chatConnection.invoke("LeaveChatGroup", chatId);
    }
    navigate('/profile/chats', { replace: true });
  };

  if (apiLoading && !messages.length) return <div className={styles.loading}>Loading chat...</div>;
  if (apiError) return <div className={styles.error}>{apiError}</div>;
  if (!chatHeader) return <div className={styles.error}>Chat not found</div>;

  return (
    <div className={styles.chatContainer}>
      <div className={styles.chatHeader}>
        <button className={styles.backButton} onClick={leaveChat}>
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M15 18L9 12L15 6" stroke="currentColor" strokeWidth="2"/>
          </svg>
        </button>
        
        <div className={styles.userInfo}>
          <div className={styles.avatarContainer}>
            <img 
              src={chatHeader?.avatarImageName ? 
                `${API_BASE_URL}user-images/${chatHeader.avatarImageName}` : 
                avatarPlaceholder} 
              alt={chatHeader?.nickName}
              className={styles.avatar}
            />
            <div className={`${styles.statusIndicator} ${
              chatHeader?.isOnline ? styles.online : styles.offline
            }`}></div>
          </div>
          <div className={styles.userNameContainer}>
            <div className={styles.userName}>{chatHeader?.nickName}</div>
            <div className={`${styles.statusText} ${
              chatHeader?.isOnline ? styles.online : ''
            }`}>
              {chatHeader?.isOnline 
                ? 'Online' 
                : `Last seen ${formatLastOnline(chatHeader?.lastOnlineAt)}`
              }
            </div>
          </div>
        </div>
      </div>
      
      <div 
        ref={messagesContainerRef}
        className={styles.messagesContainer}
        onScroll={handleScroll}
      >
        {isLoadingMessages && (
          <div className={styles.loadingMore}>Loading more messages...</div>
        )}
        
        {messages?.length === 0 && !apiLoading ? (
          <div className={styles.noMessages}>
            <div className={styles.noMessagesIcon}>💬</div>
            <p>No messages yet</p>
          </div>
        ) : (
          messages.map(message => (
            <MemoizedMessage 
              key={message.id}
              message={message}
              isMyMessage={user?.userId === message.senderId}
              onMessageViewed={handleMarkMessageAsRead}
              checkInViewport={checkInViewport}
              containerRef={messagesContainerRef}
            />
          ))
        )}
        
        <div ref={messagesEndRef} />
      </div>
      
      <div className={styles.messageInputContainer}>
        <input
          type="text"
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          placeholder="Type a message..."
          className={styles.messageInput}
          onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
        />
        <button 
          onClick={handleSendMessage}
          className={styles.sendButton}
          disabled={!newMessage.trim()}
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M22 2L11 13" stroke="currentColor" strokeWidth="2"/>
            <path d="M22 2L15 22L11 13L2 9L22 2Z" stroke="currentColor" strokeWidth="2"/>
          </svg>
        </button>
      </div>
    </div>
  );
};

export default ChatWindow;