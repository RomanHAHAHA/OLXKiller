import { useState } from 'react';
import Rating from '../components/Rating';
import styles from '../Styles/ProductPage.module.css';
import { API_BASE_URL } from '../apiConfig';
import Swal from 'sweetalert2';
import { useParams } from 'react-router-dom';
import { useAuth } from '../AuthProvider';

const CreateReviewForm = ({ onReviewCreated }) => {
  const { productId } = useParams();
  const { fetchUser } = useAuth();
  const [rating, setRating] = useState(0);
  const [text, setText] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState({});

  const handleTextChange = (e) => {
    setText(e.target.value);
    if (errors.text) setErrors(prev => ({ ...prev, text: '' }));
  };

  const handleRatingChange = (newRating) => {
    setRating(newRating);
    if (errors.rate) setErrors(prev => ({ ...prev, rate: '' }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    setErrors({});

    try {
      const response = await fetch(`${API_BASE_URL}reviews-api/api/reviews`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          ProductId: productId,
          Text: text,
          Rate: rating
        })
      });

      if (!response.ok) {
        const data = await response.json();
        
        if (response.status === 409) {
          Swal.fire({
            title: 'Error',
            text: data.descriprion || "You have not ordered this product",
            icon: 'error'
          });
          return;
        }

        if (data.errors) {
          const validationErrors = {};
          for (const field in data.errors) {
            if (data.errors[field]?.length > 0) {
              validationErrors[field.toLowerCase()] = data.errors[field][0];
            }
          }
          setErrors(validationErrors);
          return;
        }

        throw new Error(data.title || 'Failed to submit review');
      }

      setRating(0);
      setText('');
      
      Swal.fire({
        title: 'Thank you!',
        text: 'Your review has been submitted and is awaiting moderation.',
        icon: 'success'
      });

      await fetchUser();
      if (onReviewCreated) onReviewCreated();

    } catch (err) {
      Swal.fire({
        title: 'Error',
        text: err.message,
        icon: 'error'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={styles.reviewForm}>
      <h3>Write a Review</h3>
      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.formGroup}>
          <label htmlFor="rating">Your Rating</label>
          <Rating 
            interactive={true} 
            value={rating} 
            onChange={handleRatingChange} 
          />
          {errors.rate && (
            <div className={styles.errorText}>{errors.rate}</div>
          )}
        </div>
        
        <div className={styles.formGroup}>
          <label htmlFor="review">Your Review</label>
          <textarea
            id="review"
            rows="5"
            className={`${styles.reviewTextarea} ${errors.text ? styles.errorBorder : ''}`}
            placeholder="Share your honest thoughts about this product..."
            value={text}
            onChange={handleTextChange}
            required
          />
          {errors.text && (
            <div className={styles.errorText}>{errors.text}</div>
          )}
        </div>
        
        <button
          type="submit"
          className='btn-accent-outline'
          style={{ width: '150px' }}
          disabled={rating === 0 || !text.trim() || isSubmitting}
        >
          {isSubmitting ? 'Submitting...' : 'Submit Review'}
        </button>
      </form>
    </div>
  );
};

export default CreateReviewForm;