import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';
import Verify from '../../../components/Verify/Verify';
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

            //if (response.statusCode === 201) {
            //    navigate('/');
            //}
            //else if (response.statusCode === 200) {
            //    setVerification(true);
            //}
        }
        else {
            setErrorMessage(response.data);

            setTimeout(() => {
                setErrorMessage('');
            }, 5000)
        }
    };

    return (
        <div className="login">
            <div className="login-container">
                <p className="welcome-text">Sign in to FileCryptWeb</p>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="email">
                        {email ? "Email:" : "Email*"}
                        <input
                            type="email"
                            id="email"
                            required={true}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </label>
                    <label htmlFor="password">
                        {password ? "Password:" : "Password*"}
                        <input
                            type="password"
                            id="password"
                            required={true}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </label>
                    <button type="submit">Sign In</button>
                </form>
            </div>
            <button onClick={() => setRecovery(true)}>Recovery account</button>
            <Modal isActive={recoveryAccount} setActive={setRecovery}>
                <CreateRecovery />
            </Modal>
            <div className="signup-container">
                <a href="/signup">Create an account</a>
            </div>
            <Modal isActive={verificationRequired} setActive={setVerification}>
                <Verify endpoint='api/auth/verify/2fa' method='POST' />
            </Modal>
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
};

export default Login;