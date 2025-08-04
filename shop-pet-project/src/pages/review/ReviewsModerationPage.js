import { useEffect, useState } from "react";
import { API_BASE_URL } from "../../apiConfig";
import ReviewCard from "./ReviewCard";
import ReviewFilterPanel from "../../components/ReviewFilterPanel";

const reviewsUrl = `${API_BASE_URL}reviews-api/api/reviews`;

const ReviewsModerationPage = () => {
    const [reviews, setReviews] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({
        productName: undefined,
        userNickName: undefined,
        rate: undefined,
        dateFrom: undefined,
        dateTo: undefined
    });
    const [sorting, setSorting] = useState({
        orderBy: 'createdAt',
        sortDirection: 1 
    });

    useEffect(() => {
        fetchReviews();
    }, [filters, sorting]);

    const handleStatusChange = async () => {
        fetchReviews();
    };

    const fetchReviews = async () => {
        try {
            setLoading(true);
            setError(null);
            
            const params = new URLSearchParams();
            
            Object.entries(filters).forEach(([key, value]) => {
                if (value !== undefined && value !== '') {
                    params.append(key, value.toString());
                }
            });
            
            params.append('orderBy', sorting.orderBy);
            params.append('sortDirection', sorting.sortDirection.toString());

            const response = await fetch(`${reviewsUrl}?${params.toString()}`, { 
                credentials: 'include' 
            });

            if (!response.ok) {
                throw new Error('Failed to fetch reviews');
            }

            const data = await response.json();
            setReviews(data.data);
        } catch (error) {
            setError(error.message);
            console.error('Failed to fetch reviews:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-screen bg-gray-900 text-gray-200">
                <div className="text-xl">Loading reviews...</div>
            </div>
        );
    }

    return (
        <div className="container mt-4">
            <div className="row">
                <div className="col-md-3">
                    <ReviewFilterPanel 
                        filters={filters}
                        onFilterChange={setFilters}
                        sorting={sorting}
                        onSortChange={setSorting}
                    />
                </div>
                
                <div className="col-md-9">                    
                    {error && (
                        <div className="p-4 mb-6 text-red-300 bg-red-900/50 rounded-lg border border-red-700">
                            {error}
                        </div>
                    )}
                    
                    {reviews.length > 0 ? (
                        <div className="row">
                            {reviews.map(review => (
                                <div className="col-md-6 mb-4" key={review.id}>
                                    <ReviewCard
                                        review={review}
                                        onStatusChange={handleStatusChange}
                                    />
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="flex items-center justify-center h-64 text-gray-400 bg-gray-800/50 rounded-lg">
                            <div className="text-center">
                                <div className="text-lg mb-2">No reviews found</div>
                                <div className="text-sm">Try adjusting your filters</div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ReviewsModerationPage;