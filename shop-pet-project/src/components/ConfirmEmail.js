import { useState, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../apiConfig';
import Swal from 'sweetalert2';

const ConfirmEmail = ({ email }) => {
  const [codeDigits, setCodeDigits] = useState(['', '', '', '', '', '']);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isResending, setIsResending] = useState(false);
  const inputRefs = useRef([]);
  const navigate = useNavigate();

  const showAlert = useCallback((icon, title, text) => {
    Swal.fire({
      icon,
      title,
      text,
      background: '#1a1a2e',
      color: '#ffffff',
      confirmButtonColor: '#4ecca3',
      timer: 3000
    });
  }, []);

  const handleChange = useCallback((index, value) => {
    if (!/^\d?$/.test(value)) return;

    const newDigits = [...codeDigits];
    newDigits[index] = value;
    setCodeDigits(newDigits);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  }, [codeDigits]);

  const handleKeyDown = useCallback((index, e) => {
    if (e.key === 'Backspace' && !codeDigits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  }, [codeDigits]);

  const handlePaste = useCallback(async (e) => {
    e.preventDefault();
    try {
      const text = await navigator.clipboard.readText();
      if (/^\d{6}$/.test(text)) {
        const digits = text.split('');
        setCodeDigits(digits);
        inputRefs.current[5]?.focus();
      }
    } catch (err) {
      console.warn('Clipboard read failed:', err);
    }
  }, []);

  const handleSubmit = useCallback(async (e) => {
    e.preventDefault();
    const code = codeDigits.join('');
    
    if (code.length !== 6) {
      showAlert('warning', 'Invalid Code', 'Please enter all 6 digits');
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await fetch(`${API_BASE_URL}emails-api/api/email-confirmations/confirm`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, code }),
      });

      if (response.ok) {
        showAlert('success', 'Email Confirmed', 'Your email has been successfully verified!');
        navigate("/login");
      } else {
        const error = await response.json();
        showAlert('error', 'Error', error.description || 'Invalid verification code');
      }
    } catch {
      showAlert('error', 'Server Error', 'Please try again later');
    } finally {
      setIsSubmitting(false);
    }
  }, [codeDigits, email, navigate, showAlert]);

  const handleResend = useCallback(async () => {
    setIsResending(true);
    try {
      const response = await fetch(`${API_BASE_URL}emails-api/api/email-confirmations/code`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
      });

      if (response.ok) {
        showAlert('success', 'Code Sent', 'A new verification code has been sent to your email');
      } else {
        const error = await response.json();
        showAlert('error', 'Error', error.description || 'Failed to send verification code');
      }
    } catch {
      showAlert('error', 'Server Error', 'Please try again later');
    } finally {
      setIsResending(false);
    }
  }, [email, showAlert]);

  return (
    <div className="d-flex justify-content-center align-items-center min-vh-100">
      <div className="w-100" style={{ maxWidth: '420px', marginTop: '-10vh' }}>
        <div className="text-center mb-4">
          <h2 className="text-light mb-2" style={{ color: '#4ecca3' }}>Verify Your Email</h2>
          <p className="text-light">We sent a code to <span className="fw-bold">{email}</span></p>
        </div>
        
        <form 
          onSubmit={handleSubmit} 
          className="p-4 rounded-3 bg-dark shadow" 
          style={{ border: '1px solid #2c2c3a' }}
        >
          <div className="mb-4">
            <div className="d-flex justify-content-between mb-3">
              {codeDigits.map((digit, index) => (
                <input
                  key={index}
                  type="text"
                  maxLength="1"
                  className="form-control text-center mx-1 bg-dark text-light border-secondary"
                  style={{ 
                    width: '45px', 
                    height: '60px', 
                    fontSize: '24px',
                    caretColor: '#4ecca3'
                  }}
                  value={digit}
                  ref={(el) => inputRefs.current[index] = el}
                  onChange={(e) => handleChange(index, e.target.value)}
                  onKeyDown={(e) => handleKeyDown(index, e)}
                  onPaste={handlePaste}
                  autoFocus={index === 0}
                />
              ))}
            </div>
            <p className="text-muted small">Enter the 6-digit verification code</p>
          </div>

          <button 
            type="submit" 
            className="btn w-100 py-2 mb-3" 
            disabled={isSubmitting || codeDigits.some(d => d === '')}
            style={{ 
              backgroundColor: '#4ecca3',
              border: 'none',
              fontWeight: 600
            }}
          >
            {isSubmitting ? (
              <>
                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Verifying...
              </>
            ) : 'Verify Email'}
          </button>
          
          <div className="text-center">
            <button 
              type="button" 
              className="btn btn-link text-decoration-none"
              onClick={handleResend}
              disabled={isResending}
              style={{ color: '#4ecca3', fontSize: '0.9rem' }}
            >
              {isResending ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Sending...
                </>
              ) : "Didn't receive a code? Resend"}
            </button>
          </div>
        </form>

        <div className="mt-4 text-center">
          <p className="text-light">
            Wrong email?{" "}
            <a 
              href="/register" 
              className="text-decoration-none fw-bold"
              style={{ color: '#4ecca3' }}
            >
              Go back
            </a>
          </p>
        </div>
      </div>
    </div>
  );
};

export default ConfirmEmail;