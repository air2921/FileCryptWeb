import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';

const Verify: React.FC<VerifyProps> = ({ endpoint }) => {
    const [code, setCode] = useState(0);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `${endpoint}?code=${code}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            navigate('/');
        }
        else {
            setErrorMessage(response.data);
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
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
}

export default Verify;