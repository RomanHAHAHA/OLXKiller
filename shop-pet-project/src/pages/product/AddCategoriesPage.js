import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { API_BASE_URL } from "../../apiConfig";
import Swal from "sweetalert2";
import { FaArrowLeft, FaArrowRight } from "react-icons/fa";
import "../../Styles/CategoriesAdmin.css";

const AddCategoriesPage = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    
    const [allCategories, setAllCategories] = useState([]);
    const [selectedCategoryId, setSelectedCategoryId] = useState(null);
    const [loading, setLoading] = useState(true);
    const [categoryLoading, setCategoryLoading] = useState(false);
    const [expandedNodes, setExpandedNodes] = useState(new Set());

    const fetchAllCategories = async () => {
        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/categories`);
            if (!response.ok) throw new Error("Failed to fetch categories");
            const categories = (await response.json()).data; 
            setAllCategories(categories);
            
            // Auto-expand root nodes
            const rootIds = categories.map(cat => cat.id);
            setExpandedNodes(new Set(rootIds));
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
        } finally {
            setLoading(false);
        }
    };

    const fetchProductCategory = async () => {
        try {
            const response = await fetch(
                `${API_BASE_URL}products-api/api/products/${productId}/category`
            );
            if (!response.ok) throw new Error("Failed to fetch product category");
            
            const category = (await response.json()).data;
            console.log("Product category:", category.id); // Отладка
            setSelectedCategoryId(category?.id || null);
        } catch (err) {
            console.log("No category selected");
            setSelectedCategoryId(null);
        }
    };

    const setProductCategory = async (categoryId) => {
        const url = `${API_BASE_URL}products-api/api/products/${productId}/category?categoryId=${categoryId}`;

        try {
            setCategoryLoading(true);
            const response = await fetch(url, { 
                method: "PATCH", 
                credentials: "include" 
            });

            if (!response.ok) throw new Error("Failed to set category");

            setSelectedCategoryId(categoryId);
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3",
                background: "#1e1e2d",
                color: "#fff"
            });
        } finally {
            setCategoryLoading(false);
        }
    };

    const removeProductCategory = async () => {
        const url = `${API_BASE_URL}products-api/api/products/${productId}/category`;

        try {
            setCategoryLoading(true);
            const response = await fetch(url, { 
                method: "PATCH", 
                credentials: "include" 
            });
            
            var message = await response.json();
            console.log("Remove category response:", message); // Отладка
            if (!response.ok) throw new Error("Failed to remove category");

            setSelectedCategoryId(null);
        } catch (err) {
            Swal.fire({
                title: "Error",
                text: err.message,
                icon: "error",
                confirmButtonColor: "#4ecca3",
                background: "#1e1e2d",
                color: "#fff"
            });
        } finally {
            setCategoryLoading(false);
        }
    };

    const handleCategorySelect = (categoryId) => {
        if (selectedCategoryId === categoryId) {
            removeProductCategory();
        } else {
            setProductCategory(categoryId);
        }
    };

    const handleBack = () => navigate(`/products/${productId}/update`);
    const handleContinue = () => {
        navigate(`/products/${productId}/add-images`);
    };

    const toggleExpand = (categoryId) => {
        setExpandedNodes(prev => {
            const newSet = new Set(prev);
            if (newSet.has(categoryId)) {
                newSet.delete(categoryId);
            } else {
                newSet.add(categoryId);
            }
            return newSet;
        });
    };

    // Раскрываем родителей для выбранной категории
    useEffect(() => {
        if (selectedCategoryId && allCategories.length > 0) {
            const findAndExpandParents = (categories, targetId) => {
                for (const cat of categories) {
                    if (cat.id === targetId) {
                        return true;
                    }
                    if (cat.children && cat.children.length > 0) {
                        if (findAndExpandParents(cat.children, targetId)) {
                            setExpandedNodes(prev => new Set([...prev, cat.id]));
                            return true;
                        }
                    }
                }
                return false;
            };
            findAndExpandParents(allCategories, selectedCategoryId);
        }
    }, [selectedCategoryId, allCategories]);

    useEffect(() => {
        fetchAllCategories();
        fetchProductCategory();
    }, [productId]);

    const renderCategoryTree = (category, level = 0) => {
        const hasChildren = category.children && category.children.length > 0;
        const isExpanded = expandedNodes.has(category.id);
        const paddingLeft = level === 0 ? 8 : level * 24;
        const isSelected = selectedCategoryId === category.id; // ПРОСТОЕ СРАВНЕНИЕ ID
        const isLeaf = !hasChildren;

        return (
            <div key={category.id} className="category-tree-node">
                <div 
                    className="category-row"
                    style={{ 
                        paddingLeft: `${paddingLeft}px`,
                        backgroundColor: isSelected ? 'rgba(78, 204, 163, 0.15)' : 'transparent',
                        borderLeft: isSelected ? '3px solid #4ecca3' : 'none'
                    }}
                >
                    <div className="category-info">
                        {hasChildren && (
                            <button 
                                className="expand-btn"
                                onClick={() => toggleExpand(category.id)}
                                type="button"
                            >
                                {isExpanded ? (
                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                    <polyline points="6 9 12 15 18 9"></polyline>
                                </svg>
                                ) : (
                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                    <polyline points="9 18 15 12 9 6"></polyline>
                                </svg>
                                )}
                            </button>
                        )}
                        {!hasChildren && <span className="expand-placeholder"></span>}
                        <div className="category-details">
                            <span style={{ 
                                color: isSelected ? '#4ecca3' : 'var(--text)',
                                fontWeight: isSelected ? 'bold' : 'normal'
                            }}>
                                {category.name}
                            </span>
                        </div>
                    </div>
                    <div className="actions">
                        {isLeaf && (
                            <button 
                                className={`category-select-btn ${isSelected ? 'selected' : ''}`}
                                onClick={() => handleCategorySelect(category.id)}
                                disabled={categoryLoading}
                            >
                                {isSelected ? '✓ Selected' : 'Select'}
                            </button>
                        )}
                    </div>
                </div>
                {hasChildren && isExpanded && (
                    <div className="category-children">
                        {category.children.map(child => renderCategoryTree(child, level + 1))}
                    </div>
                )}
            </div>
        );
    };

    if (loading) return <div className="loading">Loading categories...</div>;

    return (
        <div className="categories-admin">
            <div className="categories-header">
                <h2>Product Category</h2>
            </div>

            <div className="categories-tree-container">
                {allCategories.length === 0 ? (
                    <div className="empty-state">
                        <p>No categories available</p>
                    </div>
                ) : (
                    allCategories.map(category => renderCategoryTree(category))
                )}
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
                    className="btn-accent"
                    onClick={handleContinue}
                >
                    Continue
                    <FaArrowRight className="ms-2" />
                </button>
            </div>
        </div>
    );
};

export default AddCategoriesPage;