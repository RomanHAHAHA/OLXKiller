import { useEffect, useState } from "react";
import { API_BASE_URL } from "../apiConfig";
import { ChevronDown, ChevronUp, Filter, X } from "lucide-react";

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
    const [selectedCategories, setSelectedCategories] = useState([]);
    const [orderBy, setOrderBy] = useState("");
    const [sortDirection, setSortDirection] = useState("0");
    const [isCollapsed, setIsCollapsed] = useState(false);

    useEffect(() => {
        const fetchCategories = async () => {
            const response = await fetch(categoriesUrl);
            if (response.ok) {
                const categories = (await response.json()).data;
                setCategories(categories);
            }
        };
        fetchCategories();
    }, []);

    useEffect(() => {
        onFilterChange({
            name,
            price,
            rating,
            isAvailable,
            filterMode, 
            categories: selectedCategories,
            sortParams: { orderBy, sortDirection }
        });
    }, [name, price, rating, isAvailable, filterMode, selectedCategories, orderBy, sortDirection]);

    const toggleCategory = (id) => {
        setSelectedCategories(prev =>
            prev.includes(id) ? prev.filter(c => c !== id) : [...prev, id]
        );
    };

    const toggleSortDirection = () => {
        setSortDirection(prev => (prev === "0" ? "1" : "0"));
    };

    const cycleFilterMode = () => {
        setFilterMode(prev => (prev + 1) % 3); // Циклически 0→1→2→0
    };

    const resetFilters = () => {
        setName("");
        setPrice("");
        setRating("");
        setIsAvailable(null);
        setFilterMode(FilterModes.AllProducts); // Сброс на 0
        setSelectedCategories([]);
        setOrderBy("");
        setSortDirection("0");
    };

    const getFilterModeButtonClass = (mode) => {
        return `btn ${filterMode === mode ? 'btn-primary' : 'btn-outline-secondary'}`;
    };

    const getFilterModeLabel = () => {
        switch(filterMode) {
            case FilterModes.MyProducts: return "Only My Products";
            case FilterModes.ExcludeMyProducts: return "Other Products";
            default: return "All Products";
        }
    };

    return (
        <div className={`bg-dark text-light p-4 rounded mb-4 ${isCollapsed ? 'py-2' : ''}`}>
            <div 
                className="d-flex justify-content-between align-items-center mb-3 cursor-pointer"
                onClick={() => setIsCollapsed(!isCollapsed)}
                style={{ cursor: 'pointer' }}
            >
                <h5 className="mb-0 d-flex align-items-center">
                    <Filter size={18} className="me-2" />
                    {isCollapsed ? 'Show Filters' : 'Product Filters'}
                </h5>
                <button 
                    className="btn btn-sm btn-outline-secondary"
                    onClick={(e) => {
                        e.stopPropagation();
                        resetFilters();
                    }}
                >
                    <X size={16} />
                </button>
            </div>

            {!isCollapsed && (
                <>
                    <div className="mb-3">
                        <label className="form-label text-light small">Product Name</label>
                        <input
                            type="text"
                            className="form-control bg-dark text-light border-secondary"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            placeholder="Search by name..."
                        />
                    </div>

                    <div className="row g-2 mb-3">
                        <div className="col">
                            <label className="form-label text-light small">Max Price</label>
                            <div className="input-group">
                                <span className="input-group-text bg-dark border-secondary text-light">₴</span>
                                <input
                                    type="number"
                                    className="form-control bg-dark text-light border-secondary"
                                    value={price}
                                    onChange={(e) => setPrice(e.target.value)}
                                    placeholder="0.00"
                                    min="0"
                                />
                            </div>
                        </div>
                        <div className="col">
                            <label className="form-label text-light small">Min Rating</label>
                            <div className="input-group">
                                <span className="input-group-text bg-dark border-secondary text-light">★</span>
                                <input
                                    type="number"
                                    className="form-control bg-dark text-light border-secondary"
                                    step="0.1"
                                    min="0"
                                    max="5"
                                    value={rating}
                                    onChange={(e) => setRating(e.target.value)}
                                    placeholder="0.0"
                                />
                            </div>
                        </div>
                    </div>

                    <div className="mb-3">
                        <div className="form-check form-switch">
                            <input
                                className="form-check-input bg-secondary"
                                type="checkbox"
                                role="switch"
                                checked={isAvailable === true}
                                onChange={(e) => setIsAvailable(e.target.checked || null)}
                                id="availableCheck"
                            />
                            <label className="form-check-label" htmlFor="availableCheck">
                                Only Available
                            </label>
                        </div>
                    </div>

                    <div className="mb-3">
                        <label className="form-label text-light small">Product Filter</label>
                        <div className="d-flex gap-2">
                           <button
                            type="button"
                            className="btn btn-outline-primary w-100"
                            onClick={cycleFilterMode}
                        >
                            {getFilterModeLabel()}
                        </button>
                        </div>
                    </div>

                    <div className="mb-3">
                        <label className="form-label text-light small">Categories</label>
                        <div 
                            className="d-flex flex-wrap gap-2"
                            style={{
                                maxHeight: '150px',
                                overflowY: 'auto',
                                padding: '2px'
                            }}
                        >
                            {categories.map(cat => (
                                <button
                                    key={cat.id}
                                    type="button"
                                    className={`btn btn-sm ${
                                        selectedCategories.includes(cat.id)
                                            ? 'btn-primary'
                                            : 'btn-outline-secondary'
                                    }`}
                                    onClick={() => toggleCategory(cat.id)}
                                    style={{
                                        borderRadius: '20px',
                                        whiteSpace: 'nowrap',
                                        padding: '0.25rem 0.75rem',
                                        fontSize: '0.875rem',
                                        lineHeight: '1.5'
                                    }}
                                >
                                    {cat.name}
                                </button>
                            ))}
                        </div>
                    </div>

                    <div className="row g-2 mb-3">
                        <div className="col-md-8">
                            <label className="form-label text-muted small">Sort By</label>
                            <select
                                className="form-select bg-dark text-light border-secondary"
                                value={orderBy}
                                onChange={(e) => setOrderBy(e.target.value)}
                            >
                                <option value="">Default</option>
                                <option value="Name">Name</option>
                                <option value="Price">Price</option>
                                <option value="AverageRating">Rating</option>
                            </select>
                        </div>
                        <div className="col-md-4 d-flex align-items-end">
                            <button
                                type="button"
                                className="btn btn-outline-primary w-100 d-flex align-items-center justify-content-center"
                                onClick={toggleSortDirection}
                            >
                                {sortDirection === "0" ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
                                <span className="ms-2">Order</span>
                            </button>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
};

export default ProductFilterPanel;