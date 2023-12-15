import React, { FormEvent, useState } from 'react';
import axios from 'axios';
import Verify from '../components/Verify/Verify';
import Error from '../components/Error/Error';

const Register: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post('https://localhost:7067/api/auth/register', {
                email: email,
                password_hash: password,
                username: username
            }, { withCredentials: true });

            if (response.status === 200) {
                setStatusCode(true);
            } else {
                const errorMessage = response.data && response.data.message ? response.data.message : 'Unknown error';
                setErrorMessage(errorMessage);
            }

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
        <div>
            {successStatusCode ? (
                <Verify endpoint='https://localhost:7067/api/auth/verify' />
            ) : (
                <div>
                    <p className="welcome-text">Welcome to FileCrypt. Let's start our adventure here</p>
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
                        <div className="form-group">
                            <label htmlFor="username">username</label>
                            <input
                                type="text"
                                id="username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                className="form-control"
                                required
                            />
                        </div>
                        <div className="form-actions">
                            <button type="submit">
                                continue
                            </button>
                        </div>
                    </form>
                    {errorMessage && <Error errorMessage={errorMessage} errorFont={'error'} />}
                </div>
            )}
        </div>
    );
}

export default Register;