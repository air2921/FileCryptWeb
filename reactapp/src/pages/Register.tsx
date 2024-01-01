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
                            <Input type="text" id="username" require={true} value={username} onChange={(e) => setUsername(e.target.value)} />
                            <Input type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                            <Input type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                            <button type="submit">
                                continue
                            </button>
                        </form>
                        {errorMessage && <Message message={errorMessage} font='error' />}
                    </div>
            )}
        </div>
    );
}

export default Register;