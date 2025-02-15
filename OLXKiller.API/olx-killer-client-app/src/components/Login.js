import { useState } from "react";
import Swal from "sweetalert2";
import { Link, useNavigate } from "react-router-dom";  
import { useAuth } from "../utils/AuthContext";

const Login = () => {
    const [formData, setFormData] = useState({
        email: '',
        password: '',
    });
    const [errors, setErrors] = useState({});
    const navigate = useNavigate();
    const { login } = useAuth(); 
    
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
            headers: { 'Content-Type': 'application/json' },
            credentials: "include",
            body: JSON.stringify(formData),
          });
          if (response.ok) {
              const userResponse = await fetch("https://localhost:7208/api/Users/get-view-data", {
                method: "GET",
                credentials: "include",
              });
              
              if (userResponse.ok) {
                  const userData = await userResponse.json();
                  
                  login({
                      nickName: userData.data.nickName,
                      avatar64String: userData.data.avatar64String,
                  });

                  setErrors({});
                  navigate("/"); 
              } else {
                  Swal.fire({
                      icon: 'error',
                      title: 'User Data Error',
                      text: 'Failed to retrieve user data.',
                  });
              }
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
        <h2 className="text-center mb-4 text-light">Login</h2>
        <form onSubmit={handleSubmit} className="p-4 border rounded-3 bg-dark shadow-sm">
          <div className="mb-3">
            <label htmlFor="email" className="form-label text-light">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              className={`form-control ${errors['Email'] ? 'is-invalid' : 'bg-secondary text-light border-0'}`}
              value={formData.email}
              onChange={handleChange}
              style={{ backgroundColor: '#343a40' }}
            />
            {errors['Email'] && <div className="invalid-feedback">{errors['Email']}</div>}
          </div>
    
          <div className="mb-3">
            <label htmlFor="password" className="form-label text-light">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              className={`form-control ${errors['Password'] ? 'is-invalid' : 'bg-secondary text-light border-0'}`}
              value={formData.password}
              onChange={handleChange}
              style={{ backgroundColor: '#343a40' }}
            />
            {errors['Password'] && <div className="invalid-feedback">{errors['Password']}</div>}
          </div>
    
          <button type="submit" className="btn btn-primary w-100">Login</button>
        </form>
    
        <div className="mt-3 text-center">
          <p className="text-light">
            Don't have an account?{" "}
            <Link to="/register" className="text-primary">
              Register here
            </Link>
          </p>
        </div>
      </div>
    );
};

export default Login;
