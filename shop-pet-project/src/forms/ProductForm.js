import { useState } from "react";
import { FaSpinner, FaSave } from "react-icons/fa";
import "../Styles/ButtonStyle.css"

const ProductForm = ({
    initialData = null,
    onSubmit,
    isSubmitting,
    formTitle,
    submitButtonText,
    errors: propErrors = {} // Добавляем проп для ошибок
}) => {
    const defaultFormState = {
        name: '',
        description: '',
        price: '',
        stockQuantity: ''
    };

    const [formData, setFormData] = useState(initialData || defaultFormState);
    const [validationErrors, setValidationErrors] = useState({});

    // Синхронизируем ошибки из пропсов
    useState(() => {
        if (propErrors) {
            setValidationErrors(propErrors);
        }
    }, [propErrors]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (validationErrors[name]) {
            setValidationErrors(prev => ({ ...prev, [name]: undefined }));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const productDto = {
            name: formData.name,
            description: formData.description,
            price: formData.price ? parseFloat(formData.price) : null,
            stockQuantity: formData.stockQuantity ? parseInt(formData.stockQuantity) : null
        };

        const result = await onSubmit(productDto);
        if (result?.errors) {
            setValidationErrors(result.errors);
        }
    };

    return (
        <div className="glass-card p-4 p-md-4">  
            <div className="text-center mb-4">  
                <h2 className="text-light mb-1">{formTitle}</h2> 
            </div>

            <form onSubmit={handleSubmit}>
                <div className="row g-2 mb-3"> 
                    <div className="col-md-4">
                        <div className="form-floating">
                            <input
                                name="name"
                                id="name"
                                className={`form-control bg-transparent text-light ${validationErrors['Name'] ? 'is-invalid' : ''}`}
                                value={formData.name}
                                onChange={handleChange}
                                placeholder=" "
                            />
                            <label htmlFor="name" className="text-light">Product Name</label>
                            {validationErrors['Name'] && (
                                <div className="invalid-feedback">
                                    {validationErrors['Name'][0]}
                                </div>
                            )}
                        </div>
                    </div>
    
                    <div className="col-md-4">
                        <div className="form-floating">
                            <input
                                type="number"
                                name="price"
                                id="price"
                                className={`form-control bg-transparent text-light ${validationErrors['Price'] ? 'is-invalid' : ''}`}
                                value={formData.price}
                                onChange={handleChange}
                                placeholder=" "
                                min="0"
                                step="0.01"
                            />
                            <label htmlFor="price" className="text-light">Price</label>
                            {validationErrors['Price'] && (
                                <div className="invalid-feedback">
                                    {validationErrors['Price'][0]}
                                </div>
                            )}
                        </div>
                    </div>
    
                    <div className="col-md-4">
                        <div className="form-floating">
                            <input
                                type="number"
                                name="stockQuantity"
                                id="stockQuantity"
                                className={`form-control bg-transparent text-light ${validationErrors['StockQuantity'] ? 'is-invalid' : ''}`}
                                value={formData.stockQuantity}
                                onChange={handleChange}
                                placeholder=" "
                                min="0"
                            />
                            <label htmlFor="stockQuantity" className="text-light">Stock</label> 
                            {validationErrors['StockQuantity'] && (
                                <div className="invalid-feedback">
                                    {validationErrors['StockQuantity'][0]}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
    
                <div className="mb-3"> 
                    <div className="form-floating">
                        <textarea
                            name="description"
                            id="description"
                            className={`form-control bg-transparent text-light ${validationErrors['Description'] ? 'is-invalid' : ''}`}
                            value={formData.description}
                            onChange={handleChange}
                            placeholder=" "
                            style={{ height: '120px', resize: 'none' }}  
                        />
                        <label htmlFor="description" className="text-light">Description</label>
                        {validationErrors['Description'] && (
                            <div className="invalid-feedback">
                                {validationErrors['Description'][0]}
                            </div>
                        )}
                    </div>
                </div>
                
                <div className="d-flex justify-content-center mt-3">
                    <button 
                        type="submit" 
                        className="btn-accent"  
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? (
                            <>
                                <FaSpinner className="spin" />
                                Processing...
                            </>
                        ) : (
                            <>
                                <FaSave />
                                {submitButtonText}
                            </>
                        )}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default ProductForm;