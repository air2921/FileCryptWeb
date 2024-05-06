import React, { FormEvent, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Message from '../../../utils/helpers/message/Message';
import './RecoveryAccount.css'
import { recoveryAccount } from '../../../utils/api/Auth';

const RecoveryAccount = () => {
    const location = useLocation();
    const searchParams = new URLSearchParams(location.search);
    const token = searchParams.get('token');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');
    const navigate = useNavigate();

    const recoveryAccountSubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (token === null || token === undefined) {
            return;
        }

        const result = await recoveryAccount(password, token);

        if (result.statusCode === 200) {
            navigate('/auth/login');
        } else {
            setMessage(result.message);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    return (
        <div className="account-recovery-container">
            <div className="account-recovery-header">
                Link available no more 30 minutes
            </div>
            <form className="account-recovery-form" onSubmit={recoveryAccountSubmit}>
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
                    <Message message={message} icon={font} />
                </div>
            }
        </div>
    );
}

export default RecoveryAccount;