import { useEffect, useState } from "react";
import { API_BASE_URL } from "../apiConfig";
import { ChevronDown, ChevronUp, Filter, X } from "lucide-react";
import '../Styles/ProductFilterPanel.css';

const categoriesUrl = `${API_BASE_URL}products-api/api/categories`;

const ProductFilterPanel = ({ onFilterChange }) => {
    const FilterModes = {
        AllProducts: 0,         
        MyProducts: 1,          
        ExcludeMyProducts: 2    
    };
    
    const [name, setName] = useState("");
    const [price, setPrice] = useState("");
    const [rating, setRating] = useState("");
    const [isAvailable, setIsAvailable] = useState(null);
    const [filterMode, setFilterMode] = useState(FilterModes.AllProducts); 
    const [categories, setCategories] = useState([]);
    const [selectedCategoryIds, setSelectedCategoryIds] = useState(new Set());
    const [orderBy, setOrderBy] = useState("");
    const [sortDirection, setSortDirection] = useState("0");
    const [isCollapsed, setIsCollapsed] = useState(false);
    const [expandedNodes, setExpandedNodes] = useState(new Set());

    useEffect(() => {
        const fetchCategories = async () => {
            const response = await fetch(categoriesUrl);
            if (response.ok) {
                const categoriesData = (await response.json()).data;
                setCategories(categoriesData);
                const rootIds = categoriesData.map(cat => cat.id);
                setExpandedNodes(new Set(rootIds));
            }
        };
        fetchCategories();
    }, []);

    const getAllSubcategoryIds = (category) => {
        let ids = [category.id];
        if (category.children && category.children.length > 0) {
            category.children.forEach(child => {
                ids = [...ids, ...getAllSubcategoryIds(child)];
            });
        }
        return ids;
    };

    const findCategoryById = (cats, id) => {
        for (const cat of cats) {
            if (cat.id === id) return cat;
            if (cat.children) {
                const found = findCategoryById(cat.children, id);
                if (found) return found;
            }
        }
        return null;
    };

    const toggleCategory = (categoryId) => {
        const category = findCategoryById(categories, categoryId);
        if (!category) return;

        const allIds = getAllSubcategoryIds(category);
        
        setSelectedCategoryIds(prev => {
            const newSet = new Set(prev);
            if (newSet.has(categoryId)) {
                allIds.forEach(id => newSet.delete(id));
            } else {
                allIds.forEach(id => newSet.add(id));
            }
            return newSet;
        });
    };

    const isCategorySelected = (category) => {
        const allIds = getAllSubcategoryIds(category);
        const anySelected = allIds.some(id => selectedCategoryIds.has(id));
        const allSelected = allIds.every(id => selectedCategoryIds.has(id));
        
        if (allSelected) return 'checked';
        if (anySelected) return 'indeterminate';
        return false;
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

    useEffect(() => {
        const selectedIdsArray = Array.from(selectedCategoryIds);
        
        onFilterChange({
            name,
            price: price ? parseFloat(price) : undefined,
            rating: rating ? parseFloat(rating) : undefined,
            isAvailable: isAvailable,
            filterMode,
            categoryIds: selectedIdsArray,
            sortParams: { orderBy, sortDirection }
        });
    }, [name, price, rating, isAvailable, filterMode, selectedCategoryIds, orderBy, sortDirection, onFilterChange]);

    const toggleSortDirection = () => {
        setSortDirection(prev => (prev === "0" ? "1" : "0"));
    };

    const cycleFilterMode = () => {
        setFilterMode(prev => (prev + 1) % 3);
    };

    const resetFilters = () => {
        setName("");
        setPrice("");
        setRating("");
        setIsAvailable(null);
        setFilterMode(FilterModes.AllProducts);
        setSelectedCategoryIds(new Set());
        setOrderBy("");
        setSortDirection("0");
    };

    const getFilterModeLabel = () => {
        switch(filterMode) {
            case FilterModes.MyProducts: return "Only My Products";
            case FilterModes.ExcludeMyProducts: return "Other Products";
            default: return "All Products";
        }
    };

    const renderCategoryTree = (category, level = 0) => {
    const hasChildren = category.children && category.children.length > 0;
    const isExpanded = expandedNodes.has(category.id);
    const selectionState = isCategorySelected(category);
    const isChecked = selectionState === 'checked';
    const isIndeterminate = selectionState === 'indeterminate';

    return (
        <div key={category.id} className="cat-item">
            <div className="cat-row" style={{ marginLeft: level * 20 }}>
                {hasChildren && (
                    <span 
                        className="cat-expand" 
                        onClick={() => toggleExpand(category.id)}
                        style={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            width: '18px',
                            height: '18px',
                            cursor: 'pointer',
                            color: '#92929f',
                            flexShrink: 0
                        }}
                    >
                        {isExpanded ? (
                            <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ display: 'block' }}>
                                <polyline points="6 9 12 15 18 9"></polyline>
                            </svg>
                        ) : (
                            <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ display: 'block' }}>
                                <polyline points="9 18 15 12 9 6"></polyline>
                            </svg>
                        )}
                    </span>
                )}
                {!hasChildren && <span style={{ width: '18px', flexShrink: 0 }}></span>}
                <label 
                    className="cat-label"
                    style={{
                        display: 'flex',
                        alignItems: 'center',
                        cursor: 'pointer',
                        margin: 0,
                        fontSize: '13px',
                        color: '#e1e1e8'
                    }}
                >
                    <input
                        type="checkbox"
                        className="cat-checkbox"
                        checked={isChecked}
                        ref={el => {
                            if (el) el.indeterminate = isIndeterminate;
                        }}
                        onChange={() => toggleCategory(category.id)}
                        style={{
                            width: '14px',
                            height: '14px',
                            margin: '0 10px 0 0',
                            padding: 0,
                            cursor: 'pointer',
                            accentColor: '#4ecca3',
                            flexShrink: 0
                        }}
                    />
                    <span style={{ lineHeight: 1 }}>{category.name}</span>
                    {category.productCount > 0 && (
                        <small style={{ fontSize: '10px', color: '#92929f', marginLeft: '4px', lineHeight: 1 }}>
                            ({category.productCount})
                        </small>
                    )}
                </label>
            </div>
            {hasChildren && isExpanded && (
                <div>
                    {category.children.map(child => renderCategoryTree(child, level + 1))}
                </div>
            )}
        </div>
    );
};
    return (
        <div className="filter-panel">
            <div className="filter-header" onClick={() => setIsCollapsed(!isCollapsed)}>
                <h5>
                    <Filter size={16} />
                    {isCollapsed ? 'Show Filters' : 'Filters'}
                </h5>
                <button className="reset-btn" onClick={(e) => { e.stopPropagation(); resetFilters(); }}>
                    <X size={14} />
                </button>
            </div>

            {!isCollapsed && (
                <div className="filter-body">
                    <div className="filter-field">
                        <label>Product Name</label>
                        <input type="text" value={name} onChange={(e) => setName(e.target.value)} placeholder="Search..." />
                    </div>

                    <div className="filter-row">
                        <div className="filter-field">
                            <label>Max Price</label>
                            <input type="number" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="0" />
                        </div>
                        <div className="filter-field">
                            <label>Min Rating</label>
                            <input type="number" step="0.1" min="0" max="5" value={rating} onChange={(e) => setRating(e.target.value)} placeholder="0" />
                        </div>
                    </div>

                    <div className="filter-field">
                        <label className="inline-label">
                            <input type="checkbox" checked={isAvailable === true} onChange={(e) => setIsAvailable(e.target.checked || null)} />
                            Only Available
                        </label>
                    </div>

                    <div className="filter-field">
                        <label>Product Filter</label>
                        <button className="mode-btn" onClick={cycleFilterMode}>
                            {getFilterModeLabel()}
                        </button>
                    </div>

                    <div className="filter-field">
                        <label>Categories</label>
                        <div className="cat-container">
                            {categories.map(category => renderCategoryTree(category))}
                        </div>
                    </div>

                    <div className="filter-row">
                        <div className="filter-field">
                            <label>Sort By</label>
                            <select value={orderBy} onChange={(e) => setOrderBy(e.target.value)}>
                                <option value="">Default</option>
                                <option value="Name">Name</option>
                                <option value="Price">Price</option>
                                <option value="AverageRating">Rating</option>
                            </select>
                        </div>
                        <div className="filter-field">
                            <label>Order</label>
                            <button className="order-btn" onClick={toggleSortDirection}>
                                {sortDirection === "0" ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                                {sortDirection === "0" ? "Asc" : "Desc"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ProductFilterPanel;