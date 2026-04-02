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
  const [parentCategoryId, setParentCategoryId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [formErrors, setFormErrors] = useState({});
  const [expandedNodes, setExpandedNodes] = useState(new Set());
  const [hoveredCategoryId, setHoveredCategoryId] = useState(null);

  const fetchCategories = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}products-api/api/categories`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include'
      });

      if (!response.ok) throw new Error('Failed to load categories');
      const data = await response.json();
      setCategories(data.data);
      
      // Сохраняем текущее состояние раскрытых узлов
      // Если expandedNodes пуст, раскрываем корневые категории
      if (expandedNodes.size === 0) {
        const rootIds = data.data.map(cat => cat.id);
        setExpandedNodes(new Set(rootIds));
      }
    } catch (error) {
      showError(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenForm = (category = null, parentId = null) => {
    if (category) {
      setCurrentCategory(category);
      setParentCategoryId(null);
    } else {
      setCurrentCategory(null);
      setParentCategoryId(parentId);
    }
    setIsFormOpen(true);
    setFormErrors({});
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setCurrentCategory(null);
    setParentCategoryId(null);
    setFormErrors({});
  };

  const handleFormSuccess = () => {
    fetchCategories();
  };

  const handleDelete = async (id, hasChildren) => {
    if (hasChildren) {
      const result = await Swal.fire({
        title: 'Cannot delete category',
        text: 'This category has subcategories. Please delete or move them first.',
        icon: 'warning',
        confirmButtonText: 'OK',
        background: '#1e1e2d',
        color: '#fff'
      });
      return;
    }

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
        
        // Удаляем id из expandedNodes, если категория была раскрыта
        setExpandedNodes(prev => {
          const newSet = new Set(prev);
          newSet.delete(id);
          return newSet;
        });
        
        fetchCategories();
        showSuccess('Category deleted successfully');
      } catch (error) {
        showError(error.message);
      }
    }
  };

  const toggleExpand = (categoryId) => {
    setExpandedNodes(prev => {
      const newSet = new Set(prev);
      if (newSet.has(categoryId)) {
        newSet.delete(categoryId);
      } else {
        newSet.add(categoryId);
      }
      return newSet;
    });
  };

  // Функция для автоматического раскрытия родительских категорий при добавлении подкатегории
  const expandParents = (categoryId, categories) => {
    const findParent = (cats, targetId, parents = []) => {
      for (const cat of cats) {
        if (cat.id === targetId) {
          return parents;
        }
        if (cat.children && cat.children.length > 0) {
          const found = findParent(cat.children, targetId, [...parents, cat.id]);
          if (found) return found;
        }
      }
      return null;
    };
    
    return findParent(categories, categoryId) || [];
  };

  // Обновляем expandedNodes после добавления подкатегории
  useEffect(() => {
    if (parentCategoryId && !currentCategory && isFormOpen === false) {
      // После закрытия формы и успешного добавления, раскрываем родительскую категорию
      const parentIds = expandParents(parentCategoryId, categories);
      setExpandedNodes(prev => {
        const newSet = new Set(prev);
        newSet.add(parentCategoryId);
        parentIds.forEach(id => newSet.add(id));
        return newSet;
      });
    }
  }, [isFormOpen, parentCategoryId, currentCategory, categories]);

  const renderCategoryTree = (category, level = 0) => {
    const hasChildren = category.children && category.children.length > 0;
    const isExpanded = expandedNodes.has(category.id);
    const paddingLeft = level === 0 ? 8 : level * 24;
    const isHovered = hoveredCategoryId === category.id;

    return (
      <div key={category.id} className="category-tree-node">
        <div 
          className="category-row" 
          style={{ paddingLeft: `${paddingLeft}px` }}
          onMouseEnter={() => setHoveredCategoryId(category.id)}
          onMouseLeave={() => setHoveredCategoryId(null)}
        >
          <div className="category-info">
            {hasChildren && (
              <button 
                className="expand-btn"
                onClick={() => toggleExpand(category.id)}
              >
                {isExpanded ? (
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <polyline points="6 9 12 15 18 9"></polyline>
                  </svg>
                ) : (
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <polyline points="9 18 15 12 9 6"></polyline>
                  </svg>
                )}
              </button>
            )}
            {!hasChildren && <span className="expand-placeholder"></span>}
            <div className="category-details">
              <span className="category-name">{category.name}</span>
              {category.productCount > 0 && (
                <span className="product-count">({category.productCount} products)</span>
              )}
            </div>
          </div>
          <div className={`actions ${isHovered ? 'actions-visible' : 'actions-hidden'}`}>
            <button 
              className="btn-accent-outline"
              onClick={() => handleOpenForm(null, category.id)}
            >
              + Add Subcategory
            </button>
            <button 
              className="btn-accent-outline yellow"
              onClick={() => handleOpenForm(category, null)}
            >
              Edit
            </button>
            <button 
              className="btn-accent-outline red"
              onClick={() => handleDelete(category.id, hasChildren)}
            >
              Delete
            </button>
          </div>
        </div>
        {hasChildren && isExpanded && (
          <div className="category-children">
            {category.children.map(child => renderCategoryTree(child, level + 1))}
          </div>
        )}
      </div>
    );
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
          onClick={() => handleOpenForm(null, null)}
        >
          + Add Root Category
        </button>
      </header>

      <div className="categories-tree-container">
        {categories.length === 0 ? (
          <div className="empty-state">
            <p>No categories yet. Click "Add Root Category" to get started.</p>
          </div>
        ) : (
          categories.map(category => renderCategoryTree(category))
        )}
      </div>

      <CategoryForm 
        isOpen={isFormOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSuccess}
        category={currentCategory}
        parentCategoryId={parentCategoryId}
      />
    </div>
  );
};

export default CategoriesAdminPage;