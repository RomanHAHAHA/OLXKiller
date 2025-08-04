import { useState } from 'react';
import { X } from 'lucide-react';
import styles from '../Styles/ActionLogs.module.css';

const ActionLogsFilter = ({ filters, onFilterChange }) => {
  const [localFilters, setLocalFilters] = useState({
    ...filters,
    startDate: formatDateForDisplay(filters.startDate),
    endDate: formatDateForDisplay(filters.endDate)
  });

  // Функция для преобразования даты из ISO в формат dd.mm.yyyy для отображения
  function formatDateForDisplay(dateString) {
    if (!dateString) return '';
    
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '';
      
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      
      return `${day}.${month}.${year}`;
    } catch (e) {
      console.error('Error formatting date:', e);
      return '';
    }
  }

  // Функция для проверки, является ли строка полной датой в формате dd.mm.yyyy
  function isCompleteDate(input) {
    return /^\d{2}\.\d{2}\.\d{4}$/.test(input);
  }

  // Функция для парсинга даты из формата dd.mm.yyyy
  function parseDateInput(input) {
    if (!isCompleteDate(input)) return null;
    
    const [day, month, year] = input.split('.');
    const dayNum = parseInt(day, 10);
    const monthNum = parseInt(month, 10);
    const yearNum = parseInt(year, 10);
    
    if (isNaN(dayNum)) return null;
    if (isNaN(monthNum)) return null;
    if (isNaN(yearNum)) return null;
    
    // Создаем дату (месяцы в JS начинаются с 0)
    const date = new Date(yearNum, monthNum - 1, dayNum);
    
    // Проверяем, что дата валидна
    if (
      date.getFullYear() === yearNum &&
      date.getMonth() === monthNum - 1 &&
      date.getDate() === dayNum
    ) {
      return date.toISOString();
    }
    
    return null;
  }

  const handleDateChange = (e) => {
  const { name, value } = e.target;

  // 1. Всегда обновляем локальное состояние (то, что видит пользователь)
  setLocalFilters(prev => ({
    ...prev,
    [name]: value
  }));

  // 2. Если дата пустая — сбрасываем фильтр
  if (value === "") {
    onFilterChange({
      ...filters,
      [name]: ""
    });
    return;
  }

  // 3. Если дата неполная (но не пустая) — НЕ ОБНОВЛЯЕМ фильтры
  if (!isCompleteDate(value)) {
    return;
  }

  // 4. Если дата полная — парсим и обновляем
  const parsedDate = parseDateInput(value);
  if (parsedDate) {
    onFilterChange({
      ...filters,
      [name]: parsedDate
    });
  } else {
    // Если дата полная, но некорректная (например, 32.13.2024) — сбрасываем
    onFilterChange({
      ...filters,
      [name]: ""
    });
  }
};

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    const updatedFilters = {
      ...localFilters,
      [name]: value
    };
    
    setLocalFilters(updatedFilters);
    
    const apiFilters = {
      ...updatedFilters,
      // Преобразуем только если дата полная и валидная
      startDate: isCompleteDate(updatedFilters.startDate) ? parseDateInput(updatedFilters.startDate) || '' : '',
      endDate: isCompleteDate(updatedFilters.endDate) ? parseDateInput(updatedFilters.endDate) || '' : ''
    };
    
    onFilterChange(apiFilters);
  };

  const resetFilters = () => {
    const newFilters = {
      userId: '',
      actionType: '',
      startDate: '',
      endDate: ''
    };
    setLocalFilters(newFilters);
    onFilterChange(newFilters);
  };

  return (
    <div className={styles.filterPanel}>
      <div className={styles.filterRow}>
        <div className={styles.filterGroup}>
          <label>User ID</label>
          <input
            type="text"
            name="userId"
            value={localFilters.userId || ''}
            onChange={handleInputChange}
            placeholder="Filter by user ID"
          />
        </div>
        
        <div className={styles.filterGroup}>
          <label>Action Type</label>
          <select
            name="actionType"
            value={localFilters.actionType || ''}
            onChange={handleInputChange}
          >
            <option value="">All Types</option>
            <option value="0">Create</option>
            <option value="1">Read</option>
            <option value="2">Update</option>
            <option value="3">Delete</option>
            <option value="4">Incorrect Password</option>
          </select>
        </div>
      </div>

      <div className={styles.filterRow}>
        <div className={styles.filterGroup}>
          <label>Start Date</label>
          <input
            type="text"
            name="startDate"
            value={localFilters.startDate || ''}
            onChange={handleDateChange}
            placeholder="dd.mm.yyyy"
          />
        </div>
        
        <div className={styles.filterGroup}>
          <label>End Date</label>
          <input
            type="text"
            name="endDate"
            value={localFilters.endDate || ''}
            onChange={handleDateChange}
            placeholder="dd.mm.yyyy"
          />
        </div>
      </div>

      <div className={styles.filterActions}>
        <button className='btn-accent-outline red' onClick={resetFilters}>
          <X size={16} /> Reset
        </button>
      </div>
    </div>
  );
};

export default ActionLogsFilter;