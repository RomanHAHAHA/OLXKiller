import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../utils/AuthContext";

const Menu = () => {
  const { isAuthorized, userData, login, logout } = useAuth();
  const [canAssignRole, setCanAssignRole] = useState(false);

  const checkAuthorization = async () => {
    try {
      const response = await fetch("https://localhost:7208/api/Accounts/is-authorized", {
        method: "GET",
        credentials: "include",
      });

      if (!response.ok) {
        logout();
        return;
      }

      const userResponse = await fetch("https://localhost:7208/api/Users/get-view-data", {
        method: "GET",
        credentials: "include",
      });

      if (userResponse.ok) {
        const jsonResponse = await userResponse.json();

        login({
          nickName: jsonResponse.data.nickName,
          avatarBytes: jsonResponse.data.avatarBytes,  
        });

        checkPermission("AssignRoleToUser");
      }
    } catch (error) {
      console.error("Error checking authorization:", error);
      logout();
    }
  };

  const checkPermission = async (permission) => {
    try {
      const response = await fetch(`https://localhost:7208/api/Permissions/has-permission?permission=${permission}`, {
        method: "GET",
        credentials: "include",
      });

      setCanAssignRole(response.ok);
    } catch (error) {
      console.error("Error checking permission:", error);
      setCanAssignRole(false);
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
            {canAssignRole && (
              <li className="nav-item">
                <Link className="nav-link" to="/users">Users</Link>
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
                {userData?.avatarBytes && (
                  <li className="nav-item">
                    <img
                      src={`data:image/png;base64,${userData.avatarBytes}`}
                      alt="User Avatar"
                      className="user-avatar"
                      style={{ width: '40px', height: '40px', borderRadius: '50%' }}
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
