import { API_BASE_URL } from "../apiConfig";
import imagePlaceholder from "../asserts/imagePlaceholder.jpg";
import defaultAvatarImage from "../asserts/default_avatar_image.png";
import { useNavigate } from "react-router-dom";

const avatarUrl = `${API_BASE_URL}user-images/`;
const imagesUrl = `${API_BASE_URL}product-images/`;

const ConfirmedOrderCard = ({ order }) => {
  const naviage = useNavigate();
  
    const getAvatarSrc = () => {
    return order.user.avatarName
      ? `${avatarUrl}${order.user.avatarName}`
      : defaultAvatarImage;
  };

  const getProductImageSrc = (path) => {
    return path ? `${imagesUrl}${path}` : imagePlaceholder;
  };

  const handleProductClick = (productId) => {
    naviage(`/products/${productId}`);
  }

  return (
    <div className="card bg-dark text-light shadow mb-4 rounded-4">
      <div className="card-body">
        <div className="d-flex justify-content-between mb-3">
          <div>
            <h5 className="card-title">Order #{order.id}</h5>
            <p className="mb-1"><strong>Status:</strong> {order.status}</p>
            <p className="mb-1"><strong>Created:</strong> {order.createdAt}</p>
          </div>
          <div className="text-end">
            <img
              src={getAvatarSrc()}
              alt="User avatar"
              className="rounded-circle object-fit-cover"
              width="64"
              height="64"
            />
            <p className="mb-0 mt-2">{order.user.nickName}</p>
          </div>
        </div>

        <p><strong>Delivery:</strong> {order.deliveryLocation.region}, {order.deliveryLocation.city}, {order.deliveryLocation.warehouse}</p>

        <div className="row">
          {order.orderItems.map((item, index) => (
            <div className="col-md-6 col-lg-4 mb-3" key={index}>
              <div className="card bg-secondary text-light h-100 shadow-sm rounded-3" onClick={() => handleProductClick(item.product.id)}>
                <img
                  src={getProductImageSrc(item.product.mainImagePath)}
                  className="card-img-top object-fit-cover"
                  alt={item.product.name}
                  style={{ height: "200px", objectFit: "cover" }}
                />
                <div className="card-body">
                  <h6 className="card-title">{item.product.name}</h6>
                  <p className="mb-1">Price: ${item.product.price}</p>
                  <p className="mb-0">Quantity: {item.quantity}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ConfirmedOrderCard;