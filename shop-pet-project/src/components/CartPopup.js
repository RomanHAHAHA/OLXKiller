import { useEffect, useRef, useCallback, memo } from "react";
import { useCart } from "../hooks/useCart";
import { API_BASE_URL } from "../apiConfig";
import { Trash2, Plus, Minus, X } from "lucide-react"; 
import imagePlaceholder from "../asserts/imagePlaceholder.jpg";
import { Link } from "react-router-dom";
import "../Styles/CartPopup.css";

const CartItem = memo(({ item, updateQuantity, removeItem }) => {
  const imageUrl = `${API_BASE_URL}product-images/`;

  const handleIncrement = useCallback(() => {
    updateQuantity(item.product.id, "increment");
  }, [item.product.id, updateQuantity]);

  const handleDecrement = useCallback(() => {
    updateQuantity(item.product.id, "decrement");
  }, [item.product.id, updateQuantity]);

  const handleRemove = useCallback(() => {
    removeItem(item.product.id);
  }, [item.product.id, removeItem]);

  return (
    <div className="cart-item">
      <img
        src={item.product.mainImagePath ? `${imageUrl}${item.product.mainImagePath}` : imagePlaceholder}
        alt={item.product.name}
        className="cart-item-image"
        loading="lazy"
      />
      <div className="cart-item-details">
        <div className="cart-item-name">{item.product.name}</div>
        <div className="cart-item-controls">
          <button
            className="cart-quantity-btn"
            onClick={handleDecrement}
            disabled={item.quantity === 1}
          >
            <Minus size={14} />
          </button>
          <span className="cart-quantity">{item.quantity}</span>
          <button className="cart-quantity-btn" onClick={handleIncrement}>
            <Plus size={14} />
          </button>
        </div>
        <div className="cart-item-price">
          {item.quantity} × {item.product.price.toFixed(2)} UAH
        </div>
      </div>
      <button className="cart-remove-btn" onClick={handleRemove}>
        <Trash2 size={14} color="white" />
      </button>
    </div>
  );
});

const CartPopup = ({ onClose }) => {
  const { cartItems, totalPrice, updateQuantity, removeItem, isLoading } = useCart();
  const popupRef = useRef(null);

  const handleClickOutside = useCallback((event) => {
    if (popupRef.current && !popupRef.current.contains(event.target)) {
      onClose();
    }
  }, [onClose]);

  useEffect(() => {
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [handleClickOutside]);

  return (
    <div ref={popupRef} className={`cart-popup ${cartItems.length > 0 ? 'active' : ''}`}>
      <div className="cart-header">
        <div className="cart-title">Your Cart</div>
        <button className="cart-close-btn" onClick={onClose}>
          <X size={20} />
        </button>
      </div>
      
      <div className="cart-content">
        {cartItems.length === 0 ? (
          <div className="cart-empty">Your cart is empty</div>
        ) : (
          cartItems.map((item) => (
            <CartItem
              key={item.product.id}
              item={item}
              updateQuantity={updateQuantity}
              removeItem={removeItem}
            />
          ))
        )}
      </div>
      
      {cartItems.length > 0 && (
        <div className="cart-footer">
          <div className="cart-total">
            <span>Total:</span>
            <span>{totalPrice.toFixed(2)} UAH</span>
          </div>
          <Link 
            to="/create-order" 
            className="cart-checkout-btn"
            onClick={onClose}
            disabled={isLoading}
          >
            {isLoading ? 'Processing...' : 'Checkout'}
          </Link>
        </div>
      )}
    </div>
  );
};

export default memo(CartPopup);