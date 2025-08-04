import { useEffect, useState } from "react";
import { API_BASE_URL } from "../apiConfig";
import Swal from "sweetalert2";
import { Plus, Minus, Loader2 } from "lucide-react";
import imagePlaceholder from "../asserts/imagePlaceholder.jpg";
import DeliverySelector from "../components/DeliverySelector";
import { useSignalR } from "../SignalRProvider";
import styles from '../Styles/OrderPage.module.css';

const cartUrl = `${API_BASE_URL}carts-api/api/carts/my`;
const imageUrl = `${API_BASE_URL}product-images/`;

const OrderPage = () => {
    const { connection } = useSignalR();
    const [deliverySelection, setDeliverySelection] = useState({
        region: null,
        city: null,
        warehouse: null
    });
    const [cartItems, setCartItems] = useState([]);
    const [stockInfoByProductId, setStockInfoByProductId] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isLoading, setIsLoading] = useState(true);

    const fetchCart = async () => {
        try {
            const res = await fetch(cartUrl, { 
                method: 'GET', 
                credentials: "include" 
            });
            const data = await res.json();
            setCartItems(data?.data.cartItems || []);
        } catch (error) {
            console.error("Failed to fetch cart:", error);
            Swal.fire("Error", "Failed to load your cart", "error");
        } finally {
            setIsLoading(false);
        }
    };

    const updateQuantity = async (productId, action) => {
        try {
            const endpoint = `${API_BASE_URL}carts-api/api/carts/${productId}/${action}`;
            await fetch(endpoint, {
                method: "PATCH",
                credentials: "include",
            });
            await fetchCart();
        } catch (error) {
            console.error("Failed to update quantity:", error);
            Swal.fire("Error", "Failed to update quantity", "error");
        }
    };

    const placeOrder = async (e) => {
        e.preventDefault();

        const { region, city, warehouse } = deliverySelection;

        if (!region?.ref || !city?.ref || !warehouse?.ref) {
            Swal.fire({
                title: "Delivery Info Required",
                text: "Please select region, city and warehouse",
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
            return;
        }

        if (cartItems.length === 0) {
            Swal.fire({
                title: "Empty Cart",
                text: "Your cart is empty",
                icon: "warning",
                confirmButtonColor: "#4ecca3"
            });
            return;
        }

        setIsSubmitting(true);
        try {
            const body = {
                region: region.description,
                city: city.description,
                warehouse: warehouse.description
            };

            const response = await fetch(`${API_BASE_URL}orders-api/api/orders`, {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || "Failed to place order");
            }
        } catch (error) {
            console.error("Order error:", error);
            Swal.fire({
                title: "Error",
                text: error.message,
                icon: "error",
                confirmButtonColor: "#4ecca3"
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    useEffect(() => {
        fetchCart();
    }, []);

    useEffect(() => {
        if (!connection) return;

        const handleOrderProcessed = (message) => {
            Swal.fire({
                title: "Order Placed!",
                text: message,
                icon: 'success',
                confirmButtonColor: "#4ecca3"
            }).then(() => {
                setCartItems([]);
            });
        };

        const handleReservationFailed = (error) => {
            const stockMap = {};
            error.forEach(info => {
                stockMap[info.productId] = info.stockQuantity;
            });

            setStockInfoByProductId(stockMap);

            Swal.fire({
                title: 'Stock Update',
                text: 'Some products have limited availability',
                icon: 'warning',
                confirmButtonColor: "#4ecca3"
            });
        };

        connection.on("NotifyOrderProcessed", handleOrderProcessed);
        connection.on("NotifyProductsReservationFailed", handleReservationFailed);

        return () => {
            connection.off("NotifyOrderProcessed", handleOrderProcessed);
            connection.off("NotifyProductsReservationFailed", handleReservationFailed);
        };
    }, [connection]);

    const total = cartItems.reduce((sum, item) => sum + item.totalPrice, 0);

    if (isLoading) {
        return (
            <div className={styles.loadingContainer}>
                <Loader2 className={styles.spinner} size={48} />
            </div>
        );
    }

    return (
        <div className={styles.container}>            
            <form onSubmit={placeOrder} className={styles.orderForm}>
                <div className={styles.deliverySection}>
                    <h3 className={styles.sectionTitle}>Delivery Information</h3>
                    <DeliverySelector 
                        onSelectionChange={setDeliverySelection} 
                        className={styles.deliverySelector}
                    />
                </div>

                <div className={styles.cartSection}>
                    <h3 className={styles.sectionTitle}>Your Order</h3>
                    
                    {cartItems.length === 0 ? (
                        <div className={styles.emptyCart}>
                            <p>Your cart is empty</p>
                        </div>
                    ) : (
                        <ul className={styles.cartItems}>
                            {cartItems.map((item) => (
                                <li key={item.product.id} className={styles.cartItem}>
                                    <div className={styles.productImage}>
                                        <img
                                            src={item.product.mainImagePath 
                                                ? `${imageUrl}${item.product.mainImagePath}?_t=${Date.now()}`
                                                : imagePlaceholder}
                                            alt={item.product.name}
                                            onError={(e) => {
                                                e.currentTarget.src = imagePlaceholder;
                                            }}
                                        />
                                    </div>
                                    
                                    <div className={styles.productDetails}>
                                        <h4 className={styles.productName}>{item.product.name}</h4>
                                        
                                        <div className={styles.quantityControls}>
                                            <button
                                                type="button"
                                                className={styles.quantityButton}
                                                onClick={() => updateQuantity(item.product.id, "decrement")}
                                                disabled={item.quantity === 1}
                                                aria-label="Decrease quantity"
                                            >
                                                <Minus size={16} />
                                            </button>
                                            <span className={styles.quantity}>{item.quantity}</span>
                                            <button
                                                type="button"
                                                className={styles.quantityButton}
                                                onClick={() => updateQuantity(item.product.id, "increment")}
                                                aria-label="Increase quantity"
                                            >
                                                <Plus size={16} />
                                            </button>
                                        </div>
                                        
                                        <div className={styles.priceInfo}>
                                            {item.quantity} × {item.product.price.toFixed(2)} UAH
                                        </div>
                                        
                                        {stockInfoByProductId[item.product.id] !== undefined && (
                                            <div className={styles.stockWarning}>
                                                Only {stockInfoByProductId[item.product.id]} left in stock
                                            </div>
                                        )}
                                    </div>
                                    
                                    <div className={styles.totalPrice}>
                                        {item.totalPrice.toFixed(2)} UAH
                                    </div>
                                </li>
                            ))}
                        </ul>
                    )}
                    
                    <div className={styles.orderTotal}>
                        <span>Total:</span>
                        <span>{total.toFixed(2)} UAH</span>
                    </div>
                </div>

                <button 
                    className={styles.submitButton} 
                    type="submit" 
                    disabled={isSubmitting || cartItems.length === 0}
                >
                    {isSubmitting ? (
                        <>
                            <Loader2 className={styles.buttonSpinner} size={18} />
                            Processing...
                        </>
                    ) : "Place Order"}
                </button>
            </form>
        </div>
    );
};

export default OrderPage;