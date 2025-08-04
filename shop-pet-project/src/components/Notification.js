import { useState, useEffect } from 'react';
import avatarPlaceholder from "../asserts/default_avatar_image.png";
import styles from '../Styles/Notification.module.css';
import { API_BASE_URL } from '../apiConfig';

const avatarUrl = `${API_BASE_URL}user-images/`;

const Notification = ({ message, onClose, onClick }) => {
  const [isClosing, setIsClosing] = useState(false);
  const [timer, setTimer] = useState(null);

  useEffect(() => {
    const newTimer = setTimeout(() => {
      handleAutoClose();
    }, 5000);
    
    setTimer(newTimer);
    
    return () => {
      if (timer) clearTimeout(timer);
    };
  }, []);

  const handleAutoClose = () => {
    setIsClosing(true);
    setTimeout(() => {
      onClose(); 
    }, 300);
  };

  const handleClose = (e) => {
    e.stopPropagation();
    if (timer) clearTimeout(timer);
    handleAutoClose();
  };

  const handleClick = () => {
    onClick(message.chatId); 
    onClose(); 
  };

  return (
    <div 
      className={`${styles.notification} ${isClosing ? styles.closing : ''}`}
      onClick={handleClick}
      onMouseEnter={() => timer && clearTimeout(timer)}
      onMouseLeave={() => {
        if (!isClosing) {
          const newTimer = setTimeout(handleAutoClose, 5000);
          setTimer(newTimer);
        }
      }}
    >
      <div className={styles.notificationContent}>
        <img 
          src={message.senderAvatar ? `${avatarUrl}${message.senderAvatar}` : avatarPlaceholder} 
          alt={message.senderName} 
          className={styles.notificationAvatar}
        />
        <div className={styles.notificationText}>
          <div className={styles.notificationSender}>{message.senderName}</div>
          <div className={styles.notificationMessage}>{message.content}</div>
        </div>
      </div>
      <button className={styles.notificationClose} onClick={handleClose}>
        &times;
      </button>
    </div>
  );
};

export default Notification;