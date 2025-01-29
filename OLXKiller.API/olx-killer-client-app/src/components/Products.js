import React, { useEffect, useState } from "react";
import ProductCard from "./ProductCard";

const Products = () => {
  const [products, setProducts] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [filters, setFilters] = useState({
    minPrice: "",
    maxPrice: "",
    name: "",
    available: false,
    liked: false,
  });
  const [sortOption, setSortOption] = useState("name");

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

  const handleFilterChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFilters((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleSortChange = (e) => {
    setSortOption(e.target.value);
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  return (
    <div className="container-fluid">
      <div className="row">
        <div className="col-md-3">
          <div
            className="p-3 bg-dark text-light border rounded"
            style={{ minHeight: "100vh" }}
          >
            <h4 className="text-warning">Filters</h4>
            <div className="mb-3">
              <label className="form-label">Min Price</label>
              <input
                type="number"
                name="minPrice"
                className="form-control bg-secondary text-light border-0"
                value={filters.minPrice}
                onChange={handleFilterChange}
              />
            </div>
            <div className="mb-3">
              <label className="form-label">Max Price</label>
              <input
                type="number"
                name="maxPrice"
                className="form-control bg-secondary text-light border-0"
                value={filters.maxPrice}
                onChange={handleFilterChange}
              />
            </div>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input
                type="text"
                name="name"
                className="form-control bg-secondary text-light border-0"
                value={filters.name}
                onChange={handleFilterChange}
              />
            </div>
            <div className="form-check mb-3">
              <input
                type="checkbox"
                name="available"
                className="form-check-input"
                checked={filters.available}
                onChange={handleFilterChange}
              />
              <label className="form-check-label">Available</label>
            </div>
            <div className="form-check mb-3">
              <input
                type="checkbox"
                name="liked"
                className="form-check-input"
                checked={filters.liked}
                onChange={handleFilterChange}
              />
              <label className="form-check-label">Liked</label>
            </div>
            <h4 className="text-warning mt-4">Sort By</h4>
            <select
              className="form-select bg-secondary text-light border-0"
              value={sortOption}
              onChange={handleSortChange}
            >
              <option value="name">Name</option>
              <option value="price">Price</option>
            </select>
          </div>
        </div>

        <div className="col-md-9">
          <div className="row">
            {products.map((product) => (
              <div className="col-md-4" key={product.id}>
                <ProductCard
                  product={product}
                  onLikeToggle={() => toggleLike(product.id, product.liked)}
                />
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Products;
