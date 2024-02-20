import React, { FormEvent, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Message from '../../../utils/helpers/message/Message';
import AxiosRequest from '../../../utils/api/AxiosRequest';
import './RecoveryAccount.css'

const RecoveryAccount = () => {
    const location = useLocation();
    const searchParams = new URLSearchParams(location.search);
    const token = searchParams.get('token');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/auth/recovery/account?password=${password}&token=${token}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            navigate('/auth/login');
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    };

    return (
        <div className="account-recovery-container">
            <div className="account-recovery-header">
                Link available no more 30 minutes
            </div>
            <form className="account-recovery-form" onSubmit={handleSubmit}>
                <div className="account-recovery-text">
                    Enter you new password
                </div>
                <div className="recovery-password-container">
                    <label htmlFor="password">
                        <input className="password-input"
                            type="password"
                            id="password"
                            required={true}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </label>
                </div>
                <div className="account-recovery-btn-container">
                    <button className="account-recovery-btn-submit" type="submit">Submit</button>
                </div>
            </form>
            {message &&
                <div className="recovery-message">
                    <Message message={message} font={font} />
                </div>
            }
        </div>
    );
}

export default RecoveryAccount;