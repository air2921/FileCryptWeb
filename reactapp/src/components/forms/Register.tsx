import React, { FormEvent, useState } from 'react';
import axios from 'axios';
import Verify from './Verify';

const Register: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        try {
            const response = await axios.post('api/auth/register', {
                email: email,
                password_hash: password,
                username: username
            });

            if (response.status === 307) {
                setStatusCode(true);
            }
            else {
                setErrorMessage(response.data.message);
            }

        } catch (error) {
            console.error(error);
            setErrorMessage('Unknown error');
        }
    };

    return (
        <div>
            {successStatusCode ? (
                <Verify onSuccess={() => console.log('Verification success!')} />
            ) : (
                <div>
                    <p className="welcome-text">Welcome to FileCrypt. Let's start here</p>
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label htmlFor="email">Email*</label>
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
                            <label htmlFor="password">Password*</label>
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
                            <label htmlFor="username">Username*</label>
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
                                Continue
                            </button>
                        </div>
                    </form>
                    <div>
                        <div>
                            <h5 className="error-head">Error</h5>
                            {errorMessage && <span className="error-body">{errorMessage}</span>}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Register;