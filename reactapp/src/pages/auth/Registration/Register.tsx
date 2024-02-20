import React, { ChangeEvent, FormEvent, useState } from 'react';
import Modal from '../../../components/Modal/Modal';
import { useNavigate } from 'react-router-dom';
import Message from '../../../utils/helpers/message/Message';
import AxiosRequest from '../../../utils/api/AxiosRequest';
import './Registration.css'

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
            <div className="verification-container">
                <div className="verification-header">
                    Verify Account
                </div>
                <form className="verification-form" onSubmit={handleSubmit}>
                    <div className="verification-code-text">Enter your numeric code from your email</div>
                    <div className="verification-code-container">
                        <label htmlFor="code" className="verification-code-label">
                            <input className="verification-code-input"
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
                    </div>
                    <div className="verification-btn-container">
                        <button className="verification-btn" type="submit">Verify</button>
                    </div>
                </form>
                {errorVerificationMessage &&
                    <div className="verification-message">
                        <Message message={errorVerificationMessage} font='error' />
                    </div>
                }
            </div>
        );
    }

    return (
        <div className="registration">
            <div className="registration-container">
                <div className="registration-header">
                    Welcome to FileCryptWeb !
                </div>
                <form className="registraion-form" onSubmit={handleSubmit}>
                    <div className="registration-input-container">
                        <div className="username-container">
                            <div className="username-text">Username</div>
                            <label className="username-label">
                                <input className="username-input"
                                    type="text"
                                    id="username"
                                    required={true}
                                    value={username}
                                    onChange={(e) => setUsername(e.target.value)}
                                    placeholder="Username"
                                />
                            </label>
                        </div>
                        <div className="email-container">
                            <div className="email-text">Email</div>
                            <label htmlFor="email" className="email-label">
                                <input className="email-input"
                                    type="email"
                                    id="email"
                                    required={true}
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder="Email"
                                />
                            </label>
                        </div>
                        <div className="password-container">
                            <div className="password-text">Password</div>
                            <label htmlFor="password" className="password-label">
                                <input className="password-input"
                                    type="password"
                                    id="password"
                                    required={true}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="Password"
                                />
                            </label>
                        </div>
                        <div className="twoFa-container">
                            <div className="twoFa-text">Enable 2FA</div>
                            <label htmlFor="2fa" className="twoFa-label">
                                <input className="twoFa-input"
                                    type="checkbox"
                                    id="2fa"
                                    checked={is2Fa}
                                    onChange={handleCheckboxChange}
                                />
                            </label>
                        </div>
                    </div>
                    <div className="registration-form-btn-container">
                        <button className="registration-form-btn" type="submit">Continue</button>
                    </div>
                </form>
                {errorMessage &&
                    <div className="verification-message">
                        <Message message={errorMessage} font='error' />
                    </div>
                }
            </div>
            <Modal isActive={successStatusCode} setActive={setStatusCode}>
                <Verify />
            </Modal>
        </div>
    );
}

export default Register;