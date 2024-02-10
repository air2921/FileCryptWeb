import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';
import Modal from '../../../components/Modal/Modal';
import CreateRecovery from '../recovery/CreateRecovery';

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
        const [errorVerificationMessage, setVerificationErrorMessage] = useState('');

        const handleSubmit = async (e: FormEvent) => {
            e.preventDefault();

            const response = await AxiosRequest({ endpoint: `api/auth/verify/2fa?code=${code}`, method: 'POST', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                navigate('/');
            }
            else {
                setVerificationErrorMessage(response.data);
            }
        };

        return (
            <div>
                <p>Two-Factor Authentication</p>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="code">
                        Enter your numeric code from your email
                        <input
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
                    <button type="submit">Verify</button>
                </form>
                {errorVerificationMessage && <Message message={errorVerificationMessage} font='error' />}
            </div>
        );
    }

    return (
        <div className="login">
            <div className="login-container">
                <p className="welcome-text">Sign in to FileCryptWeb</p>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="email">
                        {email ? "Email" : "* Email"}
                        <input
                            type="email"
                            id="email"
                            required={true}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </label>
                    <label htmlFor="password">
                        {password ? "Password" : "* Password"}
                        <input
                            type="password"
                            id="password"
                            required={true}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </label>
                    <button type="submit">Sign In</button>
                    <button onClick={() => navigate('/login')}></button>
                </form>
            </div>
            <Modal isActive={verificationRequired} setActive={setVerification}>
                <Verify />
            </Modal>
            <button onClick={() => setRecovery(true)}>Recovery account</button>
            <Modal isActive={recoveryAccount} setActive={setRecovery}>
                <CreateRecovery />
            </Modal>
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
};

export default Login;