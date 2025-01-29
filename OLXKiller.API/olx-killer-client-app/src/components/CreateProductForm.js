import React, { useState } from "react";
import Swal from "sweetalert2";
import { handleUnauthorizedError } from "../utils/handleUnauthorizedError";
import { useNavigate } from "react-router-dom";
import ImageUploadForm from "../components/ImageUploadForm"

const CreateProductForm = () => {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    price: "",
    amount: "",
  });
  const [errors, setErrors] = useState({});
  const [productId, setProductId] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prevFormData) => {
      let newValue = value;

      if (name === "price") {
        newValue = parseFloat(value);
        if (isNaN(newValue) || value === "-") {
          newValue = value;
        }
      } else if (name === "amount") {
        newValue = parseInt(value, 10);
        if (isNaN(newValue) || value === "-") {
          newValue = value;
        }
      }

      return {
        ...prevFormData,
        [name]: newValue,
      };
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);

    const formattedData = {
      ...formData,
      price: parseFloat(formData.price) || null,
      amount: parseInt(formData.amount, 10) || null,
    };

    try {
      const response = await fetch("https://localhost:7208/api/Products/create", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(formattedData),
      });

      if (response.ok) {
        const data = await response.json();
        setProductId(data.productId);
        setErrors({});
      } else {
        if (response.status === 401) {
          handleUnauthorizedError();
          return;
        }

        const error = await response.json();
        const validationErrors = {};

        for (const field in error.errors) {
          if (error.errors[field] && error.errors[field].length > 0) {
            validationErrors[field] = error.errors[field][0];
          }
        }

        setErrors(validationErrors);
      }
    } catch (err) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An error occurred. Please try again.",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleImagesUploaded = () => {
    Swal.fire({
      icon: "success",
      title: "Success",
      text: "Product created and images uploaded successfully!",
    }).then(() => {
      navigate("/");
    });
  };

  return (
    <div className="container mt-5" style={{ maxWidth: "600px" }}>
      <h2 className="text-center mb-4 text-light">Create Product</h2>
      {!productId ? (
        <form onSubmit={handleSubmit} className="p-4 border rounded-3 bg-dark shadow-sm">
          <div className="mb-3">
            <label htmlFor="name" className="form-label text-light">
              Name
            </label>
            <input
              type="text"
              id="name"
              name="name"
              className={`form-control ${errors["Name"] ? "is-invalid text-light" : "bg-secondary text-light border-0"}`}
              value={formData.name}
              onChange={handleChange}
              style={{ backgroundColor: "#343a40" }}
            />
            {errors["Name"] && <div className="invalid-feedback">{errors["Name"]}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="description" className="form-label text-light">
              Description
            </label>
            <textarea
              id="description"
              name="description"
              className={`form-control ${errors["Description"] ? "is-invalid text-light" : "bg-secondary text-light border-0"}`}
              value={formData.description}
              onChange={handleChange}
              style={{ backgroundColor: "#343a40" }}
            />
            {errors["Description"] && <div className="invalid-feedback">{errors["Description"]}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="price" className="form-label text-light">
              Price
            </label>
            <input
              type="text"
              id="price"
              name="price"
              className={`form-control ${errors["Price"] ? "is-invalid text-light" : "bg-secondary text-light border-0"}`}
              value={formData.price}
              onChange={handleChange}
              style={{ backgroundColor: "#343a40" }}
            />
            {errors["Price"] && <div className="invalid-feedback">{errors["Price"]}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="amount" className="form-label text-light">
              Amount
            </label>
            <input
              type="text"
              id="amount"
              name="amount"
              className={`form-control ${errors["Amount"] ? "is-invalid text-light" : "bg-secondary text-light border-0"}`}
              value={formData.amount}
              onChange={handleChange}
              style={{ backgroundColor: "#343a40" }}
            />
            {errors["Amount"] && <div className="invalid-feedback">{errors["Amount"]}</div>}
          </div>

          <button type="submit" className="btn btn-primary w-100" disabled={isSubmitting}>
            {isSubmitting ? "Submitting..." : "Create Product"}
          </button>
        </form>
      ) : (
        <ImageUploadForm productId={productId} onImagesUploaded={handleImagesUploaded}/>
      )}
    </div>
  );
};

export default CreateProductForm;
