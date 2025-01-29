import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";

const ProductInfo = () => {
  const { productId } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchProductData = async () => {
    try {
      const response = await fetch(`https://localhost:7208/api/Products/productId=${productId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const jsonResponse = await response.json();
        console.log(jsonResponse);
        setProduct(jsonResponse.data); 
      } else {
        const error = await response.json();
        Swal.fire({
          icon: "error",
          title: "Error",
          text: error.description || "Failed to fetch product details.",
        });
        navigate("/");
      }
    } catch (error) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An error occurred while loading product data.",
      });
      navigate("/");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProductData();
  }, [productId]);

  if (loading) {
    return <div className="text-center text-light">Loading product details...</div>;
  }

  if (!product) {
    return null;
  }

  return (
    <div className="container my-5 text-light">
      <div className="row">
        <div className="col-md-6">
          {product.imageStrings && product.imageStrings.length > 0 ? (
            <div className="d-flex flex-wrap gap-3">
              {product.imageStrings.map((image, index) => (
                <img
                  key={index}
                  src={`data:image/png;base64,${image}`}
                  alt={`${product.name} - ${index + 1}`}
                  className="img-fluid rounded shadow-sm"
                  style={{ maxWidth: "100%", maxHeight: "300px" }}
                />
              ))}
            </div>
          ) : (
            <p>No images available.</p>
          )}
        </div>
        <div className="col-md-6">
          <h2>{product.name}</h2>
          <p>{product.description}</p>
          <p>
            <strong>Price:</strong> ${product.price}
          </p>
          <p>
            <strong>Amount:</strong> {product.amount}
          </p>
          <p>
            <strong>Availability:</strong> {product.isAvailable ? "Available" : "Out of Stock"}
          </p>
          <p>
            <strong>Liked:</strong> {product.liked ? "Yes" : "No"}
          </p>
          <hr />
          <div className="seller-info mt-3">
            <h4>Seller Information</h4>
            <div className="d-flex align-items-center gap-3">
              {product.sellerAvatar ? (
                <img
                  src={`data:image/png;base64,${product.sellerAvatar}`}
                  alt={product.sellerNickName}
                  className="img-fluid rounded-circle shadow-sm"
                  style={{ width: "50px", height: "50px" }}
                />
              ) : (
                <div
                  className="rounded-circle bg-secondary d-flex justify-content-center align-items-center"
                  style={{ width: "50px", height: "50px" }}
                >
                  <span className="text-light">N/A</span>
                </div>
              )}
              <span className="seller-nickname text-light">
                <strong>{product.sellerNickName || "Unknown Seller"}</strong>
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductInfo;
