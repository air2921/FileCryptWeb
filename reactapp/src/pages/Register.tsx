import React, { FormEvent, useState } from 'react';
import Verify from '../components/Verify/Verify';
import Error from '../components/Error/Error';
import AxiosRequest from '../api/AxiosRequest';

const Register: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: 'api/auth/register', method: 'POST', withCookie: true, requestBody: { email: email, password_hash: password, username: username } });

        if (response.isSuccess) {
            setStatusCode(true);
        }
        else {
            setErrorMessage(response.data);
        }
    };

    return (
        <div>
            {successStatusCode ? (
                <Verify endpoint='api/auth/verify' />
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