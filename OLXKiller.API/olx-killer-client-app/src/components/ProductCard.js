import React from "react";
import "../styles/ProductCardDarkTheme.css";
import { FaHeart, FaRegHeart } from "react-icons/fa"; 

const ProductCard = ({ product, onLikeToggle }) => {
  return (
    <div
      className="product-card card shadow-sm border-0 bg-dark text-light"
      style={{ width: "15rem" }}
    >
      <div className="card-image-container">
        <img
          src={`data:image/png;base64,${product.imageData}`}
          className="card-img-top"
          alt={product.name}
        />
      </div>
      <div className="card-body d-flex flex-column">
        <h5 className="card-title">{product.name}</h5>
        <p className="card-text">{product.description}</p>
        <div className="d-flex justify-content-between align-items-center mt-auto">
          <p className="card-text mb-0">${product.price}</p>
          <p
            className={`card-text mb-0 ${
              product.isAvailable ? "text-success" : "text-danger"
            }`}
          >
            {product.isAvailable ? "Available" : "Out of Stock"}
          </p>
        </div>
        <div
          className="like-icon mt-2"
          onClick={() => onLikeToggle(product.id)} 
          style={{ cursor: "pointer" }}
        >
          {product.liked ? (
            <FaHeart className="text-warning" /> 
          ) : (
            <FaRegHeart className="text-warning" /> 
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
