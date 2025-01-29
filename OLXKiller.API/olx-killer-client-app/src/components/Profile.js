import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import { FaUserShield, FaImage, FaClipboardList } from "react-icons/fa";

const Profile = () => {
  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <table className="w-full h-full">
        <tbody>
          <tr>
            <td className="w-1/4 p-5 bg-gray-800 shadow-lg">
              <h2 className="text-xl font-semibold text-center text-yellow-400 mb-6">Profile</h2>
              <nav className="flex flex-col">
                <NavItem to="/profile/security" icon={<FaUserShield />} label="Security" />
                <NavItem to="/profile/avatar" icon={<FaImage />} label="Avatar" />
                <NavItem to="/profile/listings" icon={<FaClipboardList />} label="My Listings" />
              </nav>
            </td>
            <td className="flex-1 p-8 overflow-auto bg-gray-700">
              <Outlet />
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  );
};

const NavItem = ({ to, icon, label }) => (
  <div className="w-full mb-4"> {/* Добавлен отступ между кнопками */}
    <NavLink
      to={to}
      className={({ isActive }) =>
        `flex items-center w-full p-4 rounded-md transition duration-300
        ${isActive ? "bg-yellow-500 text-gray-900 shadow-md" : "bg-gray-800 text-white hover:bg-gray-600"}
        hover:scale-105 hover:shadow-lg focus:outline-none`
      }
    >
      <span className="mr-3 text-xl">{icon}</span>
      <span className="text-lg font-medium">{label}</span>
    </NavLink>
  </div>
);

export default Profile;
