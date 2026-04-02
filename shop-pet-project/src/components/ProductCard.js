import Swal from "sweetalert2";
import { API_BASE_URL } from "../apiConfig";
import imagePlaceholder from '../asserts/imagePlaceholder.jpg';
import { useSignalR } from "../SignalRProvider";
import { useEffect, useState } from "react";
import { FaShoppingCart, FaHeart, FaRegHeart, FaStar, FaEye } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import "../Styles/ProductCard.css";
import useAuthAlert from '../useAuthAlert';

const imagesUrl = `${API_BASE_URL}product-images/`;
const addToCartUrl = `${API_BASE_URL}carts-api/api/carts`;

const ProductCard = ({ product }) => {
    const { connection } = useSignalR();
    const [isHovered, setIsHovered] = useState(false);
    const [isFavorite, setIsFavorite] = useState(false);
    const [isAddingToCart, setIsAddingToCart] = useState(false);
    const navigate = useNavigate();
    const showAuthAlert  = useAuthAlert();

    const handleAddToCart = async (e) => {
        e.stopPropagation(); 
        setIsAddingToCart(true);
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
        } finally {
            setIsAddingToCart(false);
        }
    }

    const handleCardClick = () => {
        navigate(`/products/${product.id}`);
    };

    useEffect(() => {
        if (!connection) return;

        const handleProductExceeded = (stockQuantity) => {
            Swal.fire({
                title: "Warning",
                text: `There is only ${stockQuantity} items in stock`,
                icon: 'warning',
                confirmButtonColor: '#3085d6',
            });
        };

        connection.on("NotifyProductStockExceeded", handleProductExceeded);

        return () => {
            connection.off("NotifyProductStockExceeded", handleProductExceeded);
        };
    }, [connection]);

    return (
        <div className="col-6 col-md-4 col-lg-3 mb-4">
            <div 
                className="card h-100 bg-dark text-light border-0 rounded-3 overflow-hidden shadow-sm hover-shadow-lg transition-all product-card"
                style={{ 
                    transform: isHovered ? 'translateY(-5px)' : 'none',
                    transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                    cursor: 'pointer'
                }}
                onMouseEnter={() => setIsHovered(true)}
                onMouseLeave={() => setIsHovered(false)}
                onClick={handleCardClick}
            >
                <div className="position-relative overflow-hidden" style={{ height: "200px" }}>
                    <img
                        src={product.mainImagePath ? `${imagesUrl}${product.mainImagePath}` : imagePlaceholder}
                        alt={product.name}
                        className="w-100 h-100 object-fit-contain p-3"
                        style={{
                            transition: 'transform 0.5s ease',
                            transform: isHovered ? 'scale(1.05)' : 'scale(1)'
                        }}
                    />
                    
                    <div className="position-absolute top-0 start-0 p-2">
                        {!product.isAvailable && (
                            <span className="badge bg-danger">Sold Out</span>
                        )}
                        {product.discount > 0 && (
                            <span className="badge bg-warning text-dark ms-1">-{product.discount}%</span>
                        )}
                    </div>
                    
                    <div className="position-absolute top-0 end-0 p-2 d-flex flex-column gap-2">
                        <button 
                            className="btn btn-sm btn-light rounded-circle p-1 d-flex align-items-center justify-content-center"
                            onClick={(e) => {
                                e.stopPropagation();
                                setIsFavorite(!isFavorite);
                            }}
                            style={{ width: '28px', height: '28px' }}
                        >
                            {isFavorite ? (
                                <FaHeart className="text-danger" size={14} />
                            ) : (
                                <FaRegHeart className="text-muted" size={14} />
                            )}
                        </button>
                    </div>
                </div>

                <div className="card-body p-3 d-flex flex-column">
                    <div className="mb-2">
                        <h6 className="card-title text-white mb-1 text-truncate">{product.name}</h6>
                        <small className="d-block mb-1 text-gray">{product.category}</small>
                    </div>
                    
                    <div className="mt-auto">
                        <div className="d-flex align-items-center mb-2">
                            <div className="d-flex align-items-center me-2">
                                <FaStar className="text-warning me-1" size={14} />
                                <small className="text-white">{product.rating.toFixed(1)}</small>
                            </div>
                            <div className="d-flex align-items-center">
                                {product.discount > 0 ? (
                                    <>
                                        <span className="text-decoration-line-through text-muted me-2">
                                            {product.price.toFixed(2)} UAH
                                        </span>
                                        <span className="text-success fw-bold">
                                            {(product.price * (1 - product.discount/100)).toFixed(2)} UAH
                                        </span>
                                    </>
                                ) : (
                                    <span className="text-white fw-bold">{product.price.toFixed(2)} UAH</span>
                                )}
                            </div>
                        </div>
                        
                        <button
                            className={`btn-accent-outline w-100 rounded-2 d-flex align-items-center justify-content-center gap-2 ${isAddingToCart ? 'btn-progress' : ''}`}
                            onClick={handleAddToCart}
                            disabled={!product.isAvailable || isAddingToCart}
                            style={{ height: '38px' }}
                        >
                            {isAddingToCart ? (
                                <>
                                    <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                    Adding...
                                </>
                            ) : (
                                <>
                                    <FaShoppingCart size={16} />
                                    Add to Cart
                                </>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ProductCard;