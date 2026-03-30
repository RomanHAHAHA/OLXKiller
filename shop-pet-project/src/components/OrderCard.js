import { useState, useEffect } from 'react'; // ВАЖНО: импортируем useEffect
import productImagePlaceholder from '../asserts/imagePlaceholder.jpg';
import avatarPlaceholder from '../asserts/default_avatar_image.png';
import { API_BASE_URL } from '../apiConfig';
import styles from '../Styles/OrderCard.module.css';

const productImagesUrl = `${API_BASE_URL}product-images/`;
const userAvatarsUrl = `${API_BASE_URL}user-images/`;

const OrderCard = ({ order, onStatusChange, expandedOrderId, setExpandedOrderId }) => {
    const [isUpdating, setIsUpdating] = useState(false);
    const [error, setError] = useState(null);
    const [statusesList, setStatusesList] = useState([]);
    const [loadingStatuses, setLoadingStatuses] = useState(false);
    const [orderDetails, setOrderDetails] = useState(null);
    const [loadingDetails, setLoadingDetails] = useState(false);
    
    const currentStatus = order.lastStatus;
    const isExpanded = expandedOrderId === order.id;

    useEffect(() => {
        const fetchStatuses = async () => {
            setLoadingStatuses(true);
            try {
                const response = await fetch(`${API_BASE_URL}orders-api/api/order-statuses`, {
                    method: 'GET',
                    credentials: 'include'
                });
                
                if (!response.ok) {
                    throw new Error('Failed to fetch statuses');
                }
                
                const data = await response.json();
                setStatusesList(data.data || []);
            } catch (err) {
                console.error('Error fetching statuses:', err);
                setStatusesList([]);
            } finally {
                setLoadingStatuses(false);
            }
        };
        
        fetchStatuses();
    }, []);

    const fetchOrderDetails = async () => {
        if (orderDetails) return;
        
        setLoadingDetails(true);
        setError(null);
        
        try {
            const response = await fetch(`${API_BASE_URL}orders-api/api/orders/${order.id}/details`, {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const responseData = await response.json();
            console.log('Fetched order details:', responseData.data);
            setOrderDetails(responseData.data);
        } catch (err) {
            setError('Failed to load order details. Please try again.');
            console.error('Error fetching order details:', err);
        } finally {
            setLoadingDetails(false);
        }
    };

    const getStatusClass = (status) => {
        switch (status) {
            case 'Processing': return styles.statusProcessing;
            case 'Confirmed': return styles.statusConfirmed;
            case 'Shipped': return styles.statusShipped;
            case 'Delivered': return styles.statusDelivered;
            case 'Payed': return styles.statusPayed;
            case 'Received': return styles.statusReceived;
            case 'Canceled': return styles.statusCanceled;
            case 'Failed': return styles.statusFailed;
            default: return styles.statusProcessing;
        }
    };

    const toggleExpand = async () => {
        if (isExpanded) {
            setExpandedOrderId(null);
            setOrderDetails(null);
        } else {
            setExpandedOrderId(order.id);
            await fetchOrderDetails();
        }
    };

    const getStatusId = (statusName) => {
        if (!statusesList || statusesList.length === 0) return 0;
        const status = statusesList.find(s => s.name === statusName);
        return status ? status.id : 0;
    };

    const handleStatusChange = async (orderId, newStatus) => {
        const currentStatusId = getStatusId(currentStatus);
        const newStatusId = getStatusId(newStatus);
        
        if (newStatusId < currentStatusId) {
            setError('Cannot set status lower than current status');
            return;
        }
        
        setIsUpdating(true);
        setError(null);
        
        try {
            const response = await fetch(`${API_BASE_URL}orders-api/api/orders/${orderId}/${newStatus}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            await response.json();
            onStatusChange(orderId, newStatus);
        } catch (err) {
            setError('Failed to update order status. Please try again.');
            console.error('Error updating order status:', err);
        } finally {
            setIsUpdating(false);
        }
    };

    // Безопасное получение значений с защитой от undefined
    const safeNumber = (value) => {
        return Number(value || 0);
    };

    return (
        <div className={styles.card}>
            {error && <div className={styles.errorMessage}>{error}</div>}
            
            <div className={styles.header}>
                <div className={styles.orderId}>Order #{order.id}</div>
                <div className={styles.orderDate}>
                    {order.createdAt || 'No date'}
                </div>
            </div>
            
            <div className={styles.userSection}>
                <img 
                    src={order.user?.avatarName ? `${userAvatarsUrl}${order.user.avatarName}` : avatarPlaceholder} 
                    alt={order.user?.nickName || 'User'} 
                    className={styles.avatar}
                />
                <div className={styles.userInfo}>
                    <div className={styles.userName}>{order.user?.nickName || 'Anonymous'}</div>
                    <div className={styles.userId}>ID: {order.user.id}</div>
                </div>
            </div>
            
            <div className={`${styles.statusBadge} ${getStatusClass(currentStatus)}`}>
                {currentStatus || 'Unknown'}
            </div>
            
            {/* ИСПРАВЛЕНО: добавлен класс для развернутого состояния */}
            {isExpanded && (
                <div className={`${styles.contentSection} ${styles.contentSectionExpanded}`}>
                    {loadingDetails ? (
                        <div className={styles.loadingContainer}>
                            <span className={styles.loadingIndicator}>Loading order details...</span>
                        </div>
                    ) : orderDetails ? (
                        <>
                            {/* Товары */}
                            <h4 className={styles.sectionTitle}>
                                Items ({orderDetails.orderItems?.length || 0})
                            </h4>
                            {orderDetails.orderItems && orderDetails.orderItems.length > 0 ? (
                                <ul className={styles.itemList}>
                                    {orderDetails.orderItems.map((item, index) => (
                                        <li key={item.productId || index} className={styles.item}>
                                            <img 
                                                src={item.mainImagePath ? `${productImagesUrl}${item.mainImagePath}` : productImagePlaceholder} 
                                                alt={item.name || 'Product'} 
                                                className={styles.itemImage}
                                                onError={(e) => {
                                                    e.target.src = productImagePlaceholder;
                                                }}
                                            />
                                            <div className={styles.itemDetails}>
                                                <div className={styles.itemName}>{item.name || 'Unknown Product'}</div>
                                                <div className={styles.itemPrice}>
                                                    <span>${safeNumber(item.fixedPrice).toFixed(2)}</span>
                                                    <span>×</span>
                                                    <span>{item.quantity || 0}</span>
                                                    <span className={styles.itemTotal}>
                                                        = ${(safeNumber(item.fixedPrice) * (item.quantity || 0)).toFixed(2)}
                                                    </span>
                                                </div>
                                            </div>
                                        </li>
                                    ))}
                                </ul>
                            ) : (
                                <div className={styles.emptyState}>No items in this order</div>
                            )}
                            
                            {/* Доставка */}
                            <h4 className={styles.sectionTitle}>Delivery</h4>
                            {orderDetails.deliveryLocation ? (
                                <div className={styles.deliveryInfo}>
                                    <div className={styles.deliveryText}>
                                        {[
                                            orderDetails.deliveryLocation.region,
                                            orderDetails.deliveryLocation.city
                                        ].filter(Boolean).join(', ') || 'No region/city'}
                                    </div>
                                    <div className={styles.deliverySubtext}>
                                        {orderDetails.deliveryLocation.warehouse || 'No warehouse information'}
                                    </div>
                                </div>
                            ) : (
                                <div className={styles.emptyState}>No delivery information</div>
                            )}
                            
                            {/* История статусов */}
                            <h4 className={styles.sectionTitle}>Status History</h4>
                            {orderDetails.statusesHistory && orderDetails.statusesHistory.length > 0 ? (
                                <ul className={styles.statusHistory}>
                                    {orderDetails.statusesHistory.map((status, index) => (
                                        <li key={index} className={styles.statusItem}>
                                            <span className={`${styles.statusLabel} ${getStatusClass(status.status)}`}>
                                                {status.status || 'Unknown'}
                                            </span>
                                            <span className={styles.statusDate}>
                                                {status.createdAt || 'No date'}
                                            </span>
                                        </li>
                                    ))}
                                </ul>
                            ) : (
                                <div className={styles.emptyState}>No status history</div>
                            )}
                        </>
                    ) : (
                        <div className={styles.emptyState}>No details available</div>
                    )}
                </div>
            )}
            
            <div className={styles.footer}>
                <div className={styles.totalPrice}>
                    ${safeNumber(order.totalPrice).toFixed(2)}
                </div>
                
                <div className={styles.controls}>
                    <button 
                        onClick={toggleExpand}
                        className={styles.detailsButton}
                        disabled={loadingDetails}
                    >
                        {loadingDetails ? 'Loading...' : (isExpanded ? 'Hide Details' : 'Show Details')}
                    </button>
                    
                    {loadingStatuses ? (
                        <span className={styles.loadingIndicator}>Loading statuses...</span>
                    ) : (
                        <select
                            className={styles.statusSelect}
                            value={currentStatus || ''}
                            onChange={(e) => handleStatusChange(order.id, e.target.value)}
                            disabled={isUpdating}
                        >
                            {statusesList && statusesList.length > 0 ? (
                                statusesList
                                    .filter(status => status.id >= getStatusId(currentStatus))
                                    .map(status => (
                                        <option key={status.id} value={status.name}>
                                            {status.name}
                                        </option>
                                    ))
                            ) : (
                                <option value={currentStatus}>{currentStatus || 'No status'}</option>
                            )}
                        </select>
                    )}
                    {isUpdating && <span className={styles.loadingIndicator}>Updating...</span>}
                </div>
            </div>
        </div>
    );
};

export default OrderCard;