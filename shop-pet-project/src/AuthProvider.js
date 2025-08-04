import { createContext, useContext, useState, useEffect } from "react";
import { API_BASE_URL } from "./apiConfig.js"; 
import Swal from "sweetalert2";

const AuthContext = createContext(null);
const claimsUrl = `${API_BASE_URL}users-api/api/users/get-claims`;
const logoutUrl = `${API_BASE_URL}users-api/api/accounts/logout`;
const refreshTokenUrl = `${API_BASE_URL}users-api/api/accounts/refresh-token`;

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);

  const fetchUser = async () => {
    try {
      const response = await fetch(claimsUrl, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        setUser({
          userId: data.userCookiesData.userId,
          nickName: data.userCookiesData.nickName,
          role: data.userCookiesData.role,
          avatarImageName: data.userCookiesData.avatarImageName,
          permissions: data.userCookiesData.permissions || [],
        });
      } else {
        setUser(null);
      }
    } catch (error) {
      setUser(null);
    }
  };

  const refreshToken = async () => {
    try {
      const response = await fetch(refreshTokenUrl, { 
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        await fetchUser(); 
      } else {
        setUser(null);
      }
    } catch (error) {
      setUser(null);
    }
  };

  const logout = async () => {
    try {
      const response = await fetch(`${logoutUrl}`, { method: "DELETE", credentials: 'include' });
      if (!response.ok) {
        console.log("Logout error");
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: 'Unexpected error occured during the request'
      })
    }
    setUser(null); 
  };

  useEffect(() => {
    fetchUser(); 
  }, []);

  return (
    <AuthContext.Provider value={{ user, setUser, logout, fetchUser, refreshToken }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

