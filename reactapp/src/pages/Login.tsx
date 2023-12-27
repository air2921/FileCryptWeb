import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import Error from '../components/Error/Error';
import AxiosRequest from '../api/AxiosRequest';
import Input from '../components/Input/Input';

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
                <p className="welcome-text"></p>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label htmlFor="email">email</label>
                        <Input type="email" id="email" value={email} onChange={(e) => setEmail(e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="password">password</label>
                        <Input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)} />
                    </div>
                    <button type="submit" className="btn btn-primary btn-disabled">
                        Sign In
                    </button>
                </form>
            </div>
            <div className="signup-container">
                <p>No? <a href="/signup">Create an account</a>And try sign in, after it.</p>
            </div>
            {errorMessage && <Error errorMessage={errorMessage} errorFont={'error'} />}
        </div>
    );
};

export default Login;