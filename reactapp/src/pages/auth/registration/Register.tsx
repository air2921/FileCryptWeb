import React, { ChangeEvent, FormEvent, useState } from 'react';
import Modal from '../../../components/modal/Modal';
import { useNavigate } from 'react-router-dom';
import Message from '../../../utils/helpers/message/Message';
import './Registration.css'
import { registration, verifyRegistration } from '../../../utils/api/Auth';

const Register = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [is2Fa, set2Fa] = useState(false);
    const [code, setCode] = useState<number>();
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const registerSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const result = await registration(email, password, username, is2Fa);

        if (result.statusCode === 200) {
            setStatusCode(true);
        } else {
            setErrorMessage(result.message);
        }

        setTimeout(() => {
            setErrorMessage('');
        }, 5000)
    }

    const verifySubmit = async (e: FormEvent) => {
        if (code === null || code === undefined) {
            return;
        }

        const result = await verifyRegistration(code);

        if (result.statusCode === 201) {
            navigate('/auth/login')
        } else {
            setErrorMessage(result.message ? result.message : 'Unexpected error');
        }

        setTimeout(() => {
            setErrorMessage('');
        }, 5000)
    }

    const handleCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        set2Fa(e.target.checked);
    };

    const Verify = () => {
        return (
            <div className="verify-container">
                <div className="verify-header">
                    Account Verification
                </div>
                <form className="verify-form" onSubmit={verifySubmit}>
                    <div className="code-text">Enter your numeric code from your email</div>
                    <div className="code-container">
                        <label htmlFor="code" className="code-label">
                            <input className="code-input"
                                type="text"
                                id="code"
                                required={true}
                                value={code}
                                onChange={(e) => {
                                    const value = e.target.value;
                                    if (value === '') {
                                        setCode(undefined);
                                    } else {
                                        const parsedValue = parseInt(value, 10);
                                        if (!isNaN(parsedValue)) {
                                            setCode(parsedValue);
                                        }
                                    }
                                }}
                                inputMode="numeric"
                                placeholder="Code"
                            />
                        </label>
                    </div>
                    <div className="verify-btn-container">
                        <button className="verify-btn" type="submit">Verify</button>
                    </div>
                </form>
            </div>
        );
    }

    return (
        <div className="registration">
            <div className="registration-container">
                <div className="registration-header">
                    Welcome to FileCryptWeb !
                </div>
                <form className="registraion-form" onSubmit={registerSubmit}>
                    <div className="registration-input-container">
                        <div className="username-container">
                            <div className="username-text">Username</div>
                            <label className="username-label">
                                <input className="username-input"
                                    type="text"
                                    id="username"
                                    required={true}
                                    value={username}
                                    onChange={(e) => setUsername(e.target.value)}
                                    placeholder="Username"
                                />
                            </label>
                        </div>
                        <div className="email-container">
                            <div className="email-text">Email</div>
                            <label htmlFor="email" className="email-label">
                                <input className="email-input"
                                    type="email"
                                    id="email"
                                    required={true}
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder="Email"
                                />
                            </label>
                        </div>
                        <div className="password-container">
                            <div className="password-text">Password</div>
                            <label htmlFor="password" className="password-label">
                                <input className="password-input"
                                    type="password"
                                    id="password"
                                    required={true}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="Password"
                                />
                            </label>
                        </div>
                        <div className="twoFa-container">
                            <div className="twoFa-text">Enable 2FA</div>
                            <label htmlFor="2fa" className="twoFa-label">
                                <input className="twoFa-input"
                                    type="checkbox"
                                    id="2fa"
                                    checked={is2Fa}
                                    onChange={handleCheckboxChange}
                                />
                            </label>
                        </div>
                    </div>
                    <div className="registration-form-btn-container">
                        <button className="registration-form-btn" type="submit">Continue</button>
                    </div>
                </form>
            </div>
            <Modal isActive={successStatusCode} setActive={setStatusCode}>
                <Verify />
            </Modal>
            <div className="message">
                {errorMessage && <Message message={errorMessage} icon='error' />}
            </div>
        </div>
    );
}

export default Register;