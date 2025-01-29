import Swal from 'sweetalert2';

export const handleUnauthorizedError = () => {
  Swal.fire({
    icon: 'error',
    title: 'Unauthorized',
    text: 'Your session has expired. Please log in again.',
    showCancelButton: true,
    confirmButtonText: 'Go to Login',
  }).then((result) => {
    if (result.isConfirmed) {
      window.location.href = '/login';
    }
  });
};