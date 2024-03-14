import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import AxiosRequest from '../../../utils/api/AxiosRequest';
import Modal from '../../../components/Modal/Modal';
import CreateRecovery from '../recovery/CreateRecovery';
import Message from '../../../utils/helpers/message/Message';
import './Login.css'

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [verificationRequired, setVerification] = useState(false);
    const [recoveryAccount, setRecovery] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: 'api/auth/login', method: 'POST', withCookie: true, requestBody: { email: email, password: password, } });

        if (response.isSuccess) {
            if (response.data.confirm !== null && response.data.confirm !== undefined) {
                localStorage.setItem('login_email', email);
                setVerification(true);
            }
            else {
                navigate('/');
            }
        }
        else {
            setErrorMessage(response.data);

            setTimeout(() => {
                setErrorMessage('');
            }, 5000)
        }
    };

    const Verify = () => {
        const [code, setCode] = useState<number>();

        const handleSubmit = async (e: FormEvent) => {
            e.preventDefault();

            const email = localStorage.getItem('login_email');

            const response = await AxiosRequest({ endpoint: `api/auth/verify/2fa?code=${code}&email=${email}`, method: 'POST', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                navigate('/');
                localStorage.removeItem('login_email');
            }
            else {
                setErrorMessage(response.data);

                setTimeout(() => {
                    setErrorMessage('');
                }, 5000)
            }
        };

        return (
            <div className="verify-container">
                <div className="verify-header">
                    Two-Factor Authentication
                </div>
                <form className="verify-form" onSubmit={handleSubmit}>
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
        <div className="login">
            <div className="login-container">
                <div className="login-header">
                    Sign in to FileCryptWeb
                </div>
                <form className="login-form" onSubmit={handleSubmit}>
                    <div className="input-container">
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
                    </div>
                    <div className="auth-container">
                        <button className="signin-form-btn" type="submit">Login</button>
                        <button className="signup-form-btn" onClick={() => navigate('/auth/signup')}>Register</button>
                    </div>
                </form>
                <div className="login-recovery-btn-container">
                    <button className="login-recovery-btn" onClick={() => setRecovery(true)}>Recovery account</button>
                </div>
            </div>
            <Modal isActive={verificationRequired} setActive={setVerification}>
                <Verify />
            </Modal>
            <Modal isActive={recoveryAccount} setActive={setRecovery}>
                <CreateRecovery />
            </Modal>
            <div className="message">
                {errorMessage && <Message message={errorMessage} font='error' />}
            </div>
        </div>
    );
};

export default Login;