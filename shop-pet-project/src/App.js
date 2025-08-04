import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Menu from "./components/Menu";
import Login from "./pages/profile/Login";
import Register from "./pages/profile/Register";
import Products from "./pages/product/Products";
import Profile from "./pages/profile/Profile";
import ConfirmEmailWrapper from "./components/ConfirmEmailWrapper";
import CreateOrderForm from "./forms/CreateOrderForm";
import ChangeAvatar from "./pages/profile/ChangeAvatar";
import Logs from "./pages/profile/Logs";
import ProductPage from "./pages/product/ProductPage";
import CreateProductPage from "./pages/product/CreateProductPage";
import AddCategoriesPage from "./pages/product/AddCategoriesPage";
import AddImagesPage from "./pages/product/AddImagesPage";
import AddCharacteristicsPage from "./pages/product/AddCharacteristicsPage";
import UpdateProductPage from "./pages/product/UpdateProductPage";
import MyOrdersPage from "./pages/order/MyOrdersPage";
import CategoriesAdminPage from "./pages/category/Categories";
import AdminOrdersPage from "./pages/order/AdminOrdersPage";
import ReviewsModerationPage from "./pages/review/ReviewsModerationPage";
import Chats from "./pages/profile/Chats";
import ChatWindow from "./components/ChatWindow";
import NotificationContainer from "./components/NotificationContainer";
import { useNotifications } from "./context/NotificationContext";
import { useNavigate } from "react-router-dom";

function App() {
  const { notifications, clearAllNotifications } = useNotifications(); 
  const navigate = useNavigate();

  const handleNotificationClick = (chatId) => {
    navigate(`/profile/chats/${chatId}`);
    clearAllNotifications(); 
  };

  return (
    <div>
      <Menu />
      <div className="container mt-5 pt-4">
        <Routes>
          <Route path="/" element={<Products />} />
          <Route path="/create-product" element={<CreateProductPage />} />
          <Route path="/products/:productId/update" element={<UpdateProductPage />} />
          <Route path="/products/:productId/add-categories" element={<AddCategoriesPage />} />
          <Route path="/products/:productId/add-images" element={<AddImagesPage />} />
          <Route path="/products/:productId/add-characteristics" element={<AddCharacteristicsPage />} />
          <Route path="/products/:productId" element={<ProductPage />} />

          <Route path="categories" element={<CategoriesAdminPage/>} />
          
          <Route path="/admin-orders" element={<AdminOrdersPage />} />
          <Route path="/create-order" element={<CreateOrderForm />} />
          <Route path="/admin-reviews" element={<ReviewsModerationPage />} />
          
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/confirm-email" element={<ConfirmEmailWrapper />} />   
          <Route path="/profile" element={<Profile />}>
            <Route path="avatar" element={<ChangeAvatar />} />
            <Route path="logs" element={<Logs />} />
            <Route path="my-orders" element={<MyOrdersPage/>}/>
            <Route path="chats" element={<Chats/>}/>
            <Route path="chats/:chatId" element={<ChatWindow/>}/>
          </Route>
        </Routes>
      </div>
      
      <NotificationContainer
        notifications={notifications}
        onClose={clearAllNotifications} 
        onClick={handleNotificationClick}
      />
    </div>
  );
}

export default App;