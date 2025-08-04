import styles from '../Styles/NotificationContainer.module.css';
import Notification from './Notification';

const NotificationContainer = ({ notifications, onClose, onClick }) => {
  return (
    <div className={styles.notificationContainer}>
      {notifications.map(notification => (
        <Notification
          key={notification.id}
          message={notification}
          onClose={() => onClose(notification.id)}
          onClick={() => onClick(notification.chatId)}
        />
      ))}
    </div>
  );
};

export default NotificationContainer;