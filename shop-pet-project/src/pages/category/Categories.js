import { useState, useEffect } from 'react';
import Swal from 'sweetalert2';
import { API_BASE_URL } from '../../apiConfig';
import { useAuth } from '../../AuthProvider';
import CategoryForm from '../../forms/CategoryForm.js';
import '../../Styles/CategoriesAdmin.css';

const CategoriesAdminPage = () => {
  const { user } = useAuth();
  const [categories, setCategories] = useState([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [currentCategory, setCurrentCategory] = useState(null);
  const [loading, setLoading] = useState(true);
  const [formErrors, setFormErrors] = useState({});

  const fetchCategories = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}products-api/api/categories/db`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include'
      });

      if (!response.ok) throw new Error('Failed to load categories');
      const data = await response.json();
      setCategories(data.data);
    } catch (error) {
      showError(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenForm = (category = null) => {
    setCurrentCategory(category || { name: '', description: '' });
    setIsFormOpen(true);
    setFormErrors({}); // Clear errors when opening form
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setCurrentCategory(null);
    setFormErrors({});
  };

  const handleFormSuccess = () => {
    fetchCategories(); 
  };
  const handleDelete = async (id) => {
    const result = await Swal.fire({
      title: 'Delete category?',
      text: 'This action cannot be undone',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Yes, delete it',
      background: '#1e1e2d',
      color: '#fff'
    });

    if (result.isConfirmed) {
      try {
        const response = await fetch(`${API_BASE_URL}products-api/api/categories/${id}`, {
          method: 'DELETE',
          credentials: 'include'
        });
        if (!response.ok) throw new Error('Deletion failed');
        fetchCategories();
        showSuccess('Category deleted successfully');
      } catch (error) {
        showError(error.message);
      }
    }
  };

  const showError = (message) => {
    Swal.fire({
      icon: 'error',
      title: 'Error',
      text: message,
      background: '#1e1e2d',
      color: '#fff'
    });
  };

  const showSuccess = (message) => {
    Swal.fire({
      icon: 'success',
      title: 'Success',
      text: message,
      background: '#1e1e2d',
      color: '#fff',
      timer: 2000
    });
  };

  useEffect(() => {
    if (user) fetchCategories();
  }, [user]);

  if (loading) return <div className="loading">Loading categories...</div>;

  return (
    <div className="categories-admin">
      <header className="categories-header">
        <h2>Categories</h2>
        <button 
          className="btn-accent"
          onClick={() => handleOpenForm()}
        >
          + Add Category
        </button>
      </header>

      <div className="categories-table-container">
        <table className="categories-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories.map(category => (
              <tr key={category.id}>
                <td>{category.name}</td>
                <td>{category.description || '-'}</td>
                <td className="actions">
                  <button 
                    className="btn-accent-outline yellow"
                    onClick={() => handleOpenForm(category)}
                  >
                    Edit
                  </button>
                  <button 
                    className="btn-accent-outline red"
                    onClick={() => handleDelete(category.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <CategoryForm 
        isOpen={isFormOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSuccess}
        category={currentCategory}
      />
    </div>
  );
};

export default CategoriesAdminPage;