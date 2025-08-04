import { useState, useEffect } from 'react';
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
    
    const currentStatus = order.statuses[order.statuses.length - 1]?.status || 'Processing';
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
                setStatusesList(data.data);
            } catch (err) {
                console.error('Error fetching statuses:', err);
            } finally {
                setLoadingStatuses(false);
            }
        };
        
        fetchStatuses();
    }, []);

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

    const toggleExpand = () => {
        if (isExpanded) {
            setExpandedOrderId(null);
        } else {
            setExpandedOrderId(order.id);
        }
    };

    const getStatusId = (statusName) => {
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

            const data = await response.json();
            onStatusChange(orderId, newStatus);
        } catch (err) {
            setError('Failed to update order status. Please try again.');
            console.error('Error updating order status:', err);
        } finally {
            setIsUpdating(false);
        }
    };

    return (
        <div className={styles.card}>
            {error && <div className={styles.errorMessage}>{error}</div>}
            
            <div className={styles.header}>
                <div className={styles.orderId}>Order #{order.id}</div>
                <div className={styles.orderDate}>
                    {order.createdAt}
                </div>
            </div>
            
            <div className={styles.userSection}>
                <img 
                    src={order.user.avatarName ? `${userAvatarsUrl}${order.user.avatarName}` : avatarPlaceholder} 
                    alt={order.user.nickName} 
                    className={styles.avatar}
                />
                <div className={styles.userInfo}>
                    <div className={styles.userName}>{order.user.nickName}</div>
                    <div className={styles.userId}>ID: {order.user.id}</div>
                </div>
            </div>
            
            <div className={`${styles.statusBadge} ${getStatusClass(currentStatus)}`}>
                {currentStatus}
            </div>
            
            <div className={`${styles.contentSection} ${isExpanded ? styles.contentExpanded : ''}`}>
                <h4 className={styles.sectionTitle}>Items ({order.orderItems.length})</h4>
                <ul className={styles.itemList}>
                    {order.orderItems.map(item => (
                        <li key={item.productId} className={styles.item}>
                            <img 
                                src={item.mainImagePath ? `${productImagesUrl}${item.mainImagePath}` : productImagePlaceholder} 
                                alt={item.name} 
                                className={styles.itemImage}
                            />
                            <div className={styles.itemDetails}>
                                <div className={styles.itemName}>{item.name}</div>
                                <div className={styles.itemPrice}>
                                    <span>${item.fixedPrice.toFixed(2)}</span>
                                    <span>×</span>
                                    <span>{item.quantity}</span>
                                </div>
                            </div>
                        </li>
                    ))}
                </ul>
                
                <h4 className={styles.sectionTitle}>Delivery</h4>
                <div className={styles.deliveryInfo}>
                    <div className={styles.deliveryText}>
                        {order.deliveryLocation.region}, {order.deliveryLocation.city}
                    </div>
                    <div className={styles.deliverySubtext}>
                        {order.deliveryLocation.warehouse}
                    </div>
                </div>
                
                <h4 className={styles.sectionTitle}>Status History</h4>
                <ul className={styles.statusHistory}>
                    {order.statuses.map((status, index) => (
                        <li key={index} className={styles.statusItem}>
                            <span className={`${styles.statusLabel} ${getStatusClass(status.status)}`}>
                                {status.status}
                            </span>
                            <span className={styles.statusDate}>
                                {status.createdAt}
                            </span>
                        </li>
                    ))}
                </ul>
            </div>
            
            <div className={styles.footer}>
                <div className={styles.totalPrice}>
                    ${order.totalPrice.toFixed(2)}
                </div>
                
                <div className={styles.controls}>
                    <button 
                        onClick={toggleExpand}
                        className={styles.detailsButton}
                    >
                        {isExpanded ? 'Hide Details' : 'Show Details'}
                    </button>
                    
                    {loadingStatuses ? (
                        <span className={styles.loadingIndicator}>Loading statuses...</span>
                    ) : (
                        <select
                            className={styles.statusSelect}
                            value={currentStatus}
                            onChange={(e) => handleStatusChange(order.id, e.target.value)}
                            disabled={isUpdating}
                        >
                            {statusesList
                                .filter(status => status.id >= getStatusId(currentStatus))
                                .map(status => (
                                    <option key={status.id} value={status.name}>
                                        {status.name}
                                    </option>
                                ))}
                        </select>
                    )}
                    {isUpdating && <span className={styles.loadingIndicator}>Updating...</span>}
                </div>
            </div>
        </div>
    );
};

export default OrderCard;