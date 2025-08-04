import { useState } from 'react';
import '../Styles/Rating.css';

const Rating = ({ 
  value = 0, 
  max = 5,
  size = 'md',
  color = 'gold',
  interactive = false,
  onChange 
}) => {
  const [hoverValue, setHoverValue] = useState(null);

  const handleClick = (newValue) => {
    if (interactive && onChange) {
      onChange(newValue);
    }
  };

  return (
    <div 
      className={`rating rating--${color} rating--${size}`}
      aria-label={`Rating: ${value} out of ${max}`}
    >
      {[...Array(max)].map((_, index) => {
        const starValue = index + 1;
        const isActive = starValue <= (hoverValue || value);
        
        return (
          <button
            key={starValue}
            type="button"
            className={`rating-star ${isActive ? 'active' : ''} ${interactive ? 'interactive' : ''}`}
            onClick={() => handleClick(starValue)}
            onMouseEnter={() => interactive && setHoverValue(starValue)}
            onMouseLeave={() => interactive && setHoverValue(null)}
            aria-label={`Rate ${starValue} out of ${max}`}
          >
            <svg viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z"
              />
            </svg>
          </button>
        );
      })}
    </div>
  );
};

export default Rating;