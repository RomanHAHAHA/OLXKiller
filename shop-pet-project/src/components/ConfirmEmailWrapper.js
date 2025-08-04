import { useLocation, Navigate } from 'react-router-dom';
import ConfirmEmail from '../components/ConfirmEmail';

const ConfirmEmailWrapper = () => {
  const location = useLocation();
  const email = location.state?.email;
  console.log("ConfirmEmailWrapper email from state:", email);

  if (!email) return <Navigate to="/register" />;

  return <ConfirmEmail email={email} />;
};

export default ConfirmEmailWrapper;
