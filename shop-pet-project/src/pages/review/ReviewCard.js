import { API_BASE_URL } from "../../apiConfig";
import avatarPlaceholder from "../../asserts/default_avatar_image.png";
import styles from "../../Styles/ReviewCard.module.css";

const reviewsUrl = `${API_BASE_URL}reviews-api/api/reviews`;
const avatarUrl = `${API_BASE_URL}user-images/`;

const ReviewCard = ({ review, onStatusChange }) => {
    const handleStatusChange = async (review, newStatus) => {
        try {
            const response = await fetch(`${reviewsUrl}/${review.user.id}/${review.productId}/status/${newStatus}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ status: newStatus })
            });

            if (response.ok) {
                onStatusChange();
            } 
        } catch (error) {
            console.error('Failed to update review status:', error);
        }
    };

    const getStatusBadge = (status) => {
        switch(status) {
            case "Approved": return styles.statusApproved;
            case "Rejected": return styles.statusRejected;
            default: return styles.statusPending;
        }
    };
  
    return (
        <div className={styles.card}>
            <div className={styles.header}>
                <div className={styles.userSection}>
                    <img 
                        src={review.user.avatarPath ? `${avatarUrl}${review.user.avatarPath}` : avatarPlaceholder} 
                        alt={review.user.nickName}
                        className={styles.avatar}
                    />
                    <div className={styles.userInfo}>
                        <div className={styles.userName}>{review.user.nickName}</div>
                        <div className={styles.reviewDate}>
                            {review.createdAt}
                        </div>
                    </div>
                </div>
                <div className={styles.rating}>
                    {[...Array(5)].map((_, i) => (
                        <span 
                            key={i} 
                            className={`${styles.star} ${i < review.rate ? styles.starFilled : ''}`}
                        >
                            ★
                        </span>
                    ))}
                </div>
            </div>
            
            <div className={styles.contentSection}>
                <p className={styles.reviewText}>{review.text}</p>
            </div>
            
            <div className={styles.statusSection}>
                <span className={`${styles.statusBadge} ${getStatusBadge(review.status)}`}>
                    {review.status}
                </span>
                <div className={styles.actions}>
                    {review.status != "Approved" && (
                        <button 
                            className={`${styles.actionButton} ${styles.approveButton}`}
                            onClick={() => handleStatusChange(review, 1)}
                        >
                            Approve
                        </button>
                    )}
                    {review.status != "Rejected" && (
                        <button 
                            className={`${styles.actionButton} ${styles.rejectButton}`}
                            onClick={() => handleStatusChange(review, 2)}
                        >
                            Reject
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ReviewCard;