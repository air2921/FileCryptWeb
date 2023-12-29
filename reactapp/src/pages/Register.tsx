import React, { FormEvent, useState } from 'react';
import Verify from '../components/Verify/Verify';
import Input from '../components/Input/Input';
import AxiosRequest from '../api/AxiosRequest';
import Message from '../components/Message/Message';

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
                <Verify endpoint='api/auth/verify' method='POST' />
            ) : (
                    <div className="register-container">
                        <p className="welcome-text">Welcome to FileCrypt. Let's start our adventure here</p>
                        <form onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label htmlFor="email">email</label>
                                <Input type="email" id="email" value={email} onChange={(e) => setEmail(e.target.value)} />
                            </div>
                            <div className="form-group">
                                <label htmlFor="password">password</label>
                                <Input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)} />
                            </div>
                            <div className="form-group">
                                <label htmlFor="username">username</label>
                                <Input type="text" id="username" value={username} onChange={(e) => setUsername(e.target.value)} />
                            </div>
                            <div className="form-actions">
                                <button type="submit">
                                    continue
                                </button>
                            </div>
                        </form>
                        {errorMessage && <Message message={errorMessage} font={'error'} />}
                    </div>
            )}
        </div>
    );
}

export default Register;