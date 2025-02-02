import React, { useEffect, useState } from "react";
import { FaEdit } from "react-icons/fa";
import ReactModal from "react-modal";
import "../styles/UsersCardStyle.css";
import "../styles/ChangeRolePopupStyle.css";

const Users = () => {
  const [users, setUsers] = useState([]);
  const [selectedRole, setSelectedRole] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [currentUserId, setCurrentUserId] = useState(null);

  const FETCH_USERS_URL = "https://localhost:7208/api/Users/grouped-by-role";
  const ASSIGN_ROLE_TO_USER_URL = "https://localhost:7208/api/Roles/assign-to-user";

  const fetchUsers = async () => {
    try {
      const response = await fetch(FETCH_USERS_URL, { method: "GET", credentials: "include" });
      const data = await response.json();
      setUsers(data.data ?? []);
    } catch (err) {
      console.log(err.message);
    }
  };

  const handleRoleChange = async () => {
    if (!selectedRole) return;
    try {
      const response = await fetch(`${ASSIGN_ROLE_TO_USER_URL}/${currentUserId}/${selectedRole}`, {
        method: "POST",
        credentials: "include",
      });
      if (!response.ok) {
        throw new Error("Failed to assign role.");
      }
      setShowModal(false);
      fetchUsers();
    } catch (err) {
      console.log(err.message);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  return (
    <div className="users-container">
      <h2>Users</h2>
      <div className="row">
        {users.map((user) => (
          <div className="col-md-4" key={user.id}>
            <div className="card mb-4 user-card">
              <div className="card-body d-flex justify-content-between align-items-center">
                <div className="user-info d-flex align-items-center">
                  <img src={`data:image/png;base64,${user.avatarBytes}`} alt="User Avatar" className="user-avatar" />
                  <div className="ml-3">
                    <h5 className="card-title">{user.nickName}</h5>
                    <p className="card-role">{user.roleName}</p>
                  </div>
                </div>
                <button
                  className="btn btn-outline-light btn-sm role-change-btn"
                  onClick={() => {
                    setCurrentUserId(user.id);
                    setSelectedRole(user.roleName.toLowerCase() === 'user' ? 1 : 3);
                    setShowModal(true);
                  }}
                >
                  <FaEdit />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      <ReactModal isOpen={showModal} onRequestClose={() => setShowModal(false)} ariaHideApp={false}>
        <h4>Select a new role</h4>
        <div className="role-selection-container">
          <div
            className={`role-card ${selectedRole === 3 ? 'selected' : ''}`}
            onClick={() => setSelectedRole(3)}
          >
            <h5>Moderator</h5>
            <p>Can manage content and users.</p>
          </div>
          <div
            className={`role-card ${selectedRole === 1 ? 'selected' : ''}`}
            onClick={() => setSelectedRole(1)}
          >
            <h5>User</h5>
            <p>Regular user without admin rights.</p>
          </div>
        </div>
        <div className="mt-3">
          <button onClick={handleRoleChange} className="btn btn-primary" disabled={!selectedRole}>
            Confirm
          </button>
          <button className="btn btn-secondary ml-2" onClick={() => setShowModal(false)}>
            Close
          </button>
        </div>
      </ReactModal>

    </div>
  );
};

export default Users;
