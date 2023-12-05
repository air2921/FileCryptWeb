import React, { FormEvent, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const Verify: React.FC<{ onSuccess: () => void }> = ({ onSuccess }) => {
    const [code, setCode] = useState(0);
    const [errorMessage, setErrorMessage] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post(`https://localhost:7067/api/auth/verify?code=${code}`, null, { withCredentials: true })
            const navigate = useNavigate();

            if (response.status === 201) {
                navigate('/');
            }
            else {
                setErrorMessage(response.data.message)
            }
        } catch (error) {
            console.error(error);
            setErrorMessage('Unknown error');
        }
    };

    return (
        <div>
            <p className="welcome-text">A confirmation code has been sent to your email address</p>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="code">Code*</label>
                    <input
                        type="number"
                        id="code"
                        value={code}
                        onChange={(e) => setCode(parseInt(e.target.value, 10))}
                        className="form-control"
                        required
                    />
                </div>
                <div className="form-actions">
                    <button type="submit">
                        Confirm
                    </button>
                </div>
            </form>
            <div>
                <div>
                    {errorMessage && <span className="error">{errorMessage}</span>}
                </div>
            </div>
        </div>
    );
}

export default Verify;