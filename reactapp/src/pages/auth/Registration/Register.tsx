import React, { ChangeEvent, FormEvent, useState } from 'react';
import Modal from '../../../components/Modal/Modal';
import { useNavigate } from 'react-router-dom';
import Message from '../../../utils/helpers/message/Message';
import AxiosRequest from '../../../utils/api/AxiosRequest';

const Register = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [username, setUsername] = useState('');
    const [is2Fa, set2Fa] = useState(false);
    const [successStatusCode, setStatusCode] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

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

    const handleCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        set2Fa(e.target.checked);
    };

    const Verify = () => {
        const [code, setCode] = useState<number>();
        const [errorVerificationMessage, setVerificationErrorMessage] = useState('');

        const handleSubmit = async (e: FormEvent) => {
            e.preventDefault();

            const response = await AxiosRequest({ endpoint: `api/auth/verify?code=${code}`, method: 'POST', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                navigate('/');
            }
            else {
                setVerificationErrorMessage(response.data);
            }
        };

        return (
            <div>
                <p>Verify Account</p>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="code">
                        Enter your numeric code from your email
                        <input
                            type="text"
                            id="code"
                            required={true}
                            value={code}
                            onChange={(e) => {
                                const value = e.target.value;
                                if (value === '') {
                                    setCode(undefined);
                                } else {
                                    const parsedValue = parseInt(value, 10);
                                    if (!isNaN(parsedValue)) {
                                        setCode(parsedValue);
                                    }
                                }
                            }}
                            inputMode="numeric"
                            placeholder="Code"
                        />
                    </label>
                    <button type="submit">Verify</button>
                </form>
                {errorVerificationMessage && <Message message={errorVerificationMessage} font='error' />}
            </div>
        );
    }

    return (
        <>
            <div className="register-container">
                <p className="welcome-text">Welcome to FileCryptWeb !</p>
                <form onSubmit={handleSubmit}>
                    <label>
                        {username ? "Username" : "* Username"}
                        <input
                            type="text"
                            id="username"
                            required={true}
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            placeholder="air2921"
                        />
                    </label>
                    <label>
                        {email ? "* Email" : "* Email"}
                        <input
                            type="email"
                            id="email"
                            required={true}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Air2921@gmail.com"
                        />
                    </label>
                    <label>
                        {password ? "Password:" : "* Password"}
                        <input
                            type="password"
                            id="password"
                            required={true}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </label>
                    <label htmlFor="2fa">
                        <input
                            type="checkbox"
                            id="2fa"
                            checked={is2Fa}
                            onChange={handleCheckboxChange}
                        />
                    </label>
                    <button type="submit">Register</button>
                </form>
                {errorMessage && <Message message={errorMessage} font='error' />}
            </div>
            <Modal isActive={successStatusCode} setActive={setStatusCode}>
                <Verify />
            </Modal>
        </>
    );
}

export default Register;