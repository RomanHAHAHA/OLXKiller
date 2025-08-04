import { useState } from 'react';
import { Filter, X, ChevronUp, ChevronDown } from 'lucide-react';

const ReviewFilterPanel = ({ filters, onFilterChange, sorting, onSortChange }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    
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
        onSortChange({
            ...sorting,
            sortDirection: sorting.sortDirection === 0 ? 1 : 0
        });
    };
    
    const resetFilters = () => {
        onFilterChange({
            productId: undefined,
            userId: undefined,
            rate: undefined,
            status: undefined,
            dateFrom: undefined,
            dateTo: undefined
        });
    };

    return (
        <div className={`bg-dark text-light p-4 rounded mb-4 ${isCollapsed ? 'py-2' : ''}`}>
            <div 
                className="d-flex justify-content-between align-items-center mb-3 cursor-pointer"
                onClick={() => setIsCollapsed(!isCollapsed)}
            >
                <h5 className="mb-0 d-flex align-items-center">
                    <Filter size={18} className="me-2" />
                    {isCollapsed ? 'Show Filters' : 'Review Filters'}
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
                        <label className="form-label text-light small">Min Rating</label>
                        <select
                            className="form-select bg-dark text-light border-secondary"
                            name="rate"
                            value={filters.rate || ''}
                            onChange={handleInputChange}
                        >
                            <option value="">Any Rating</option>
                            <option value="1">1+</option>
                            <option value="2">2+</option>
                            <option value="3">3+</option>
                            <option value="4">4+</option>
                            <option value="5">5</option>
                        </select>
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Review Status</label>
                        <select
                            className="form-select bg-dark text-light border-secondary"
                            name="status"
                            value={filters.status || ''}
                            onChange={handleInputChange}
                        >
                            <option value="">Any Status</option>
                            <option value="0">Pending</option>
                            <option value="1">Approved</option>
                            <option value="2">Rejected</option>
                        </select>
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Date from</label>
                        <input
                            type="date"
                            className="form-control bg-dark text-light border-secondary"
                            name="dateFrom"
                            value={filters.dateFrom ? filters.dateFrom.split('T')[0] : ''}
                            onChange={handleDateChange}
                            placeholder="From"
                        />
                    </div>

                    <div>
                        <label className="form-label text-light small">Date to</label>
                        <input
                            type="date"
                            className="form-control bg-dark text-light border-secondary"
                            name="dateTo"
                            value={filters.dateTo ? filters.dateTo.split('T')[0] : ''}
                            onChange={handleDateChange}
                            placeholder="To"
                        />
                    </div>
                    
                    <div>
                        <label className="form-label text-light small">Sort By</label>
                        <div className="input-group">
                            <select
                                className="form-select bg-dark text-light border-secondary"
                                name="orderBy"
                                value={sorting.orderBy || ''}
                                onChange={(e) => onSortChange({
                                    ...sorting,
                                    orderBy: e.target.value
                                })}
                            >
                                <option value="СreatedAt">Date</option>
                                <option value="Rate">Rating</option>
                            </select>
                            <button 
                                className="btn btn-outline-primary border-secondary"
                                onClick={toggleSortDirection}
                            >
                                {sorting.sortDirection === 0 ? <ChevronUp size={18} /> : <ChevronDown size={18} />}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ReviewFilterPanel;