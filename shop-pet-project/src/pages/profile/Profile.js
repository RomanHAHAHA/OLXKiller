import { useNavigate, Outlet, useLocation, NavLink } from "react-router-dom";
import { useAuth } from "../../AuthProvider";
import styles from "../../Styles/Profile.module.css";

const Profile = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  const allNavItems = [
    { label: "Profile", path: "/profile/avatar", icon: "bi-person", end: true },
    { label: "My Orders", path: "/profile/my-orders", icon: "bi-cart" },
    { label: "Reviews", path: "/profile/reviews", icon: "bi-star" },
    { label: "Chats", path: "/profile/chats", icon: "bi-star" },
    { label: "System Logs", path: "/profile/logs", icon: "bi-journal-text", requiredPermission: "ManageActionLogs" },
  ];

  const navItems = allNavItems.filter(item => {
    if (!item.requiredPermission) return true;
    return user?.permissions?.includes(item.requiredPermission);
  });

  return (
    <div className={styles.container}>
      <aside className={styles.sidebar}>
        <nav className={styles.navList}>
          {navItems.map((item) => {
            const isActive = location.pathname === item.path || 
                            (item.end && location.pathname.startsWith(item.path));
            return (
              <button
                key={item.path}
                className={`${styles.navButton} ${isActive ? styles.navButtonActive : ''}`}
                onClick={() => navigate(item.path)}
              >
                <i className={`bi ${item.icon}`}></i>
                <span>{item.label}</span>
                <i className="bi bi-chevron-right"></i>
              </button>
            );
          })}
        </nav>

        <button className={styles.logoutButton} onClick={handleLogout}>
          <i className="bi bi-box-arrow-right"></i>
          <span>Log out</span>
        </button>
      </aside>

      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  );
};

export default Profile;