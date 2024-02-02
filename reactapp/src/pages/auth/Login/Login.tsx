import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import AxiosRequest from '../../../api/AxiosRequest';
import Input from '../../../components/Helpers/Input';
import Message from '../../../components/Message/Message';
import Button from '../../../components/Helpers/Button';
import Verify from '../../../components/Verify/Verify';
import Modal from '../../../components/Modal/Modal';
import CreateRecovery from '../Recovery/CreateRecovery';

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
            if (response.statusCode === 201) {
                navigate('/');
            }
            else if (response.statusCode === 200) {
                setVerification(true);
            }
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
                    <Input text='Email address' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                    <Input text='Password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                    <Button>Sign In</Button>
                </form>
                {errorMessage && <Message message={errorMessage} font='error' />}
            </div>
            <Button onClick={() => setRecovery(true)}>Recovery account</Button>
            <Modal isActive={recoveryAccount} setActive={setRecovery}>
                <CreateRecovery />
            </Modal>
            <div className="signup-container">
                <a href="/signup">Create an account</a>
            </div>
            <Modal isActive={verificationRequired} setActive={setVerification}>
                <Verify endpoint='api/auth/verify/2fa' method='POST' />
            </Modal>
        </div>
    );
};

export default Login;