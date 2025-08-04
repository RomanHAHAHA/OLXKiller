import { useEffect, useRef } from 'react';
import styles from '../Styles/ChatWindow.module.css';

const Message = ({ 
  message, 
  isMyMessage, 
  onMessageViewed,
  containerRef,
  checkInViewport
}) => {
  const messageRef = useRef();

  useEffect(() => {
    if (isMyMessage || message.isRead || !onMessageViewed) return;

    const checkVisibility = () => {
      if (checkInViewport(messageRef.current)) {
        onMessageViewed(message.id);
      }
    };

    checkVisibility();
    
    const container = containerRef.current;
    container?.addEventListener('scroll', checkVisibility);
    
    return () => {
      container?.removeEventListener('scroll', checkVisibility);
    };
  }, [message.id, message.isRead, isMyMessage, onMessageViewed, containerRef, checkInViewport]);

  const formatTimeSend = (createdAt) => {
    if (!createdAt) return 'unknown';
    
    const date = new Date(createdAt);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
  };

  return (
    <div 
      ref={messageRef}
      className={`${styles.messageWrapper} ${isMyMessage ? styles.myMessage : styles.otherMessage}`}
    >
      <div className={styles.messageBubble}>
        <div className={styles.messageContent}>{message.content}</div>
        <div className={styles.messageMeta}>
          <span className={styles.messageTime}>{formatTimeSend(message.createdAt)}</span>
          {isMyMessage && (
            <span className={styles.readStatus}>
              {message.isRead ? '✓✓' : '✓'}
            </span>
          )}
        </div>
      </div>
    </div>
  );
};

export default Message;