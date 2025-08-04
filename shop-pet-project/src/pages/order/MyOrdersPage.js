import { useEffect, useState } from "react";
import { API_BASE_URL } from "../../apiConfig";
import { useAuth } from "../../AuthProvider";
import { Truck, CheckCircle, Clock, XCircle, Loader2, ChevronDown, ChevronUp } from "lucide-react";
import imagePlaceholder from "../../asserts/imagePlaceholder.jpg";
import styles from '../../Styles/MyOrdersPage.module.css';

const ORDERS_URL = `${API_BASE_URL}orders-api/api/orders/my`;

const statusIcons = {
    'Processing': <Loader2 className={`${styles.statusIcon} ${styles.spinning}`} />,
    'Confirmed': <CheckCircle className={styles.statusIcon} />,
    'Shipped': <Truck className={styles.statusIcon} />,
    'Delivered': <CheckCircle className={styles.statusIcon} />,
    'Payed': <CheckCircle className={styles.statusIcon} />,
    'Received': <CheckCircle className={styles.statusIcon} />,
    'Canceled': <XCircle className={styles.statusIcon} />,
    'Failed': <XCircle className={styles.statusIcon} />, 
    'Created': <Clock className={styles.statusIcon} />
};

const statusColors = {
    'Processing': 'var(--status-processing)',
    'Confirmed': 'var(--status-confirmed)',
    'Shipped': 'var(--status-shipped)',
    'Delivered': 'var(--status-delivered)',
    'Payed': 'var(--status-payed)',
    'Received': 'var(--status-received)',
    'Canceled': 'var(--status-canceled)',
    'Failed': 'var(--status-failed)',
    'Created': 'var(--status-created)'
};

// Функция для преобразования статуса в читаемый формат
const formatStatus = (status) => {
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
};

const MyOrdersPage = () => {
    const { user } = useAuth();
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [expandedOrderId, setExpandedOrderId] = useState(null);

    const fetchOrders = async () => {
        try {
            const response = await fetch(ORDERS_URL, {
                method: 'GET',
                credentials: 'include'
            });
            
            if (!response.ok) {
                throw new Error('Failed to fetch orders');
            }
            
            const data = await response.json();
            setOrders(data.data || []);
        } catch (err) {
            console.error('Error fetching orders:', err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (user) {
            fetchOrders();
        }
    }, [user]);

    const toggleOrderExpand = (orderId) => {
        setExpandedOrderId(expandedOrderId === orderId ? null : orderId);
    };

    const getLatestStatus = (statuses) => {
        if (!statuses || statuses.length === 0) return 'Unknown';
        const latestStatus = statuses[statuses.length - 1]?.status;
        return formatStatus(latestStatus);
    };

    if (loading) {
        return (
            <div className={styles.loadingContainer}>
                <Loader2 className={styles.spinner} size={48} />
            </div>
        );
    }

    if (error) {
        return (
            <div className={styles.errorContainer}>
                <p className={styles.errorText}>{error}</p>
                <button 
                    className={styles.retryButton}
                    onClick={fetchOrders}
                >
                    Retry
                </button>
            </div>
        );
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>My Orders</h1>
            
            {orders.length === 0 ? (
                <div className={styles.emptyState}>
                    <Truck size={48} className={styles.emptyIcon} />
                    <h2>No orders yet</h2>
                    <p>Your orders will appear here once you make a purchase</p>
                </div>
            ) : (
                <div className={styles.ordersList}>
                    {orders.map(order => (
                        <div 
                            key={order.id} 
                            className={styles.orderCard}
                            onClick={() => toggleOrderExpand(order.id)}
                        >
                            <div className={styles.orderHeader}>
                                <div className={styles.orderInfo}>
                                    <span className={styles.orderId}>Order #{order.id.slice(0, 8)}</span>
                                    <span className={styles.orderDate}>{order.createdAt}</span>
                                </div>
                                
                                <div className={styles.orderStatus}>
                                    <span 
                                        className={styles.statusBadge}
                                        style={{ backgroundColor: statusColors[getLatestStatus(order.statuses)] }}
                                    >
                                        {statusIcons[getLatestStatus(order.statuses)]}
                                        {getLatestStatus(order.statuses)}
                                    </span>
                                </div>
                                
                                <div className={styles.orderTotal}>
                                    {order.totalPrice.toFixed(2)} UAH
                                </div>
                                
                                <button 
                                    className={styles.expandButton}
                                    aria-expanded={expandedOrderId === order.id}
                                >
                                    {expandedOrderId === order.id ? <ChevronUp /> : <ChevronDown />}
                                </button>
                            </div>
                            
                            {expandedOrderId === order.id && (
                                <div className={styles.orderDetails}>
                                    <div className={styles.deliveryInfo}>
                                        <h3>Delivery Information</h3>
                                        <p>
                                            <strong>Region:</strong> {order.deliveryLocation.region}<br />
                                            <strong>City:</strong> {order.deliveryLocation.city}<br />
                                            <strong>Warehouse:</strong> {order.deliveryLocation.warehouse}
                                        </p>
                                    </div>
                                    
                                    <div className={styles.statusTimeline}>
                                        <h3>Order Status History</h3>
                                        <ul>
                                            {order.statuses.map((status, index) => {
                                                const formattedStatus = formatStatus(status.status);
                                                return (
                                                    <li key={index} className={styles.statusItem}>
                                                        <div className={styles.statusDot} 
                                                            style={{ backgroundColor: statusColors[formattedStatus] }} />
                                                        <div className={styles.statusContent}>
                                                            <span className={styles.statusName}>
                                                                {formattedStatus}
                                                            </span>
                                                            <span className={styles.statusDate}>
                                                                {status.createdAt}
                                                            </span>
                                                        </div>
                                                    </li>
                                                );
                                            })}
                                        </ul>
                                    </div>
                                    
                                    <div className={styles.itemsSection}>
                                        <h3>Items ({order.orderItems.length})</h3>
                                        <ul className={styles.itemsList}>
                                            {order.orderItems.map(item => (
                                                <li key={item.productId} className={styles.orderItem}>
                                                    <img
                                                        src={item.mainImagePath 
                                                            ? `${API_BASE_URL}product-images/${item.mainImagePath}`
                                                            : imagePlaceholder}
                                                        alt={item.name}
                                                        className={styles.itemImage}
                                                        onError={(e) => {
                                                            e.currentTarget.src = imagePlaceholder;
                                                        }}
                                                    />
                                                    <div className={styles.itemDetails}>
                                                        <h4>{item.name}</h4>
                                                        <div className={styles.itemPrice}>
                                                            {item.fixedPrice.toFixed(2)} UAH × {item.quantity}
                                                        </div>
                                                    </div>
                                                    <div className={styles.itemTotal}>
                                                        {(item.fixedPrice * item.quantity).toFixed(2)} UAH
                                                    </div>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default MyOrdersPage;