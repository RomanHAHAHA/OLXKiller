import { useState, useEffect } from 'react';
import { useAuth } from '../../AuthProvider';
import { API_BASE_URL } from '../../apiConfig';
import styles from '../../Styles/ActionLogs.module.css';
import ActionLogsFilter from '../../components/ActionLogsFilter';
import ActionLogRow from '../../components/ActionLogRow';

const ActionLogs = () => {
  const { user } = useAuth();
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 30,
    totalCount: 0,
    totalPages: 1
  });
  const [filters, setFilters] = useState({
    userId: '',
    actionType: '',
    startDate: '',
    endDate: ''
  });
  const [sortParams, setSortParams] = useState({
    orderBy: 'CreatedAt',
    sortDirection: '1' 
  });

  const fetchLogs = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      console.log(filters.startDate)
      console.log(filters.endDate)

      if (filters.userId) params.append('UserId', filters.userId);
      if (filters.actionType) params.append('ActionType', filters.actionType);
      if (filters.startDate) params.append('StartDate', filters.startDate);
      if (filters.endDate) params.append('EndDate', filters.endDate);
      
      params.append('Page', pagination.page);
      params.append('PageSize', pagination.pageSize);
      params.append('OrderBy', sortParams.orderBy);
      params.append('SortDirection', sortParams.sortDirection);

      const response = await fetch(`${API_BASE_URL}logs-api/api/action-logs?${params.toString()}`, { credentials: 'include' });

      if (!response.ok) throw new Error('Failed to fetch logs');
      
      const data = await response.json();
      setLogs(data.items);
      setPagination({
        page: data.page,
        pageSize: data.pageSize,
        totalCount: data.totalCount,
        totalPages: data.totalPages
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSort = (column) => {
    setSortParams(prev => ({
      orderBy: column,
      sortDirection: prev.orderBy === column ? (prev.sortDirection === '1' ? '0' : '1') : '1'
    }));
  };

  const handlePageChange = (newPage) => {
    setPagination(prev => ({ ...prev, page: newPage }));
  };

  const handleFilterChange = (newFilters) => {
    setFilters(newFilters);
    setPagination(prev => ({ ...prev, page: 1 })); // Reset to first page when filters change
  };

  const onDelete = () => {
    fetchLogs();
  }

  useEffect(() => {
    fetchLogs();
  }, [pagination.page, sortParams, filters]);

  if (!user?.permissions?.includes('ManageActionLogs')) {
    return (
      <div className={styles.unauthorized}>
        <h2>Access Denied</h2>
        <p>You don't have permission to view this page.</p>
      </div>
    );
  }

  if (loading) return <div className={styles.loading}>Loading...</div>;
  if (error) return <div className={styles.error}>{error}</div>;

  return (
    <div className={styles.container}>
      <ActionLogsFilter 
        filters={filters} 
        onFilterChange={handleFilterChange}
      />

      <div className={styles.tableContainer}>
        <table className={styles.logsTable}>
          <thead>
            <tr>
              <th onClick={() => handleSort('Id')}>
                ID {sortParams.orderBy === 'Id' && (
                  sortParams.sortDirection === '1' ? '▼' : '▲'
                )}
              </th>
              <th onClick={() => handleSort('UserId')}>
                User {sortParams.orderBy === 'UserId' && (
                  sortParams.sortDirection === '1' ? '▼' : '▲'
                )}
              </th>
              <th onClick={() => handleSort('ActionType')}>
                Action {sortParams.orderBy === 'ActionType' && (
                  sortParams.sortDirection === '1' ? '▼' : '▲'
                )}
              </th>
              <th>Description</th>
              <th onClick={() => handleSort('CreatedAt')}>
                Date {sortParams.orderBy === 'CreatedAt' && (
                  sortParams.sortDirection === '1' ? '▼' : '▲'
                )}
              </th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {logs.map(log => (
              <ActionLogRow 
                key={log.id} 
                log={log} 
                onDelete={() => onDelete(log.id)}
              />
            ))}
          </tbody>
        </table>

        {logs.length === 0 && (
          <div className={styles.noResults}>No logs found matching your criteria</div>
        )}
      </div>

      <div className={styles.pagination}>
        <button
          disabled={pagination.page === 1}
          onClick={() => handlePageChange(pagination.page - 1)}
        >
          Previous
        </button>
        
        <span>
          Page {pagination.page} of {parseInt(pagination.totalCount / pagination.pageSize)}
        </span>
        
        <button
          disabled={pagination.page === pagination.totalPages}
          onClick={() => handlePageChange(pagination.page + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default ActionLogs;