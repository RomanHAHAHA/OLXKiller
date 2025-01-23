import React, { createContext, useState, useContext } from "react";

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [userData, setUserData] = useState(null);

  const login = (user) => {
    setIsAuthorized(true);
    setUserData(user);
  };

  const logout = () => {
    setIsAuthorized(false);
    setUserData(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthorized, userData, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
