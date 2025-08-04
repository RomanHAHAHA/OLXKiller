import Swal from "sweetalert2";
import { API_BASE_URL } from "../../apiConfig";
import { useEffect, useState } from "react";
import { useSignalR } from "../../SignalRProvider";
import "../../Styles/CreateProductForm.css";
import { useNavigate } from "react-router-dom";
import ProductForm from "../../forms/ProductForm";
import useAuthAlert from "../../useAuthAlert"

const CreateProductPage = () => {
    const navigate = useNavigate();
    const { connection } = useSignalR();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const showAuthAlert  = useAuthAlert();

    const handleSubmit = async (productDto) => {
        setIsSubmitting(true);
        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/products`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(productDto)
            });

            if (!response.ok) {
                if (response.status === 401) {
                    showAuthAlert({ text: "Please, login to create your product"});
                    return;
                } else {
                    const data = await response.json();
                    if (data.errors) {
                        const formattedErrors = {};
                        Object.keys(data.errors).forEach(key => {
                            formattedErrors[key] = data.errors[key];
                        });
                        return { errors: formattedErrors };
                    }
                }
            }

            return await response.json();
        } catch (error) {
            Swal.fire('Error', error.message, 'error');
            return { errors: { general: error.message } };
        } finally {
            setIsSubmitting(false);
        }
    };

    useEffect(() => {
        if (!connection) return;

        connection.on("NotifyProductCreated", (productId) => {
            navigate(`/products/${productId}/add-categories`);
        });

        connection.on("NotifyProductCreationFailed", (error) => {
            Swal.fire({
                title: 'Creation Failed',
                text: error,
                icon: 'error',
                confirmButtonColor: '#3085d6',
            });
        });

        return () => {
            connection.off("NotifyProductCreated");
            connection.off("NotifyProductCreationFailed");
        };
    }, [connection, navigate]);

    return (
        <div className="container py-4" style={{ maxWidth: '800px' }}>
            <ProductForm 
                onSubmit={handleSubmit}
                isSubmitting={isSubmitting}
                formTitle="Create New Product"
                submitButtonText="Create Product"
            />
        </div>
    );
};

export default CreateProductPage;