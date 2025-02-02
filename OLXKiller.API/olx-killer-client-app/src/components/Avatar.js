import React, { useEffect, useState } from "react";
import { motion } from "framer-motion";

const Avatar = () => {
  const [avatar, setAvatar] = useState(null);
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchAvatar = async () => {
    try {
      const response = await fetch("https://localhost:7208/api/Users/get-view-data", {
        method: "GET",
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Error fetching avatar");
      }

      const data = await response.json();
      setAvatar(data.data.avatarBytes);
    } catch (error) {
      console.error(error);
    }
  };

  useEffect(() => {
    fetchAvatar();
  }, []);

  const handleFileChange = (event) => {
    setFile(event.target.files[0]);
  };

  const handleUpload = async () => {
    if (!file) return;

    const formData = new FormData();
    formData.append("image", file);

    setLoading(true);

    try {
      const response = await fetch("https://localhost:7208/api/users/set-avatar", {
        method: "POST",
        body: formData,
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Error updating avatar");
      }

      fetchAvatar();
      alert("Avatar updated!");
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center p-6">
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="text-center space-y-4"
      >
        {avatar ? (
          <img
            src={`data:image/jpeg;base64,${avatar}`}
            alt="Avatar"
            style={{ width: '150px', height: '150px', borderRadius: '50%' }} 
          />
        ) : (
          <div
            style={{ width: '150px', height: '150px', borderRadius: '50%' }}
            className="bg-gray-500 flex items-center justify-center"
          >
            <span className="text-white">No avatar</span>
          </div>
        )}
  
        <input
          type="file"
          accept="image/*"
          onChange={handleFileChange}
          className="mb-4"
        />
        
        <button
          onClick={handleUpload}
          disabled={loading}
          className="bg-yellow-500 text-gray-900 p-2 rounded-lg transition-colors hover:bg-yellow-400"
        >
          {loading ? "Uploading..." : "Change Avatar"}
        </button>
      </motion.div>
    </div>
  );  
};

export default Avatar;
