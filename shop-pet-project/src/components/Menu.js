import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../AuthProvider";  
import imagePlaceholder from "../asserts/default_avatar_image.png";
import { API_BASE_URL } from "../apiConfig";
import { ShoppingCart, Menu as MenuIcon } from "lucide-react"; 
import CartPopup from "../components/CartPopup";
import "../Styles/Menu.css";

const avatarUrl = `${API_BASE_URL}user-images/`;

const Menu = () => {
  const { user } = useAuth();  
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [showCart, setShowCart] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  
  useEffect(() => {
    setIsAuthorized(!!user);
  }, [user]);

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen);
  };

  return (
    <nav className="menu">
      <div className="menu-container">
        <Link className="menu-brand" to="/">Home</Link>

        <button className="menu-toggle" onClick={toggleMobileMenu}>
          <MenuIcon size={24} />
        </button>

        <ul className={`menu-items ${mobileMenuOpen ? 'open' : ''}`}>
          {user?.permissions?.length > 0 && user.permissions.includes("ManageOrders") && (
            <li>
              <Link className="menu-link" to="/admin-orders" onClick={() => setMobileMenuOpen(false)}>
                Orders
              </Link>
            </li>
          )}
          {user?.permissions?.length > 0&& user.permissions.includes("ManageCategories") && (
            <li>
              <Link className="menu-link" to="/categories" onClick={() => setMobileMenuOpen(false)}>
                Categories
              </Link>
            </li>
          )}
          {user?.permissions?.length > 0 && user.permissions.includes("ManageCategories") && (
            <li>
              <Link className="menu-link" to="/admin-reviews" onClick={() => setMobileMenuOpen(false)}>
                Reviews
              </Link>
            </li>
          )}
          
          <li>
            <Link className="menu-link" to="/create-product" onClick={() => setMobileMenuOpen(false)}>
              Create Product
            </Link>
          </li>

          <li>
            <button
              className="cart-button"
              onClick={() => {
                setShowCart(prev => !prev);
                setMobileMenuOpen(false);
              }}
            >
              <ShoppingCart className="cart-icon" size={20} />
            </button>
            {showCart && <CartPopup onClose={() => setShowCart(false)} />}
          </li>

          {isAuthorized && user ? (
            <li className="user-avatar">
              <Link 
                className="menu-link" 
                to="/profile/avatar"
                onClick={() => setMobileMenuOpen(false)}
              >
                {user.nickName}
              </Link>
              <img
                src={user.avatarImageName ? `${avatarUrl}${user.avatarImageName}` : imagePlaceholder}
                onError={(e) => { e.currentTarget.src = imagePlaceholder }}
                alt="avatar"
                className="avatar-image"
              />
            </li>
          ) : (
            <li>
              <Link 
                className="menu-link" 
                to="/login"
                onClick={() => setMobileMenuOpen(false)}
              >
                Login
              </Link>
            </li>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default Menu;