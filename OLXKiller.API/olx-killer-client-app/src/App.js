import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Login from "./components/Login";
import Register from "./components/Register";
import Profile from "./components/Profile";
import Menu from "./components/Menu";
import { AuthProvider } from "./utils/AuthContext";
import Products from "./components/Products";
import CreateProductForm from "./components/CreateProductForm";
import ProductInfo from "./components/ProductInfo";
import Avatar from "./components/Avatar"; // Добавляем компонент Avatar
import Users from "./components/Users";

function App() {
  return (
    <AuthProvider>
      <Router>
        <div>
          <Menu />
          <div className="container mt-5 pt-4">
            <Routes>
              <Route path="/" element={<Products />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/users" element={<Users />} />
              <Route path="/profile/*" element={<Profile />}> 
                <Route path="avatar" element={<Avatar />} /> 
              </Route>
              <Route path="/create-product" element={<CreateProductForm />} />
              <Route path="/products/:productId" element={<ProductInfo />} />
            </Routes>
          </div>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
