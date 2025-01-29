import React, { useEffect } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../utils/AuthContext";

const Menu = () => {
  const { isAuthorized, userData, login, logout } = useAuth();

  const checkAuthorization = async () => {
    try {
      const response = await fetch("https://localhost:7208/api/Accounts/is-authorized", {
        method: "GET",
        credentials: "include",
      });
      if (response.ok) {
        const userResponse = await fetch("https://localhost:7208/api/Users/get-view-data", {
          method: "GET",
          credentials: "include",
        });
        if (userResponse.ok) {
          const jsonResponse = await userResponse.json();
          login({
            nickName: jsonResponse.data.nickName,
            avatar64String: jsonResponse.data.avatar64String,
          });
        }
      } else {
        logout();
      }
    } catch (error) {
      console.error("Error checking authorization:", error);
      logout();
    }
  };

  useEffect(() => {
    checkAuthorization();
  }, []);

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark fixed-top">
      <div className="container">
        <Link className="navbar-brand" to="/">MyApp</Link>
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav ms-auto">
            {isAuthorized && (
              <li className="nav-item">
                <Link className="nav-link" to="/create-product">Create product</Link>
              </li>
            )}
            {!isAuthorized ? (
              <li className="nav-item">
                <Link className="nav-link" to="/login">Login</Link>
              </li>
            ) : (
              <>
                <li className="nav-item">
                  <Link className="nav-link" to="/profile">
                    {userData?.nickName || "Profile"}
                  </Link>
                </li>
                {userData?.avatar64String && (
                  <li className="nav-item">
                    <img
                      src={`data:image/png;base64,${userData.avatar64String}`}
                      alt="User Avatar"
                      className="user-avatar"
                      style={{ width: '30px', height: '30px', borderRadius: '50%' }}
                    />
                  </li>
                )}
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
};

export default Menu;
