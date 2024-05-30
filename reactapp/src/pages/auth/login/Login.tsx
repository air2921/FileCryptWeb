import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import Modal from '../../../components/modal/Modal';
import CreateRecovery from '../recovery/CreateRecovery';
import Message from '../../../components/widgets/Message';
import { login, verifyLogin } from '../../../utils/api/Auth';
import './Login.css'

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [verificationRequired, setVerification] = useState(false);
    const [recoveryAccount, setRecovery] = useState(false);
    const [code, setCode] = useState<number>();
    const navigate = useNavigate();

    const loginSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const result = await login(email, password);
        if (result.statusCode === 200 && result.verificationRequired) {
            setVerification(true);
        } else if (result.statusCode === 200 && !result.verificationRequired) {
            navigate('/');
        } else {
            setErrorMessage(result.message);

            setTimeout(() => {
                setErrorMessage('');
            }, 5000)
        }
    };

    const verifySubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (code === undefined) {
            setErrorMessage('Code cannot be empty');

            setTimeout(() => {
                setErrorMessage('');
            }, 5000)
            return;
        }

        const result = await verifyLogin(code);
        if (result.statusCode === 200) {
            navigate('/')
        } else {
            setErrorMessage(result.message);

            setTimeout(() => {
                setErrorMessage('');
            }, 5000)
        }
    };

    const Verify = () => {
        return (
            <div className="verify-container">
                <div className="verify-header">
                    Two-Factor Authentication
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
        <div className="login">
            <div className="login-container">
                <div className="login-header">
                    Sign in to FileCryptWeb
                </div>
                <form className="login-form" onSubmit={loginSubmit}>
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
                {errorMessage && <Message message={errorMessage} icon='error' />}
            </div>
        </div>
    );
};

export default Login;