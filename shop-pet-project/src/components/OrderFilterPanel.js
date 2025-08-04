import { useState, useEffect } from 'react';
import { Filter, X, ChevronUp, ChevronDown } from 'lucide-react';
import { API_BASE_URL } from '../apiConfig';

const OrderFilterPanel = ({ filters, onFilterChange }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const [orderStatuses, setOrderStatuses] = useState([]);
    
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        onFilterChange({
            ...filters,
            [name]: value !== '' ? value : undefined
        });
    };
    
    const handleDateChange = (e) => {
        const { name, value } = e.target;
        onFilterChange({
            ...filters,
            [name]: value ? new Date(value).toISOString() : undefined
        });
    };
    
    const toggleSortDirection = () => {
        onFilterChange({
            ...filters,
            sortOrder: filters.sortOrder === 0 ? 1 : 0
        });
    };
    
    const resetFilters = () => {
        onFilterChange({});
    };

    useEffect(() => {
        const fetchStatuses = async () => {
            try {
                const response = await fetch(`${API_BASE_URL}orders-api/api/order-statuses`, {
                    method: 'GET',
                    credentials: 'include'
                });
                
                if (!response.ok) {
                    throw new Error('Failed to fetch statuses');
                }
                
                const data = await response.json();
                setOrderStatuses(data.data);
            } catch (err) {
                console.error('Error fetching statuses:', err);
            } 
        };
        
        fetchStatuses();
    }, []);

    return (
        <div className={`bg-dark text-light p-4 rounded mb-4 border border-secondary ${isCollapsed ? 'py-2' : ''}`}>
            <div 
                className="d-flex justify-content-between align-items-center mb-3 cursor-pointer"
                onClick={() => setIsCollapsed(!isCollapsed)}
            >
                <h5 className="mb-0 d-flex align-items-center">
                    <Filter size={18} className="me-2" />
                    {isCollapsed ? 'Show Filters' : 'Order Filters'}
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
                <div className="d-flex flex-column gap-3">
                    <div>
                        <label className="form-label text-light small">User ID</label>
                        <input
                            type="text"
                            className="form-control bg-dark text-light border-secondary"
                            name="userId"
                            value={filters.userId || ''}
                            onChange={handleInputChange}
                            placeholder="Filter by user ID"
                        />
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Product ID</label>
                        <input
                            type="text"
                            className="form-control bg-dark text-light border-secondary"
                            name="productId"
                            value={filters.productId || ''}
                            onChange={handleInputChange}
                            placeholder="Filter by product ID"
                        />
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Status</label>
                        <select
                            className="form-select bg-dark text-light border-secondary"
                            name="orderStatus"
                            value={filters.orderStatus || ''}
                            onChange={handleInputChange}
                        >
                            <option value="">All Statuses</option>
                            {orderStatuses.map(status => (
                                <option key={status.value} value={status.value}>
                                    {status.name}
                                </option>
                            ))}
                        </select>
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Min Price</label>
                        <div className="input-group">
                            <span className="input-group-text bg-dark border-secondary text-light">$</span>
                            <input
                                type="number"
                                className="form-control bg-dark text-light border-secondary"
                                name="minPrice"
                                value={filters.minPrice || ''}
                                onChange={handleInputChange}
                                placeholder="0.00"
                                min="0"
                                step="0.01"
                            />
                        </div>
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Date from</label>
                        <div className="d-flex gap-2">
                            <input
                                type="date"
                                className="form-control bg-dark text-light border-secondary"
                                name="startDate"
                                value={filters.startDate ? filters.startDate.split('T')[0] : ''}
                                onChange={handleDateChange}
                                placeholder="From"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="form-label text-light small">Date to</label>
                        <div className="d-flex gap-2">
                            <input
                                type="date"
                                className="form-control bg-dark text-light border-secondary"
                                name="endDate"
                                value={filters.endDate ? filters.endDate.split('T')[0] : ''}
                                onChange={handleDateChange}
                                placeholder="To"
                            />
                        </div>
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Sort By</label>
                        <div className="input-group">
                            <select
                                className="form-select bg-dark text-light border-secondary"
                                name="sortBy"
                                value={filters.sortBy || ''}
                                onChange={handleInputChange}
                            >
                                <option value="">Default</option>
                                <option value="createdAt">Date</option>
                                <option value="totalPrice">Total Price</option>
                            </select>
                            <button 
                                className="btn btn-outline-primary border-secondary"
                                onClick={toggleSortDirection}
                                disabled={!filters.sortBy}
                            >
                                {filters.sortOrder === 0 ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default OrderFilterPanel;