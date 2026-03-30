import Swal from "sweetalert2";
import { API_BASE_URL } from "../../apiConfig";
import { useState, useEffect } from "react";
import { FaPlusCircle, FaMinusCircle, FaSpinner, FaArrowLeft } from "react-icons/fa";
import { useParams, useNavigate } from "react-router-dom";
import { CloudRainWind } from "lucide-react";

const AddCharacteristicsPage = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    const [formData, setFormData] = useState({
        characteristics: [{ name: "", value: "" }]
    });
    const [validationErrors, setValidationErrors] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchCharacteristics = async () => {
            try {
                const response = await fetch(`${API_BASE_URL}products-api/api/products/${productId}/characteristics`);
                if (response.ok) {
                    const responseData = await response.json();                    
                    // Достаем массив из поля data
                    const characteristicsArray = responseData.data || [];
                    
                    if (characteristicsArray.length > 0) {
                        setFormData({
                            characteristics: characteristicsArray.map(c => ({ 
                                name: c.name || "", 
                                value: c.value || "" 
                            }))
                        });
                    } else {
                        // Если характеристик нет, оставляем одно пустое поле
                        setFormData({
                            characteristics: [{ name: "", value: "" }]
                        });
                    }
                }
            } catch (error) {
                console.error("Failed to fetch characteristics:", error);
            } finally {
                setIsLoading(false);
            }
        };

        fetchCharacteristics();
    }, [productId]);

    const handleChange = (index, field, value) => {
        setFormData(prev => {
            const newCharacteristics = [...prev.characteristics];
            newCharacteristics[index][field] = value;
            return { ...prev, characteristics: newCharacteristics };
        });
    };

    const addCharacteristic = () => {
        setFormData(prev => ({
            ...prev,
            characteristics: [...prev.characteristics, { name: "", value: "" }]
        }));
    };

    const removeCharacteristic = (index) => {
        if (formData.characteristics.length <= 1) return;
        
        setFormData(prev => {
            const newCharacteristics = [...prev.characteristics];
            newCharacteristics.splice(index, 1);
            return { ...prev, characteristics: newCharacteristics };
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsSubmitting(true);

        const hasCharacteristics = formData.characteristics.some(
            char => char.name.trim() && char.value.trim()
        );
        
        if (!hasCharacteristics) {
            navigate(`/products/${productId}`);
            return;
        }

        const errors = {};
        formData.characteristics.forEach((char, index) => {
            if (!char.name.trim()) errors[`name_${index}`] = "Name is required";
            if (!char.value.trim()) errors[`value_${index}`] = "Value is required";
        });

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            setIsSubmitting(false);
            return;
        }

        try {
            const characteristicsDto = formData.characteristics.map(char => ({
                name: char.name,
                value: char.value
            }));

            const response = await fetch(`${API_BASE_URL}products-api/api/products/${productId}/characteristics`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(characteristicsDto)
            });

            const responseData = await response.json();

            if (!response.ok) {
                throw new Error(responseData.message || 'Failed to save characteristics');
            }

            navigate(`/products/${productId}`);

        } catch (error) {
            Swal.fire({
                title: 'Error',
                text: error.message,
                icon: 'error',
                confirmButtonColor: '#3085d6',
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleBack = () => navigate(`/products/${productId}/add-images`);
    const handleSkip = () => {
        Swal.fire({
            title: 'Skip characteristics?',
            text: 'Are you sure you want to continue without adding characteristics?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, skip',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                navigate(`/products/${productId}`);
            }
        });
    };

    if (isLoading) {
        return (
            <div className="container py-5 text-center">
                <FaSpinner className="spin" size={24} />
                <p>Loading characteristics...</p>
            </div>
        );
    }

    return (
        <div className="container py-4" style={{ maxWidth: '800px' }}>
            <div className="glass-card p-4 p-md-4">
                <div className="text-center mb-4">
                    <h2 className="text-light mb-1" style={{ color: "#4ecca3" }}>Product Characteristics</h2>
                    <p className="text-light">Specify key features and their values</p>
                </div>

                <div className="characteristics-container mb-4">
                    {formData.characteristics.length === 0 ? (
                        <div className="text-center text-light py-4">
                            <p>No characteristics added yet</p>
                        </div>
                    ) : (
                        formData.characteristics.map((char, index) => (
                            <div key={index} className="characteristic-item mb-3 p-3 bg-secondary bg-opacity-10 rounded">
                                <div className="row g-2 align-items-center">
                                    <div className="col-md-5">
                                        <div className="form-floating">
                                            <input
                                                type="text"
                                                className={`form-control bg-transparent text-light ${validationErrors[`name_${index}`] ? 'is-invalid' : ''}`}
                                                value={char.name}
                                                onChange={(e) => handleChange(index, 'name', e.target.value)}
                                                placeholder=" "
                                                style={{ color: '#fff' }}
                                            />
                                            <label className="text-light">Characteristic Name</label>
                                            {validationErrors[`name_${index}`] && (
                                                <div className="invalid-feedback d-block">
                                                    {validationErrors[`name_${index}`]}
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    <div className="col-md-5">
                                        <div className="form-floating">
                                            <input
                                                type="text"
                                                className={`form-control bg-transparent text-light ${validationErrors[`value_${index}`] ? 'is-invalid' : ''}`}
                                                value={char.value}
                                                onChange={(e) => handleChange(index, 'value', e.target.value)}
                                                placeholder=" "
                                                style={{ color: '#fff' }}
                                            />
                                            <label className="text-light">Value</label>
                                            {validationErrors[`value_${index}`] && (
                                                <div className="invalid-feedback d-block">
                                                    {validationErrors[`value_${index}`]}
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    <div className="col-md-2 d-flex justify-content-center">
                                        <button
                                            type="button"
                                            className="btn btn-outline-danger p-2 d-flex align-items-center justify-content-center"
                                            style={{ width: '38px', height: '38px' }}
                                            onClick={() => removeCharacteristic(index)}
                                            disabled={formData.characteristics.length <= 1}
                                            title="Remove characteristic"
                                        >
                                            <FaMinusCircle className="m-0" />
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))
                    )}
                </div>

                <div className="mb-4 text-center">
                    <button
                        type="button"
                        className="btn-accent px-4 py-2"
                        onClick={addCharacteristic}
                    >
                        <FaPlusCircle className="icon me-2" />
                        Add Characteristic
                    </button>
                </div>
            </div>
            <div className="d-flex justify-content-between mt-4 pt-3">
                <button
                    type="button"
                    className="btn-accent-outline px-4 py-2"
                    onClick={handleBack}
                >
                    <FaArrowLeft className="me-2" /> Back
                </button>
                <button
                    type="button"
                    className="btn-accent-outline px-4 py-2"
                    onClick={handleSkip}
                >
                    Skip
                </button>
                <button
                    type="button"
                    className="btn-accent px-4 py-2"
                    onClick={handleSubmit}
                    disabled={isSubmitting}
                >
                    {isSubmitting ? (
                        <>
                            <FaSpinner className="spin me-2" />
                            Saving...
                        </>
                    ) : (
                        "Save & Continue"
                    )}
                </button>
            </div>
        </div>
    );
};

export default AddCharacteristicsPage;