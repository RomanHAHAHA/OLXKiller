import React, { useState } from "react";
import Swal from "sweetalert2";

const ImageUploadForm = ({ productId, onImagesUploaded }) => {
  const [images, setImages] = useState([]);

  const handleImageChange = (e) => {
    const files = e.target.files;
    const validImages = [];
    let valid = true;
    
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      if (file.type !== "image/jpeg") {
        valid = false;
        Swal.fire({
          icon: "error",
          title: "Invalid File Type",
          text: `Only JPG files are allowed. File "${file.name}" is not a JPG.`,
        });
        break;
      }
      if (file.size > 2 * 1024 * 1024) { 
        valid = false;
        Swal.fire({
          icon: "error",
          title: "File Too Large",
          text: `File "${file.name}" exceeds the size limit of 2 MB.`,
        });
        break;
      }
      validImages.push(file); 
    }

    if (valid) {
      setImages(validImages);
    } else {
      setImages([]); 
    }
  };

  const handleAddImages = async (e) => {
    e.preventDefault();

    if (!images || images.length === 0) {
      Swal.fire({
        icon: "warning",
        title: "No Images",
        text: "Please select images to upload.",
      });
      return;
    }

    const formData = new FormData();
    Array.from(images).forEach((image) => {
      formData.append("images", image);
    });

    try {
      const response = await fetch(
        `https://localhost:7208/api/Products/add-images-to-product/${productId}`,
        {
          method: "POST",
          credentials: "include", 
          body: formData,
        }
      );

      if (response.ok) {
        Swal.fire({
          icon: "success",
          title: "Images Added",
          text: "Images uploaded successfully!",
        });
        onImagesUploaded();
      } else {
        const error = await response.json();
        Swal.fire({
          icon: "error",
          title: "Error",
          text: error.description || "Failed to upload images.",
        });
      }
    } catch (err) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An error occurred while uploading images.",
      });
    }
  };

  return (
    <form onSubmit={handleAddImages} className="p-4 border rounded-3 bg-dark shadow-sm">
      <div className="mb-3">
        <label htmlFor="images" className="form-label text-light">
          Upload Images
        </label>
        <input
          type="file"
          id="images"
          name="images"
          multiple
          accept="image/jpeg" 
          className="form-control bg-secondary text-light border-0"
          onChange={handleImageChange}
        />
      </div>
      <button type="submit" className="btn btn-primary w-100">
        Upload Images
      </button>
    </form>
  );
};

export default ImageUploadForm;
