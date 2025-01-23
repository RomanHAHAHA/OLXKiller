import React, { useEffect, useState } from "react";
import ProductCard from "./ProductCard";

const Products = () => {
  const [products, setProducts] = useState([]);
  const [totalCount, setTotalCount] = useState(0);

  const fetchProducts = async () => {
    try {
      const response = await fetch("https://localhost:7208/api/Products", {
        method: "GET",
        credentials: "include", 
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch products: ${response.status}`);
      }

      const data = await response.json();
      setProducts(data.result.collection);
      setTotalCount(data.result.totalCount);
    } catch (error) {
      console.error("Error fetching products:", error);
    } 
  };

  const likeProduct = async (productId) => {
    try {
      const response = await fetch(`https://localhost:7208/api/Products/like/${productId}`, {
          method: "POST",
          credentials: "include", 
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to like product: ${response.status}`);
      }

      fetchProducts();
    } catch (error) {
      console.error("Error liking product:", error);
    }
  };

  const unlikeProduct = async (productId) => {
    try {
      const response = await fetch(`https://localhost:7208/api/Products/un-like/${productId}`, {
          method: "POST",
          credentials: "include", 
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to unlike product: ${response.status}`);
      }

      fetchProducts();
    } catch (error) {
      console.error("Error unliking product:", error);
    }
  };

  const toggleLike = (productId, liked) => {
    if (liked) {
      unlikeProduct(productId);
    } else {
      likeProduct(productId);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  return (
    <div className="container">
      <h3>Total Products: {totalCount}</h3>
      <div className="row">
        {products.map(product => (
          <div className="col-md-4" key={product.id}>
            <ProductCard
              product={product}
              onLikeToggle={() => toggleLike(product.id, product.liked)} 
            />
          </div>
        ))}
      </div>
    </div>
  );
};

export default Products;
