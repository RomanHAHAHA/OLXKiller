import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { API_BASE_URL } from "../../apiConfig";
import Swal from "sweetalert2";
import { FaArrowLeft, FaArrowRight } from "react-icons/fa";

const AddCategoriesPage = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    
    const [allCategories, setAllCategories] = useState([]);
    const [productCategories, setProductCategories] = useState(new Set());
    const [loading, setLoading] = useState({
        categories: true,
        productCategories: true
    });
    const [categoryLoading, setCategoryLoading] = useState(null);

    const fetchAllCategories = async () => {
        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/categories`);
            if (!response.ok) throw new Error("Failed to fetch categories");
            const categories = (await response.json()).data; 
            setAllCategories(categories);
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
        } finally {
            setLoading(prev => ({ ...prev, categories: false }));
        }
    };

    const fetchProductCategories = async () => {
        try {
            const response = await fetch(
                `${API_BASE_URL}products-api/api/products/${productId}/categories`
            );
            if (!response.ok) throw new Error("Failed to fetch product categories");
            
            const categories = (await response.json()).data; 
            setProductCategories(new Set(categories.map(c => c.id)));
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
        } finally {
            setLoading(prev => ({ ...prev, productCategories: false }));
        }
    };

    const toggleCategory = async (categoryId) => {
        const isSelected = productCategories.has(categoryId);
        const method = isSelected ? "DELETE" : "POST";
        const url = `${API_BASE_URL}products-api/api/products/${productId}/categories/${categoryId}`;

        try {
            setCategoryLoading(categoryId);
            const response = await fetch(url, { method, credentials: "include" });

            if (!response.ok) throw new Error("Operation failed");

            setProductCategories(prev => {
                const updated = new Set(prev);
                isSelected ? updated.delete(categoryId) : updated.add(categoryId);
                return updated;
            });
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
        } finally {
            setCategoryLoading(null);
        }
    };

    const handleBack = () => navigate(`/products/${productId}/update`);
    const handleContinue = () => {
        if (productCategories.size === 0) {
            Swal.fire({
                title: "No categories selected",
                text: "Are you sure you want to continue without categories?",
                icon: "question",
                showCancelButton: true,
                confirmButtonColor: "#4ecca3",
                cancelButtonColor: "#ff4444",
                confirmButtonText: "Continue anyway",
                cancelButtonText: "Select categories"
            }).then((result) => {
                if (result.isConfirmed) {
                    navigate(`/products/${productId}/add-images`);
                }
            });
        } else {
            navigate(`/products/${productId}/add-images`);
        }
    };

    useEffect(() => {
        fetchAllCategories();
        fetchProductCategories();
    }, [productId]);

    const isLoading = loading.categories || loading.productCategories;

    return (
        <div className="container mt-5" style={{ maxWidth: '800px' }}>
            <div className="text-center mb-4">
                <h2 className="text-light mb-0" style={{ color: "#4ecca3" }}>
                    Product Categories
                </h2>
                <div style={{ width: "100px" }}></div> 
            </div>

            {isLoading ? (
                <div className="text-center py-5">
                    <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>
            ) : (
                <>
                    <div className="list-group mb-4">
                        {allCategories.map((category) => (
                            <label 
                                key={category.id} 
                                className={`list-group-item d-flex align-items-center bg-dark text-light`}
                                style={{ transition: "all 0.3s ease" }}
                            >
                                <div className="form-check form-switch me-3">
                                    <input
                                        type="checkbox"
                                        className="form-check-input"
                                        checked={productCategories.has(category.id)}
                                        disabled={categoryLoading === category.id}
                                        onChange={() => toggleCategory(category.id)}
                                        style={{ 
                                            cursor: "pointer",
                                            backgroundColor: productCategories.has(category.id) 
                                                ? "#4ecca3" 
                                                : "",
                                            borderColor: productCategories.has(category.id) 
                                                ? "#4ecca3" 
                                                : ""
                                        }}
                                    />
                                </div>
                                <span className="flex-grow-1">{category.name}</span>
                                {categoryLoading === category.id && (
                                    <div className="spinner-border spinner-border-sm text-light ms-2" 
                                         role="status">
                                        <span className="visually-hidden">Loading...</span>
                                    </div>
                                )}
                            </label>
                        ))}
                    </div>
                    
                    <div className="d-flex justify-content-between mt-4">
                        <button 
                            onClick={handleBack}
                            className="btn-accent-outline"
                        >
                            <FaArrowLeft className="me-2" />
                            Back
                        </button>
                        
                        <button 
                            className="btn-accent-outline"
                            onClick={handleContinue}
                            disabled={categoryLoading !== null}
                        >
                            Continue
                            <FaArrowRight className="ms-2" />
                        </button>
                    </div>
                </>
            )}
        </div>
    );
};

export default AddCategoriesPage;