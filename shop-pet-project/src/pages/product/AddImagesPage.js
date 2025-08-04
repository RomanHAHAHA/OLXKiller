import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import { API_BASE_URL } from '../../apiConfig';
import imagePlaceholder from '../../asserts/imagePlaceholder.jpg';
import { FaTrash, FaStar, FaUpload, FaImages, FaCloudUploadAlt, FaCrown, FaArrowRight, FaArrowLeft } from 'react-icons/fa';
import "../../Styles/UploadImagesStyles.css";

const AddImagesPage = () => {
    const { productId } = useParams();
    const navigate = useNavigate();
    const [images, setImages] = useState([]);
    const [selectedFiles, setSelectedFiles] = useState([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        fetchImages();
    }, [productId]);

    const fetchImages = async () => {
        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}products-api/api/products/${productId}/images`);
            if (response.ok) {
                const images = (await response.json()).data;
                setImages(images || []);
            } else {
                throw new Error('Failed to load images');
            }
        } catch (error) {
            Swal.fire('Error', error.message, 'error');
        } finally {
            setIsLoading(false);
        }
    };

    const handleFileChange = (e) => {
        setSelectedFiles(Array.from(e.target.files));
    };

    const handleUpload = async () => {
        if (selectedFiles.length === 0) {
            Swal.fire('Warning', 'Please select at least one file to upload', 'warning');
            return;
        }

        const formData = new FormData();
        selectedFiles.forEach(file => formData.append('images', file));

        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/product-images/${productId}`, {
                method: 'POST',
                body: formData,
                credentials: 'include',
            });

            if (response.ok) {
                await fetchImages();
                setSelectedFiles([]);
            } else {
                throw new Error('Failed to upload images');
            }
        } catch (error) {
            Swal.fire('Error', error.message, 'error');
        }
    };

    const handleDelete = async (imageId) => {
        const result = await Swal.fire({
            title: 'Are you sure?',
            text: 'This action cannot be undone!',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
        });

        if (result.isConfirmed) {
            try {
                const response = await fetch(`${API_BASE_URL}products-api/api/product-images/${imageId}`, {
                    method: 'DELETE',
                    credentials: 'include',
                });
                if (response.ok) {
                    await fetchImages();
                } else {
                    throw new Error('Failed to delete image');
                }
            } catch (error) {
                Swal.fire('Error', error.message, 'error');
            }
        }
    };

    const handleSetMain = async (imageId) => {
        try {
            const response = await fetch(`${API_BASE_URL}products-api/api/product-images/${imageId}`, {
                method: 'PATCH',
                credentials: 'include',
            });
            if (response.ok) {
                await fetchImages();
            } else {
                throw new Error('Failed to set main image');
            }
        } catch (error) {
            Swal.fire('Error', error.message, 'error');
        }
    };

    const handleBack = () => navigate(`/products/${productId}/add-categories`);
    const handleNext = () => navigate(`/products/${productId}/add-characteristics`);

    return (
        <div className="container mt-1" style={{ maxWidth: '800px' }}>
            <div className="text-center mb-4">
                <h2 className="text-light mb-0" style={{ color: "#4ecca3" }}>
                    Product Images
                </h2>
            </div>
            <div className="glass-card p-4 mb-4">    
                <div className="mb-3">
                    <div 
                        className="dropzone-area p-4 text-center"
                        onClick={() => document.querySelector('.file-input').click()}
                    >
                        <input
                            type="file"
                            multiple
                            className="file-input d-none"
                            onChange={handleFileChange}
                            accept="image/*"
                        />
                        <FaCloudUploadAlt className="text-light mb-2" size={36} />
                        <h5 className="text-light mb-2">Drag & drop images here</h5>
                        <p className="text-muted mb-2">or click to browse files</p>
                        {selectedFiles.length > 0 && (
                            <span className="badge bg-primary mt-2">
                                {selectedFiles.length} {selectedFiles.length === 1 ? 'file' : 'files'} selected
                            </span>
                        )}
                    </div>
                    <button
                        className="btn-accent mt-3 px-4 py-2 d-flex align-items-center mx-auto"
                        onClick={handleUpload}
                    >
                        <FaUpload /> Upload Images
                    </button>
                </div>

                {isLoading ? (
                    <div className="text-center py-5">
                        <div className="spinner-border text-primary" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                    </div>
                ) : images.length > 0 ? (
                    <div className="gallery-container">
                        <div className="gallery-scroll">
                            {images.map((image) => (
                                <div className="gallery-item" key={image.id}>
                                    <div className="gallery-image-container">
                                        <img
                                            src={`${API_BASE_URL}product-images/${image.imageName}`}
                                            onError={(e) => { e.target.src = imagePlaceholder }}
                                            alt="Product"
                                            className="gallery-image"
                                        />
                                        {image.isMain && (
                                            <div className="main-badge">
                                                <FaCrown size={12} className="me-1" /> Main
                                            </div>
                                        )}
                                    </div>
                                    <div className="gallery-actions">
                                        {!image.isMain && (
                                            <button
                                                className="btn-sm btn-action set-main"
                                                onClick={() => handleSetMain(image.id)}
                                            >
                                                <FaStar className="me-1" /> Main
                                            </button>
                                        )}
                                        <button
                                            className="btn-sm btn-action delete"
                                            onClick={() => handleDelete(image.id)}
                                        >
                                            <FaTrash className="me-1" />
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                ) : (
                    <div className="empty-state text-center py-4">
                        <FaImages className="text-light mb-2" size={36} />
                        <h5 className="text-light mb-1">No images yet</h5>
                        <p className="text-light">Upload some images to create your product gallery</p>
                    </div>
                )}
            </div>
            <div className="d-flex justify-content-between mt-0 pt-0">
                <button 
                    className="btn-accent-outline"
                    onClick={handleBack}
                >
                    <FaArrowLeft className="me-2" /> Back
                </button>
                <button 
                    className="btn-accent-outline"
                    onClick={handleNext}
                >
                    Continue <FaArrowRight className="ms-2" />
                </button>
            </div>
        </div>
    );
};

export default AddImagesPage;