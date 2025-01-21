import { useState } from "react";
import Swal from "sweetalert2";
import { Link, useNavigate } from "react-router-dom";  

const Login = () => {
    const [formData, setFormData] = useState({
        email: '',
        password: '',
    });
    
    const [errors, setErrors] = useState({});
    const navigate = useNavigate();
    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
          ...formData,
          [name]: value,
        });
    };
    
    const handleSubmit = async (e) => {
        e.preventDefault();
    
        try {
          const response = await fetch('https://localhost:7208/api/Accounts/login', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData),
          });
          if (response.ok) {
              setErrors({});
              navigate("/");
          } else {
            const error = await response.json();
            if (response.status === 401 || response.status === 404) {
              Swal.fire({
                icon: 'error',
                title: 'Error ' + response.status,
                text: error.description,
              });
            } else {
              const validationErrors = {};
    
              for (const field in error.errors) {
                if (error.errors[field] && error.errors[field].length > 0) {
                  validationErrors[field] = error.errors[field][0];
                }
              }
    
              setErrors(validationErrors);
            }
          }
        } catch (error) {
          Swal.fire({
            icon: 'error',
            title: 'Server Error',
            text: 'An internal server error occurred. Please try again later.',
          });
        }
    };
    
      return (
        <div className="container mt-5" style={{ maxWidth: '400px' }}>
          <h2 className="text-center mb-4">Login</h2>
          <form onSubmit={handleSubmit} className="p-3 border rounded bg-light">
            <div className="mb-3">
              <label htmlFor="email" className="form-label">Email</label>
              <input
                type="email"
                id="email"
                name="email"
                className={`form-control ${errors['Email'] ? 'is-invalid' : ''}`}
                value={formData.email}
                onChange={handleChange}
              />
              {errors['Email'] && <div className="invalid-feedback">{errors['Email']}</div>}
            </div>
    
            <div className="mb-3">
              <label htmlFor="password" className="form-label">Password</label>
              <input
                type="password"
                id="password"
                name="password"
                className={`form-control ${errors['Password'] ? 'is-invalid' : ''}`}
                value={formData.password}
                onChange={handleChange}
              />
              {errors['Password'] && <div className="invalid-feedback">{errors['Password']}</div>}
            </div>
    
            <button type="submit" className="btn btn-primary w-100">Login</button>
          </form>

          <div className="mt-3 text-center">
            <p>Don't have an account? <Link to="/account/register">Register here</Link></p> 
          </div>
        </div>
      );
};

export default Login;