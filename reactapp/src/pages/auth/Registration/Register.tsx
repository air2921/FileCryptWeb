import React, { ChangeEvent, FormEvent, useState } from 'react';
import Verify from '../../../components/Verify/Verify';
import Input from '../../../components/Helpers/Input';
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';
import Button from '../../../components/Helpers/Button';
import CheckBox from '../../../components/Helpers/CheckBox';
import Modal from '../../../components/Modal/Modal';

const Register = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [is2Fa, set2Fa] = useState(false);
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({
            endpoint: 'api/auth/register',
            method: 'POST',
            withCookie: true,
            requestBody: {
                email: email,
                password: password,
                username: username,
                is_2fa_enabled: is2Fa
            }
        });

        if (response.isSuccess) {
            setStatusCode(true);
        }
        else {
            setErrorMessage(response.data);
        }
    };

    const handleInternalCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        set2Fa(e.target.checked);
    };

    return (
        <>
            <div className="register-container">
                <p className="welcome-text">Welcome to FileCryptWeb !</p>
                <form onSubmit={handleSubmit}>
                    <Input text='Username' type="text" id="username" require={true} value={username} onChange={(e) => setUsername(e.target.value)} />
                    <Input text='Email address' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                    <Input text='Password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                    <CheckBox type="checkbox" id="2fa" checked={is2Fa} onChange={handleInternalCheckboxChange} />
                    <Button>Continue</Button>
                </form>
                {errorMessage && <Message message={errorMessage} font='error' />}
            </div>
            <Modal isActive={successStatusCode} setActive={setStatusCode}>
                <Verify endpoint='api/auth/verify' method='POST' />
            </Modal>
        </>
    );
}

export default Register;