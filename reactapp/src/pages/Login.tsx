import axios from 'axios';
import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom'
import Error from '../components/Error/Error';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post('https://localhost:7067/api/auth/login', {
                email: email,
                password_hash: password,
            }, { withCredentials: true });

            navigate("/")

        } catch (error: any) {
            console.error(error);
            if (error.response) {
                const errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
                setErrorMessage(errorMessage);
            } else {
                setErrorMessage('Unknown error');
            }
        }
    };

    return (
        <div className="content">
            <div className="login-signup-container">
                <div className="login-container">
                    <p className="welcome-text"></p>
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label htmlFor="email">email</label>
                            <input
                                type="email"
                                id="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                className="form-control"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="password">password</label>
                            <input
                                type="password"
                                id="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                className="form-control"
                                required
                            />
                        </div>
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Sign In
                        </button>
                    </form>
                </div>
                <div className="signup-container">
                    <p>No? <a href="/signup">Create an account</a>And try sign in, after it.</p>
                </div>
            </div>
            {errorMessage && <Error errorMessage={errorMessage} errorFont={'error'} />}
        </div>
    );
};

export default Login;