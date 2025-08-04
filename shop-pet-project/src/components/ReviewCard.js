import Rating from './Rating';
import styles from '../Styles/ProductPage.module.css';
import defaultAvatar from "../asserts/default_avatar_image.png";
import { API_BASE_URL } from "../apiConfig";
import { FaThumbsUp, FaThumbsDown } from 'react-icons/fa'; // Залитые иконки
import { FaRegThumbsUp, FaRegThumbsDown } from 'react-icons/fa'; // Контурные иконки
import useAuthAlert from '../useAuthAlert'

const avatarUrl = `${API_BASE_URL}user-images/`;

const ReviewCard = ({ review, onReviewVoted }) => {
  const showAuthAlert = useAuthAlert();

  const handleVote = async (voteType) => {
    try {
      const response = await fetch(
        `${API_BASE_URL}reviews-api/api/review-votes/${review.userId}/${review.productId}/${voteType}`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include'
        }
      );

      if (response.ok) {
        onReviewVoted();
      } else if (response.status === 401) {
          showAuthAlert({ text: 'Please, login to perform this action'});
          return;
      }
    } catch (error) {
      console.error('Error submitting vote:', error);
    }
  };

  return (
    <div className={styles.reviewItem}>
      <div className={styles.reviewContent}>
        <img
          src={review.avatarPath ? `${avatarUrl}${review.avatarPath}` : defaultAvatar}
          alt={review.nickName}
          className={styles.reviewAvatar}
          onError={(e) => {
            e.target.src = defaultAvatar;
          }}
        />
        <div className={styles.reviewDetails}>
          <div className={styles.reviewHeader}>
            <div>
              <h3 className={styles.reviewAuthor}>{review.nickName}</h3>
              <div className={styles.reviewMeta}>
                <Rating value={review.rate} size="sm" />
                <span className={styles.reviewDate}>{review.createdAt}</span>
              </div>
            </div>
            <div className={styles.reviewActions}>
              <button 
                className={`${styles.reviewAction} ${review.currentUserVote === 1 ? styles.activeVote : ''}`}
                onClick={() => handleVote(1)}
                aria-label="Like this review"
              >
                <span className={styles.actionCount}>({review.likesCount})</span>
                {review.currentUserVote === 1 ? (
                  <FaThumbsUp size={16} className={styles.actionIcon} />
                ) : (
                  <FaRegThumbsUp size={16} className={styles.actionIcon} />
                )}
              </button>
              <button 
                className={`${styles.reviewAction} ${styles.reviewDislike} ${review.currentUserVote === 2 ? styles.activeVote : ''}`}
                onClick={() => handleVote(2)}
                aria-label="Dislike this review"              >
                <span className={styles.actionCount}>({review.dislikesCount})</span>
                {review.currentUserVote === 2 ? (
                  <FaThumbsDown size={16} className={styles.actionIcon} />
                ) : (
                  <FaRegThumbsDown size={16} className={styles.actionIcon} />
                )}
              </button>
            </div>
          </div>
          <p className={styles.reviewText}>{review.text}</p>
        </div>
      </div>
    </div>
  );
};

export default ReviewCard;