import { useEffect, useState } from 'react';
import Swal from 'sweetalert2';
import { API_BASE_URL } from '../apiConfig';
import '../Styles/CategoryFormStyle.css'; 

const CategoryForm = ({ isOpen, onClose, onSubmit, category, parentCategoryId }) => {
  const [formData, setFormData] = useState({ name: '' });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  useEffect(() => {
    if (isOpen) {
      console.log('Form is open, category:', category, 'parentId:', parentCategoryId);
      if (category && category.id) {
        setFormData({
          name: category.name || '',
          id: category.id
        });
        setIsEditMode(true);
      } else {
        setFormData({ name: '' });
        setIsEditMode(false);
      }
      setErrors({});
    }
  }, [category, parentCategoryId, isOpen]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      setErrors({ name: 'Category name is required' });
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      let url, method, body;

      if (isEditMode) {
        url = `${API_BASE_URL}products-api/api/categories/${formData.id}`;
        method = 'PATCH';
        body = JSON.stringify({
          id: formData.id,
          name: formData.name.trim()
        });
      } else {
        url = `${API_BASE_URL}products-api/api/categories`;
        method = 'POST';
        body = JSON.stringify({
          parentCategoryId: parentCategoryId || null,
          name: formData.name.trim()
        });
      }

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body,
        credentials: 'include'
      });
      
      let responseData;
      try {
        responseData = await response.json();
      } catch (e) {
        responseData = {};
      }
      
      if (!response.ok) {
        if (response.status === 400) {
          const validationErrors = {};
          if (responseData.errors) {
            for (const field in responseData.errors) {
              if (responseData.errors[field]?.length > 0) {
                validationErrors[field.toLowerCase()] = responseData.errors[field][0];
              }
            }
          } else if (responseData.message) {
            throw new Error(responseData.message);
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
        title: isEditMode ? 'Category updated' : 'Category created',
        background: '#1e1e2d',
        color: '#fff',
        timer: 2000,
        showConfirmButton: false
      });
    } catch (error) {
      // Закрываем форму перед показом ошибки
      onClose();
      
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

  const getModalTitle = () => {
    if (isEditMode) return 'Edit Category';
    if (parentCategoryId) return 'Add Subcategory';
    return 'Add Root Category';
  };

  if (!isOpen) return null;

  return (
    <div className="category-form-overlay">
      <div className="category-form-modal">
        <h3>{getModalTitle()}</h3>
        
        {parentCategoryId && !isEditMode && (
          <div className="category-form-info-message">
            <p>Creating subcategory under parent category</p>
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          <div className="category-form-group">
            <label>Name *</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              placeholder="Enter category name"
              className={errors.name ? 'error' : ''}
              autoFocus
            />
            {errors.name && (
              <div className="category-form-error">
                {errors.name}
              </div>
            )}
          </div>
          
          <div className="category-form-actions">
            <button 
              type="button" 
              onClick={onClose}
              disabled={isSubmitting}
              className="category-form-cancel"
            >
              Cancel
            </button>
            <button 
              type="submit"
              disabled={isSubmitting}
              className="category-form-submit"
            >
              {isSubmitting ? 'Processing...' : (isEditMode ? 'Save Changes' : 'Create')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CategoryForm;