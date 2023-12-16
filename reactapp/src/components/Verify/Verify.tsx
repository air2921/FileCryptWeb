import React, { FormEvent, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import Error from '../Error/Error';

const Verify: React.FC<VerifyProps> = ({ endpoint }) => {
    const [code, setCode] = useState(0);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post(`${endpoint}?code=${code}`, null, { withCredentials: true })
            navigate("/");

        } catch (error: any) {
            console.error(error);
            if (error.response) {
                const errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
                setErrorMessage(errorMessage);
            } else {
                setErrorMessage('Unknown error');
            }
        }
    };

    return (
        <div>
            <p className="welcome-text">A confirmation code has been sent to your email address</p>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="code">code</label>
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
            {errorMessage && <Error errorMessage={errorMessage} errorFont={'error'} />}
        </div>
    );
}

export default Verify;