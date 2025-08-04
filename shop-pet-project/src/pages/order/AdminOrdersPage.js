import { useState, useEffect } from 'react';
import OrderFilterPanel from '../../components/OrderFilterPanel';
import OrderCard from '../../components/OrderCard';
import Pagination from '../../components/Pagination';
import { API_BASE_URL } from '../../apiConfig';
import '../../Styles/AdminOrders.css';

const AdminOrdersPage = () => {
    const [filters, setFilters] = useState({});
    const [pageParams, setPageParams] = useState({ page: 1, pageSize: 8 });
    const [expandedOrderId, setExpandedOrderId] = useState(null);
    const [ordersData, setOrdersData] = useState({
        items: [],
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const fetchOrders = async () => {
        setLoading(true);
        setError(null);
        
        try {
            const params = new URLSearchParams();
            
            Object.entries(filters).forEach(([key, value]) => {
                if (value !== undefined && value !== '') {
                    params.append(key, value.toString());
                }
            });
            
            params.append('page', pageParams.page.toString());
            params.append('pageSize', pageParams.pageSize.toString());
            
            const response = await fetch(`${API_BASE_URL}orders-api/api/orders?${params.toString()}`, {
                method: 'GET',
                credentials: 'include'
            });
            
            if (!response.ok) {
                throw new Error('Failed to fetch orders');
            }
            
            const pagedResult = (await response.json()).data;
            setOrdersData(pagedResult);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, [filters, pageParams]);

    const handlePageChange = (newPage) => {
        setPageParams(prev => ({ ...prev, page: newPage }));
    };

    const onStatusChange = async () => {
        fetchOrders();
    };

    return (
        <div className="admin-orders-container">
            <div className="filter-panel-wrapper">
                <OrderFilterPanel 
                    filters={filters} 
                    onFilterChange={setFilters} 
                />
            </div>
            
            <div className="orders-content">
                {loading && <div className="loading-indicator">Loading orders...</div>}
                {error && <div className="error-message">{error}</div>}
                
                <div className="orders-list">
                    {ordersData.items.map(order => (
                        <OrderCard 
                            key={order.id}
                            order={order}
                            onStatusChange={onStatusChange}
                            expandedOrderId={expandedOrderId}
                            setExpandedOrderId={setExpandedOrderId}
                        />
                    ))}
                </div>
                
                <Pagination 
                    currentPage={pageParams.page}
                    pageSize={pageParams.pageSize}
                    totalCount={ordersData.totalCount}
                    onPageChange={handlePageChange}
                />
            </div>
        </div>
    );
};

export default AdminOrdersPage;