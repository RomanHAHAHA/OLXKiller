import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { API_BASE_URL } from "../../apiConfig";
import { useAuth } from "../../AuthProvider";  
import Rating from "../../components/Rating";
import imagePlaceholder from "../../asserts/imagePlaceholder.jpg";
import defaultAvatar from "../../asserts/default_avatar_image.png";
import styles from "../../Styles/ProductPage.module.css";
import ReviewCard from "../../components/ReviewCard";
import CreateReviewForm from "../../forms/CreateReviewForm";
import Swal from "sweetalert2";
import useAuthAlert from "../../useAuthAlert";

const productsUrl = `${API_BASE_URL}products-api/api/products`;
const reviewsUrl = `${API_BASE_URL}reviews-api/api/reviews`;
const ordersUrl = `${API_BASE_URL}orders-api/api/orders`;
const imagesUrl = `${API_BASE_URL}product-images/`;
const avatarUrl = `${API_BASE_URL}user-images/`;
const addToCartUrl = `${API_BASE_URL}carts-api/api/carts`;

const ProductPage = () => {
  const { productId } = useParams();
  const { user } = useAuth();  
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentImage, setCurrentImage] = useState(0);
  const [hasOrderedProduct, setHasOrderedProduct] = useState(false);
  const [hasReviewedProduct, setHasReviewedProduct] = useState(false);
  const showAuthAlert = useAuthAlert();

  const fetchProductData = async () => {
    try {
      setLoading(true);
      const productResponse = await fetch(`${productsUrl}/${productId}`, {
        credentials: 'include'
      });
      if (!productResponse.ok) throw new Error("Product not found");
      const productData = await productResponse.json();
      setProduct(productData.data);
      setLoading(false);
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  };

  const fetchProductReviews = async () => {
    const reviewsResponse = await fetch(`${reviewsUrl}/product/${productId}`, { credentials: 'include'});
    if (reviewsResponse.ok) {
      const reviewsData = await reviewsResponse.json();
      setReviews(reviewsData);
    }
  }

  const redirectToChat = async () => {
    if (product.seller.userId === user.userId) return;
    
    try {
      const response = await fetch(`${API_BASE_URL}chats-api/api/chats/${product.seller.userId}/exists`, {
        credentials: 'include',
      });

      if (!response.ok && response.status === 404) {
        await createChat();
      }

      const data = await response.json();
      
      if (data.data) {
        navigate(`/profile/chats/${data.data}`);
      } else {
        await createChat();
      }
    } catch (error) {
      console.error('Error checking chat:', error);
    }
  }

  const createChat = async () => {
    if (product.seller.userId === user.userId) return;
    
    try {
      const response = await fetch(`${API_BASE_URL}chats-api/api/chats/${product.seller.userId}`, {
        method: 'POST',
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error('Failed to check chat existence');
      }

      const data = await response.json();
      
      if (data.data) {
        navigate(`/profile/chats/${data.data}`);
      } else {
        
      }
    } catch (error) {
      console.error('Error checking chat:', error);
    }
  }

  useEffect(() => {
    fetchProductData();
    fetchProductReviews();
  }, [productId]);

  useEffect(() => {
    const checkIfUserOrderedProduct = async () => {
      if (!user || !productId) return;
      
      try {
        const response = await fetch(`${ordersUrl}/${productId}`, {
          credentials: 'include'
        });
        
        if (response.ok) {
          const hasOrdered = (await response.json()).data;
          setHasOrderedProduct(hasOrdered);
        }
      } catch (error) {
        console.error("Failed to check order status:", error);
      }
    };

    checkIfUserOrderedProduct();
  }, [productId, user]);

  useEffect(() => {
    const checkIfUserReviewedProduct = async () => {
      if (!user || !productId) return;
      
      try {
        const response = await fetch(`${reviewsUrl}/has-reviewed-product/${productId}`, {
          credentials: 'include'
        });
        
        if (response.ok) {
          const hasReviewed = (await response.json()).data;
          setHasReviewedProduct(hasReviewed);
        }
      } catch (error) {
        console.error("Failed to check order status:", error);
      }
    };

    checkIfUserReviewedProduct();
  })

  const handleAddToCart = async (e) => {
        e.stopPropagation(); 
        try {
            const response = await fetch(`${addToCartUrl}/${product.id}`, { 
                method: 'POST', 
                credentials: 'include' 
            });
            
            if (response.ok) {
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Added to cart!',
                    showConfirmButton: false,
                    timer: 1000,
                    toast: true,
                    background: '#28a745',
                    color: 'white'
                });
            } else if (response.status === 400) {
                const error = await response.json();
                Swal.fire({
                    title: 'Error',
                    icon: 'error',
                    text: error.message || "Unexpected error occurred"
                });
                
            } else if (response.status === 401) {
                showAuthAlert({
                    text: 'Please log in to add items to your cart'
                });    
            } else {
                throw new Error("Unexpected error occurred");
            }
        } catch (error) {
            Swal.fire({
                title: 'Error',
                icon: 'error',
                text: error.message
            });
        } 
    }

  const handleUpdateClick = () => {
    navigate(`/products/${productId}/update`);
  };

  const onReviewCreated = () => {
    fetchProductData();
    fetchProductReviews();
  }

  const onReviewVoted = () => {
    fetchProductReviews();
  }

  if (loading) return (
    <div className={styles.loadingContainer}>
      <div className={styles.loadingSpinner}></div>
    </div>
  );

  if (error) return (
    <div className={styles.errorContainer}>
      {error}
    </div>
  );

  if (!product) return (
    <div className={styles.notFoundContainer}>
      Product not found
    </div>
  );

  return (
    <div className={styles.productPage}>
      <div className={styles.productContainer}>
        <div className={styles.productGrid}>
          {/* Левая колонка - галерея изображений и отзывы */}
          <div className={styles.leftColumn}>
            <div className={styles.productGallery}>
              <div className={styles.mainImageContainer}>
                <img
                  src={product.images[currentImage]?.path ? `${imagesUrl}${product.images[currentImage].path}` : imagePlaceholder}
                  alt={product.name}
                  className={styles.mainImage}
                  onError={(e) => {
                    e.target.src = imagePlaceholder;
                  }}
                />
              </div>
              <div className={styles.thumbnailContainer}>
                {product.images.map((image, index) => (
                  <button
                    key={image.id}
                    onClick={() => setCurrentImage(index)}
                    className={`${styles.thumbnail} ${currentImage === index ? styles.activeThumbnail : ''}`}
                  >
                    <img
                      src={image.path ? `${imagesUrl}${image.path}` : imagePlaceholder}
                      alt={`${product.name} thumbnail ${index}`}
                      onError={(e) => {
                        e.target.src = imagePlaceholder;
                      }}
                    />
                  </button>
                ))}
              </div>
            </div>

            {/* Секция отзывов под галереей */}
            <div className={styles.reviewsSection}>
              <h2 className={styles.reviewsTitle}>Customer Reviews</h2>
              
              {reviews.length > 0 ? (
                <div className={styles.reviewsList}>
                  {reviews.map((review) => (
                    <ReviewCard key={`${review.userId}-${review.createdAt}`} review={review} onReviewVoted={onReviewVoted}/>
                  ))}
                </div>
              ) : (
                <div className={styles.noReviews}>
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                  </svg>
                  <p>No reviews yet. Be the first to review this product!</p>
                </div>
              )}

              {hasOrderedProduct && !hasReviewedProduct && (
                <CreateReviewForm onReviewCreated={onReviewCreated}/>
              )}
            </div>
          </div>

          {/* Правая колонка - информация о продукте */}
          <div className={styles.productInfo}>
            <div>
              <div className={styles.productHeader}>
                <h1 className={styles.productTitle}>{product.name}</h1>
                <span className={styles.productPrice}>${product.price}</span>
              </div>
              
              <div className={styles.productMeta}>
                <Rating value={product.rating || 0} />
                <span className={styles.reviewCount}>({reviews.length} reviews)</span>
                {product.stockQuantity > 0 ? (
                  <span className={styles.inStock}>In Stock ({product.stockQuantity})</span>
                ) : (
                  <span className={styles.outOfStock}>Out of Stock</span>
                )}
              </div>
            </div>

            <div className={styles.categoryTags}>
              {product.categories.map(category => (
                <span key={category.id} className={styles.categoryTag}>
                  {category.name}
                </span>
              ))}
            </div>

            <div className={styles.infoCard}>
              <div 
                className={styles.sellerInfo}
                onClick={redirectToChat}
                style={{ cursor: product.seller.userId !== user.userId ? 'pointer' : 'default' }}
                >
                <img
                  src={product.seller.avatarImageName ? `${avatarUrl}${product.seller.avatarImageName}` : defaultAvatar}
                  alt={product.seller.nickName}
                  className={styles.sellerAvatar}
                  onError={(e) => {
                    e.target.src = defaultAvatar;
                  }}
                />
                <div>
                  <p className={styles.sellerName}>Sold by {product.seller.nickName}</p>
                  <div className={styles.sellerRating}>
                    <Rating value={product.seller.rating || 0} size="sm" />
                    <span className={styles.sellerJoinDate}>Member since {product.seller.registerDate}</span>
                  </div>
                </div>
              </div>
            </div>

            <div className={styles.infoCard}>
              <h2>Description</h2>
              <p>{product.description}</p>
            </div>

            <div className={styles.actionButtons}>
              {product.isMine && (
                <button 
                  className={`${styles.actionButton} ${styles.updateButton}`}
                  onClick={handleUpdateClick}
                >
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                  Update
                </button>
              )}
              <button 
                className={`${styles.actionButton} ${styles.primaryButton}`}
                disabled={product.stockQuantity <= 0}
                onClick={handleAddToCart}
              >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                Add to Cart
              </button>
              <button 
                className={`${styles.actionButton} ${styles.secondaryButton}`}
                disabled={product.stockQuantity <= 0}
              >
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
                Buy Now
              </button>
            </div>

            {product.characteristics && product.characteristics.length > 0 && (
              <div className={styles.infoCard}>
                <h2>Specifications</h2>
                <div className={styles.specsList}>
                  {product.characteristics.map((char, index) => (
                    <div key={index} className={styles.specItem}>
                      <span className={styles.specItem}>
                        <span className={styles.specName}>{char.name}:</span> 
                        <span className={styles.specValue}> {char.value}</span>
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductPage;