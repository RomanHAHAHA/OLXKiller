import { useEffect, useState } from "react";
import { API_BASE_URL } from "../apiConfig";
import { useSignalR } from "../SignalRProvider";
import Swal from "sweetalert2";

const baseCartUrl = `${API_BASE_URL}carts-api/api/carts/`;
const getUserCartUrl = `${baseCartUrl}my`;

export const useCart = () => {
  const { connection } = useSignalR();
  const [cartItems, setCartItems] = useState([]);
  const [totalPrice, setTotalPrice] = useState(0);

  const fetchCart = async () => {
    try {
      const response = await fetch(getUserCartUrl, { credentials: "include" });
      const data = await response.json();
      setCartItems(data.data.cartItems || []);
      setTotalPrice(data.data.totalCartPrice || 0);
    } catch (error) {
      console.error("Error fetching cart:", error);
    }
  };

  const updateQuantity = async (productId, action) => {
    try {
      const url = `${baseCartUrl}${productId}/${action}`;
      await fetch(url, {
        method: "PATCH",
        credentials: "include",
      });
      await fetchCart();
    } catch (error) {
      console.error(`Error updating quantity: ${action}`, error);
    }
  };

  const removeItem = async (productId) => {
    try {
      await fetch(`${baseCartUrl}${productId}`, {
        method: "DELETE",
        credentials: "include",
      });
      await fetchCart();
    } catch (error) {
      console.error("Error removing item", error);
    }
  };

  useEffect(() => {
      if (!connection) return;

      const handlePorductExceeded = (stockQuantity) => {
          Swal.fire({
              title: "Warning",
              text: `There is only ${stockQuantity} items on stock`,
              icon: 'warning',
          });
      };

      connection.on("NotifyProductStockExceeded", handlePorductExceeded);

      return () => {
          connection.off("NotifyProductStockExceeded", handlePorductExceeded);
      };
  }, [connection]);

  useEffect(() => {
    fetchCart();
  }, []);

  return { cartItems, totalPrice, updateQuantity, removeItem };
};
