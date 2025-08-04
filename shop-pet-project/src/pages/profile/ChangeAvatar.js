import { useEffect, useRef, useState } from "react";
import { API_BASE_URL } from "../../apiConfig";
import imagePlaceholder from '../../asserts/default_avatar_image.png'; 
import { useAuth } from "../../AuthProvider";
import { useSignalR } from "../../SignalRProvider";
import Swal from "sweetalert2";
import { PulseLoader } from "react-spinners";
import styles from '../../Styles/ChangeAvatar.module.css'; 

const AVATAR_URL = `${API_BASE_URL}user-images/`;
const UPDATE_AVATAR_URL = `${API_BASE_URL}users-api/api/users/me/avatar`;

const ChangeAvatar = () => {
  const { user, refreshToken } = useAuth(); 
  const { connection } = useSignalR();
  const [preview, setPreview] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const fileInputRef = useRef();

  const handleAvatarClick = () => {
    fileInputRef.current.click();
  };

  const handleFileChange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    // Валидация файла (размер и тип)
    const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
    const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

    if (!ALLOWED_TYPES.includes(file.type)) {
      Swal.fire({
        icon: 'error',
        title: 'Invalid file type',
        text: 'Please upload a JPEG, PNG, or WebP image.',
      });
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      Swal.fire({
        icon: 'error',
        title: 'File too large',
        text: 'Maximum file size is 5MB.',
      });
      return;
    }

    setPreview(URL.createObjectURL(file));

    try {
      setIsLoading(true);
      const formData = new FormData();
      formData.append("File", file);

      const response = await fetch(UPDATE_AVATAR_URL, {
        method: "PATCH",
        body: formData,
        credentials: "include",
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Failed to update avatar");
      }

      // Если API не отправляет уведомление через SignalR, показываем успех здесь
      if (!connection) {
        await refreshToken();
        Swal.fire({
          title: "Success!",
          text: "Avatar updated successfully",
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
        });
      }
    } catch (error) {
      console.error("Avatar upload error:", error);
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: error.message || "An error occurred while updating avatar",
      });
      setPreview(null); // Сбрасываем превью при ошибке
    } finally {
      setIsLoading(false);
    }
  };

  // Обработчики SignalR
  useEffect(() => {
    if (!connection) return;

    const handleAvatarUpdated = (message) => {
      refreshToken();
      Swal.fire({
        title: "Success!",
        text: message,
        icon: 'success',
        timer: 2000,
        showConfirmButton: false,
      });
      setPreview(null); // Сбрасываем превью после успешного обновления
    };

    const handleAvatarUpdateFailed = (message) => {
      Swal.fire({
        title: "Error",
        text: message,
        icon: 'error',
      });
      setPreview(null);
    };

    connection.on("NotifyUserAvatarUpdated", handleAvatarUpdated);
    connection.on("NotifyUserAvatarUpdateFailed", handleAvatarUpdateFailed);

    return () => {
      connection.off("NotifyUserAvatarUpdated", handleAvatarUpdated);
      connection.off("NotifyUserAvatarUpdateFailed", handleAvatarUpdateFailed);
    };
  }, [connection, refreshToken]);

  const avatarSrc = preview 
    ? preview 
    : user?.avatarImageName 
      ? `${AVATAR_URL}${user.avatarImageName}?_t=${Date.now()}` // Добавляем timestamp для избежания кеширования
      : imagePlaceholder;

  return (
    <div className={styles.container}>
      <h5 className={styles.title}>Profile Picture</h5>
      
      <div 
        className={styles.avatarWrapper}
        onClick={handleAvatarClick}
      >
        <img
          src={avatarSrc}
          onError={(e) => {
            e.currentTarget.src = imagePlaceholder;
          }}
          alt="User avatar"
          className={styles.avatarImage}
        />

        {isLoading && (
          <div className={styles.loaderOverlay}>
            <PulseLoader color="#ffffff" size={10} />
          </div>
        )}

        <div className={styles.changeHint}>
          <span>Click to change</span>
        </div>
      </div>

      <input
        type="file"
        accept="image/jpeg, image/png, image/webp"
        ref={fileInputRef}
        onChange={handleFileChange}
        className={styles.fileInput}
      />
    </div>
  );
};

export default ChangeAvatar;