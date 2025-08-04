import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../AuthProvider.js";
import { API_BASE_URL } from '../../apiConfig.js';
import Swal from "sweetalert2";

const Login = () => {
    const [formData, setFormData] = useState({
        email: '',
        password: '',
    });
    const [errors, setErrors] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { fetchUser } = useAuth(); 
    const navigate = useNavigate();
    
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
            const response = await fetch(`${API_BASE_URL}users-api/api/accounts/login`, {
                method: 'POST', 
                headers: { 'Content-Type': 'application/json' }, 
                credentials: 'include',
                body: JSON.stringify({
                    Email: formData.email,
                    Password: formData.password
                }), 
            });
            
            if (response.ok) {
                await fetchUser();
                setErrors({});
                navigate("/"); 
                return;
            }

            const error = await response.json();
            if (response.status === 404 || response.status === 409) {
                showError('Login Error', error.message || 'Invalid email or password');
            } else if (response.status === 400) {
                showError('Login error', error.message);
            } else {
                const validationErrors = {};
                for (const field in error.errors) {
                    if (error.errors[field]?.length > 0) {
                        validationErrors[field.toLowerCase()] = error.errors[field][0];
                    }
                }
                setErrors(validationErrors);
            }
        } catch (error) {
            console.error('Login error:', error);
            showError('Connection Error', 'Unable to connect to the server. Please try again later.');
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
    
    return (
        <div className="d-flex justify-content-center align-items-center min-vh-100">
            <div className="w-100" style={{ maxWidth: '420px', marginTop: '-10vh' }}>
                <div className="text-center mb-4">
                    <h2 className="text-light mb-2" style={{ color: '#4ecca3' }}>Welcome Back</h2>
                    <p className="text-light">Sign in to continue</p>
                </div>
                
                <form onSubmit={handleSubmit} className="p-4 rounded-3 bg-dark shadow" style={{ border: '1px solid #2c2c3a' }}>
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

                    <div className="mb-4">
                      <label htmlFor="password" className="form-label text-light">Password</label>
                      <input
                        type="password"
                        id="password"
                        name="password"
                        className={`form-control ${errors.password ? 'is-invalid bg-dark text-light' : 'bg-dark text-light border-secondary'}`}
                        value={formData.password}
                        onChange={handleChange}
                        placeholder="Enter your password"
                      />
                      {errors.password && <div className="invalid-feedback d-block mt-1">{errors.password}</div>}
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
                                Signing in...
                            </>
                        ) : 'Sign In'}
                    </button>
                    
                    <div className="text-center">
                        <Link 
                            to="/forgot-password" 
                            className="text-decoration-none"
                            style={{ color: '#4ecca3', fontSize: '0.9rem' }}
                        >
                            Forgot password?
                        </Link>
                    </div>
                </form>
        
                <div className="mt-4 text-center">
                    <p className="text-light">
                        Don't have an account?{" "}
                        <Link 
                            to="/register" 
                            className="text-decoration-none fw-bold"
                            style={{ color: '#4ecca3' }}
                        >
                            Sign up
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
};

export default Login;