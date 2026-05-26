import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../../apiConfig';
import { useSignalR } from "../../SignalRProvider";
import Swal from 'sweetalert2';

const Register = () => {
  const { connection, connectionId } = useSignalR();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    nickName: '',
    email: '',
    password: '',
    passwordConfirm: '',
    connectionId: connectionId
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      const response = await fetch(`${API_BASE_URL}users-api/api/accounts/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          NickName: formData.nickName,
          Email: formData.email,
          Password: formData.password,
          PasswordConfirm: formData.passwordConfirm,
          ConnectionId: formData.connectionId
        }),
      });

      if (response.ok) {
        return;
      }

      const error = await response.json();
      if (response.status === 409) {
        showError('Conflict', error.description);
      } else {
        const validationErrors = {};
        for (const field in error.errors) {
          if (error.errors[field]?.length > 0) {
            validationErrors[field.toLowerCase()] = error.errors[field][0];
          }
        }
        setErrors(validationErrors);
        
        if (validationErrors["connectionid"]) {
          showError('Server Error', 'An internal server error occurred. Please try again later.');
        }
      }
    } catch (error) {
      showError('Server Error', 'An internal server error occurred. Please try again later.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const showError = (title, text) => {
    Swal.fire({
      icon: 'error',
      title,
      text,
      background: '#1a1a2e',
      color: '#ffffff',
      confirmButtonColor: '#4ecca3',
      timer: 3000
    });
  };

  useEffect(() => {
    if (connectionId) {
      setFormData(prev => ({ ...prev, connectionId }));
      }
    }, [connectionId]);

    useEffect(() => {
    if (!connection) return;

    // Обработчик успешной регистрации
    connection.on("NotifyUserRegistered", () => {
      setErrors({});
      navigate("/confirm-email", { state: { email: formData.email } });
    });

    // Обработчик ошибки регистрации (без параметров)
    connection.on("NotifyUserRegistrationFailed", () => {
      setIsSubmitting(false);
      Swal.fire({
        icon: 'error',
        title: 'Registration Failed',
        text: 'An error occurred during registration. Please try again later.',
        background: '#1a1a2e',
        color: '#ffffff',
        confirmButtonColor: '#4ecca3',
      });
    });

    // Очистка при размонтировании
    return () => {
      connection.off("NotifyUserRegistered");
      connection.off("notifyuserregistrationfailed");
    };
  }, [connection, formData.email, navigate]);

  return (
    <div className="d-flex justify-content-center align-items-center min-vh-100">
      <div className="w-100" style={{ maxWidth: '420px', marginTop: '-10vh' }}>
        <div className="text-center mb-4">
          <h2 className="text-light mb-2" style={{ color: '#4ecca3' }}>Create Account</h2>
          <p className="text-light">Join us today</p>
        </div>
        
        <form onSubmit={handleSubmit} className="p-4 rounded-3 bg-dark shadow" style={{ border: '1px solid #2c2c3a' }}>
          <div className="mb-3">
            <label htmlFor="nickName" className="form-label text-light">Nickname</label>
            <input
              type="text"
              id="nickName"
              name="nickName"
              className={`form-control ${errors.nickname ? 'is-invalid bg-dark text-light' : 'bg-dark text-light border-secondary'}`}
              value={formData.nickName}
              onChange={handleChange}
              placeholder="Enter your nickname"
            />
            {errors.nickname && <div className="invalid-feedback d-block mt-1">{errors.nickname}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="email" className="form-label text-light">Email Address</label>
            <input
              type="email"
              id="email"
              name="email"
              className={`form-control ${errors.email ? 'is-invalid bg-dark text-light' : 'bg-dark text-light border-secondary'}`}
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter your email"
            />
            {errors.email && <div className="invalid-feedback d-block mt-1">{errors.email}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="password" className="form-label text-light">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              className={`form-control ${errors.password ? 'is-invalid bg-dark text-light' : 'bg-dark text-light border-secondary'}`}
              value={formData.password}
              onChange={handleChange}
              placeholder="Create a password"
            />
            {errors.password && <div className="invalid-feedback d-block mt-1">{errors.password}</div>}
          </div>

          <div className="mb-4">
            <label htmlFor="passwordConfirm" className="form-label text-light">Confirm Password</label>
            <input
              type="password"
              id="passwordConfirm"
              name="passwordConfirm"
              className={`form-control ${errors.passwordconfirm ? 'is-invalid bg-dark text-light' : 'bg-dark text-light border-secondary'}`}
              value={formData.passwordConfirm}
              onChange={handleChange}
              placeholder="Confirm your password"
            />
            {errors.passwordconfirm && <div className="invalid-feedback d-block mt-1">{errors.passwordconfirm}</div>}
          </div>

          <button 
            type="submit" 
            className="btn w-100 py-2 mb-3" 
            disabled={isSubmitting}
            style={{ 
              backgroundColor: '#4ecca3',
              border: 'none',
              fontWeight: 600
            }}
          >
            {isSubmitting ? (
              <>
                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Creating account...
              </>
            ) : 'Sign Up'}
          </button>
        </form>

        <div className="mt-4 text-center">
          <p className="text-light">
            Already have an account?{" "}
            <Link 
              to="/login" 
              className="text-decoration-none fw-bold"
              style={{ color: '#4ecca3' }}
            >
              Sign in
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;