import React, { FormEvent, useState } from 'react';
import Verify from '../components/Verify/Verify';
import Input from '../components/Helpers/Input';
import AxiosRequest from '../api/AxiosRequest';
import Message from '../components/Message/Message';
import Button from '../components/Helpers/Button';

const Register = () => {
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
                        <p className="welcome-text">Welcome to FileCryptWeb! Let's start secure your data here</p>
                        <form onSubmit={handleSubmit}>
                            <Input text='Username' type="text" id="username" require={true} value={username} onChange={(e) => setUsername(e.target.value)} />
                            <Input text='Email address' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                            <Input text='Password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                            <Button>Continue</Button>
                        </form>
                        {errorMessage && <Message message={errorMessage} font='error' />}
                    </div>
            )}
        </div>
    );
}

export default Register;