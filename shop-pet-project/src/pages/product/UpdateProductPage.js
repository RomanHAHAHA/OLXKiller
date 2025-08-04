import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useSignalR } from "../../SignalRProvider";
import ProductForm from "../../forms/ProductForm";
import Swal from "sweetalert2";
import { API_BASE_URL } from "../../apiConfig";
import { CgSpinner } from "react-icons/cg";
import { FaArrowRight } from "react-icons/fa";

const UpdateProductPage = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    const { connection } = useSignalR();
    const [initialData, setInitialData] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [hasChanges, setHasChanges] = useState(false);
    const [validationErrors, setValidationErrors] = useState({});

    useEffect(() => {
        const fetchProduct = async () => {
            try {
                const response = await fetch(`${API_BASE_URL}products-api/api/products/${productId}/base`);
                if (!response.ok) throw new Error('Product not found');
                const product = (await response.json()).data; 
                setInitialData(product);
            } catch (error) {
                Swal.fire('Error', error.message, 'error');
                navigate('/create-product');
            } finally {
                setIsLoading(false);
            }
        };

        fetchProduct();
    }, [productId, navigate]);

    const handleSubmit = async (productDto) => {
        setIsSubmitting(true);
        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/products/${productId}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(productDto)
            });

            if (!response.ok) {
                const data = await response.json();
                if (data.errors) {
                    // Преобразуем ошибки к нужному формату
                    const formattedErrors = {};
                    Object.keys(data.errors).forEach(key => {
                        formattedErrors[key] = data.errors[key];
                    });
                    setValidationErrors(formattedErrors);
                    return { errors: formattedErrors };
                }
                throw new Error(data.title || 'Failed to update product');
            }

            setHasChanges(false);
            setValidationErrors({});
            return await response.json();
        } catch (error) {
            Swal.fire('Error', error.message, 'error');
            return { errors: { general: error.message } };
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleSkip = () => {
        if (hasChanges) {
            Swal.fire({
                title: 'Unsaved Changes',
                text: 'You have unsaved changes. Are you sure you want to skip?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, skip changes',
                cancelButtonText: 'No, stay'
            }).then((result) => {
                if (result.isConfirmed) {
                    navigate(`/products/${productId}/add-categories`);
                }
            });
        } else {
            navigate(`/products/${productId}/add-categories`);
        }
    };

    const handleFormChange = (newData) => {
        const changed = JSON.stringify(newData) !== JSON.stringify(initialData);
        setHasChanges(changed);
        // Очищаем ошибки при изменении данных
        if (Object.keys(validationErrors).length > 0) {
            setValidationErrors({});
        }
    };

    useEffect(() => {
        if (!connection) return;

        connection.on("NotifyProductUpdated", (updatedProductId) => {
            navigate(`/products/${updatedProductId}/add-categories`);
        });

        connection.on("NotifyProductUpdateFailed", (error) => {
            Swal.fire({
                title: 'Update Failed',
                text: error,
                icon: 'error',
                confirmButtonColor: '#3085d6',
            });
        });

        return () => {
            connection.off("NotifyProductUpdated");
            connection.off("NotifyProductUpdateFailed");
        };
    }, [connection, navigate]);

    if (isLoading) return <div className="text-center py-5"><CgSpinner className="animate-spin" size={24} /></div>;

    return (
        <div className="container py-4" style={{ maxWidth: '800px' }}>
            <div className="text-center mb-3">
                <h2 className="mb-0">Update Product</h2>
            </div>

            <ProductForm 
                initialData={initialData}
                onSubmit={handleSubmit}
                onChange={handleFormChange}
                isSubmitting={isSubmitting}
                submitButtonText="Update Product"
                errors={validationErrors} // Передаем ошибки в форму
            />
            
            <div className="d-flex justify-content-end mt-2">
                <button 
                    onClick={handleSkip}
                    className="btn-accent-outline"
                >
                    To Categories <FaArrowRight className="ms-2" />
                </button>
            </div>
        </div>
    );
};

export default UpdateProductPage;