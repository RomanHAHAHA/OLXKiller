import Swal from 'sweetalert2';
import { useNavigate } from 'react-router-dom';
import "./Styles/AuthAlert.css";

const useAuthAlert = () => {
  const navigate = useNavigate();

  const showAuthAlert = (customOptions = {}) => {
    Swal.fire({
      title: 'Authorization Required',
      text: 'You need to log in to perform this action',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Log In',
      cancelButtonText: 'Cancel',
      background: '#1f2937', 
      color: '#f3f4f6', 
      customClass: {
        popup: 'dark:bg-gray-800',
        title: 'dark:text-white',
        htmlContainer: 'dark:text-gray-200',
        confirmButton: 'swal-dark-confirm',
        cancelButton: 'swal-dark-cancel'
      },
      buttonsStyling: false,
      ...customOptions
    }).then((result) => {
      if (result.isConfirmed) {
        navigate('/login', { state: { from: window.location.pathname } }); 
      }
    });
  };

  return showAuthAlert;
};

export default useAuthAlert;