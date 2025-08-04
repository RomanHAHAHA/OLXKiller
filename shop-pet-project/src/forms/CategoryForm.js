import { useEffect, useState } from 'react';
import Swal from 'sweetalert2';
import { API_BASE_URL } from '../apiConfig';

const CategoryForm = ({ isOpen, onClose, onSubmit, category }) => {
  const [formData, setFormData] = useState({ name: '', description: '' });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (category) {
      setFormData({
        name: category.name || '',
        description: category.description || '',
        id: category.id
      });
    } else {
      setFormData({ name: '', description: '' });
    }
    setErrors({});
  }, [category, isOpen]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    try {
      const method = formData.id ? 'PATCH' : 'POST';
      const url = formData.id 
        ? `${API_BASE_URL}products-api/api/categories/${formData.id}`
        : `${API_BASE_URL}products-api/api/categories`;

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData),
        credentials: 'include'
      });
      
      const responseData = await response.json();
      
      if (!response.ok) {
        if (response.status === 400) {
          const validationErrors = {};
          for (const field in responseData.errors) {
            if (responseData.errors[field]?.length > 0) {
              validationErrors[field.toLowerCase()] = responseData.errors[field][0];
            }
          }
          setErrors(validationErrors);
          return;
        }
        throw new Error(responseData.message || 'An error occurred');
      }

      onSubmit(responseData);
      onClose();
      
      Swal.fire({
        icon: 'success',
        title: formData.id ? 'Category updated' : 'Category created',
        background: '#1e1e2d',
        color: '#fff',
        timer: 2000
      });
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: error.message,
        background: '#1e1e2d',
        color: '#fff'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  if (!isOpen) return null;

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      backgroundColor: 'rgba(0,0,0,0.5)',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 1000
    }}>
      <div style={{
        background: '#2a2a3c',
        padding: '20px',
        borderRadius: '8px',
        width: '90%',
        maxWidth: '500px'
      }}>
        <h3 style={{ color: 'white', marginTop: 0 }}>
          {formData.id ? 'Edit Category' : 'Add New Category'}
        </h3>
        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: '15px' }}>
            <label style={{ display: 'block', color: '#b8b8d2', marginBottom: '5px' }}>Name</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              style={{
                width: '100%',
                padding: '8px',
                background: '#1e1e2d',
                border: errors.name ? '1px solid #dc3545' : '1px solid #444',
                borderRadius: '4px',
                color: 'white'
              }}
            />
            {errors.name && (
              <div style={{ 
                color: '#dc3545', 
                fontSize: '0.875rem',
                marginTop: '5px'
              }}>
                {errors.name}
              </div>
            )}
          </div>
          <div style={{ marginBottom: '15px' }}>
            <label style={{ display: 'block', color: '#b8b8d2', marginBottom: '5px' }}>Description</label>
            <textarea
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows="4"
              style={{
                width: '100%',
                padding: '8px',
                background: '#1e1e2d',
                border: errors.description ? '1px solid #dc3545' : '1px solid #444',
                borderRadius: '4px',
                color: 'white',
                minHeight: '100px'
              }}
            />
            {errors.description && (
              <div style={{ 
                color: '#dc3545', 
                fontSize: '0.875rem',
                marginTop: '5px'
              }}>
                {errors.description}
              </div>
            )}
          </div>
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
            <button 
              type="button" 
              onClick={onClose}
              disabled={isSubmitting}
              className='btn-accent red'
            >
              Cancel
            </button>
            <button 
              type="submit"
              disabled={isSubmitting}
              className='btn-accent-outline'
            >
              {isSubmitting ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Processing...
                </>
              ) : (formData.id ? 'Save' : 'Create')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CategoryForm;