import { Trash2, Copy } from 'lucide-react';
import Swal from 'sweetalert2';
import styles from '../Styles/ActionLogs.module.css';
import { API_BASE_URL } from '../apiConfig';

const ActionLogRow = ({ log, onDelete }) => {
  const deleteLog = async (logId) => {
    try {
      const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        background: '#1e293b',
        color: '#f8fafc',
        iconColor: '#f59e0b',
        showCancelButton: true,
        confirmButtonColor: '#3b82f6',
        cancelButtonColor: '#ef4444',
        confirmButtonText: 'Yes, delete it!',
        customClass: {
          popup: 'dark-swal',
          actions: 'swal-actions-dark'
        }
      });

      if (result.isConfirmed) {
        const response = await fetch(`${API_BASE_URL}logs-api/api/action-logs/${logId}`, {
          method: 'DELETE',
          credentials: 'include'
        });

        if (response.ok) {
          onDelete();
        } else {
          throw new Error('Failed to delete log');
        }
      }
    } catch (err) {
      Swal.fire('Error!', err.message, 'error');
    }
  };

  const copyUserId = async () => {
    try {
      await navigator.clipboard.writeText(log.userId);
    } catch (err) {
      Swal.fire('Error!', 'Failed to copy User ID', 'error');
    }
  };

  return (
    <tr>
      <td>{log.id.substring(0, 8)}...</td>
      <td>
        <div className={styles.userIdCell}>
          {log.userId.substring(0, 8)}...
        </div>
      </td>
      <td>
        <span className={`${styles.actionBadge} ${styles[log.actionType]}`}>
          {log.actionType}
        </span>
      </td>
      <td>{log.description}</td>
      <td>{log.createdAt}</td>
      <td>
        <div className={styles.actionButtons}>
          <button 
            className={styles.copyButton}
            onClick={copyUserId}
            title="Copy User ID"
          >
            <Copy size={16} />
          </button>
          <button 
            className={styles.deleteButton}
            onClick={() => deleteLog(log.id)}
            title="Delete log"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </td>
    </tr>
  );
};

export default ActionLogRow;