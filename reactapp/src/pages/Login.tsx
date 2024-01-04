import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import AxiosRequest from '../api/AxiosRequest';
import Input from '../components/Helpers/Input';
import Message from '../components/Message/Message';
import Button from '../components/Helpers/Button';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: 'api/auth/login', method: 'POST', withCookie: true, requestBody: { email: email, password_hash: password, } });

        if (response.isSuccess) {
            navigate('/')
        }
        else {
            setErrorMessage(response.data);
        }
    };

    return (
        <div className="login">
            <div className="login-container">
                <p className="welcome-text">
                    Have an account ?
                    Sign In
                </p>
                <form onSubmit={handleSubmit}>
                    <Input text='Login' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                    <Input text='Password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                    <Button>Sign In</Button>
                </form>
                {errorMessage && <Message message={errorMessage} font='error' />}
            </div>
            <div className="signup-container">
                <p>No? <a href="/signup">Create an account</a>And try sign in, after it.</p>
            </div>
        </div>
    );
};

export default Login;