import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import Modal from '../../components/modal/Modal';
import Message from '../../utils/helpers/message/Message';
import AxiosRequest from '../../utils/api/AxiosRequest';
import { getUser } from '../../utils/api/user/User';
import { UserProps } from './UserProps';
import Loader from '../static/loader/Loader';
import ErrorPage from '../static/error-status/ErrorPage';

interface VerifyProps {
    endpoint: string;
    method: string;
}

interface TwoFaProps {
    isEnable: boolean;
}

const Verify = ({ endpoint, method }: VerifyProps) => {
    const [code, setCode] = useState<number>();
    const [verificationMessage, setVerificationMessage] = useState('');
    const [verificationIcon, setVerificationIcon] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `${endpoint}?code=${code}`, method: method, withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setVerificationMessage('Action confirmed, you can close this window');
            setVerificationIcon('check-circle');
        }
        else {
            setVerificationMessage(response.data);
            setVerificationIcon('error');
        }
    };

    return (
        <div>
            <p>Verify Action</p>
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
            {verificationMessage && <Message message={verificationMessage} font={verificationIcon} />}
        </div>
    );
}

const Settings = () => {
    const [globalMessage, setGlobalMessage] = useState('');
    const [status, setStatus] = useState(500)
    const [user, setUser] = useState<UserProps | null>();
    const [lastUpdate, setUpdate] = useState<Date>()

    const [message, setMessage] = useState('');
    const [icon, setIcon] = useState('')

    async function fetchUser() {
        const response = await getUser(0, true);
        if (response.success) {
            setUser(response.data.user)
        } else {
            setStatus(response.statusCode);
            setGlobalMessage(response.message!);
        }
    }

    //#region Username Component
        const Username = () => {
            const [username, setUsername] = useState('');

            const handleSubmit = async (e: FormEvent) => {
                e.preventDefault();

                const response = await AxiosRequest({ endpoint: `api/account/edit/username?username=${username}`, method: 'PUT', withCookie: true, requestBody: { username: username } })

                if (response.isSuccess) {
                    setMessage(response.data.message);
                    setIcon('done')
                }
                else {
                    setMessage(response.data);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('')
                }, 3000)
            }

            return (
                <div className="username">
                    <form onSubmit={handleSubmit}>
                        <label htmlFor="username">
                            {username ? "Username" : "* Username"}
                            <input
                                type="text"
                                id="username"
                                required={true}
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                            />
                        </label>
                        <button type="submit">Save Username</button>
                    </form>
                </div>
            );
        }
    //#endregion

    // #region Password Component
        const Password = () => {
            const [oldPassword, setOld] = useState('');
            const [newPassword, setNew] = useState('');

            const handleSubmit = async (e: FormEvent) => {
                e.preventDefault();

                const response = await AxiosRequest({
                    endpoint: `api/account/edit/password`,
                    method: 'PUT',
                    withCookie: true,
                    requestBody: {
                        OldPassword: oldPassword,
                        NewPassword: newPassword
                    }
                })

                if (response.isSuccess) {
                    setMessage(response.data.message);
                    setIcon('done')
                }
                else {
                    setMessage(response.data);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('')
                }, 3000)
            }

            return (
                <div className="password">
                    <form onSubmit={handleSubmit}>
                        <label htmlFor="old">
                            {oldPassword ? "Old Password" : "* Old Password"}
                            <input
                                type="password"
                                id="old"
                                required={true}
                                value={oldPassword}
                                onChange={(e) => setOld(e.target.value)}
                            />
                        </label>
                        <label htmlFor="new">
                            {newPassword ? "New Password" : "* New Password"}
                            <input
                                type="password"
                                id="new"
                                required={true}
                                value={newPassword}
                                onChange={(e) => setNew(e.target.value)}
                            />
                        </label>
                        <button type="submit">Save Password</button>
                    </form>
                </div>
            );
        }
    // #endregion

    // #region Email Component
        const Email = () => {
            const [successStatus, setStatus] = useState(false);
            const [password, setPassword] = useState('');

            const handleSubmit = async (e: FormEvent) => {
                e.preventDefault();

                const response = await AxiosRequest({ endpoint: `api/account/edit/email/start?password=${password}`, method: 'POST', withCookie: true, requestBody: null })

                if (response.isSuccess) {
                    setStatus(true);
                }
                else {
                    setMessage(response.data);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('')
                }, 3000)
            }

            return (
                <div>
                    {successStatus ? (
                        <Confirm />
                    ) : (
                        <div className="email">
                            <form onSubmit={handleSubmit}>
                                <label htmlFor="password">
                                    {password ? "Password" : "* Password"}
                                    <input
                                        type="password"
                                        id="password"
                                        required={true}
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                    />
                                </label>
                                <button type="submit">Confirm</button>
                            </form>
                        </div>
                    )}
                </div>
            );
        }
    // #endregion

    // #region Helped Component "Confirm"
        const Confirm = () => {
            const [successStatus, setStatus] = useState(false);

            const [email, setEmail] = useState('');
            const [code, setCode] = useState<number>();

            const handleSubmit = async (e: FormEvent) => {
                e.preventDefault();

                const response = await AxiosRequest({ endpoint: `api/account/edit/email/confirm/old?email=${email}&code=${code}`, method: 'POST', withCookie: true, requestBody: null })

                if (response.isSuccess) {
                    setStatus(true);
                }
                else {
                    setMessage(response.data);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('')
                }, 3000)
            }

            return (
                <div>
                    {successStatus ? (
                        <Verify endpoint='api/account/edit/email/confirm/new' method='PUT' />
                    ) : (
                        <div className="email-and-code">
                            <form onSubmit={handleSubmit}>
                                <label htmlFor="email">
                                    {email ? "Your new email" : "* Your new email"}
                                    <input
                                        type="email"
                                        id="email"
                                        required={true}
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                    />
                                </label>
                                <label htmlFor="code">
                                    {email ? "Confirmation code" : "* Confirmation code"}
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
                                    />
                                </label>
                                <button type="submit">Confirm</button>
                            </form>
                        </div>
                    )}
                </div>
            );
        }
    // #endregion

    // #region 2FA Component

        const TwoFA = ({ isEnable }: TwoFaProps) => {
            const [password, setPassword] = useState('');
            const [successStatus, setStatus] = useState(false);
            const [is2Fa, set2Fa] = useState(true);
            const [visibleForm, setFormVisible] = useState(false);

            const handleSubmit = async (e: FormEvent) => {
                e.preventDefault();

                const response = await AxiosRequest({ endpoint: `api/account/edit/2fa/start?password=${password}`, method: 'POST', withCookie: true, requestBody: null })

                if (response.isSuccess) {
                    setStatus(true);
                }
                else {
                    setMessage(response.data);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('')
                }, 3000)
            }

            const set2FaStatus = (twoFaStatus: boolean, formVisible: boolean) => {
                set2Fa(twoFaStatus);
                setFormVisible(formVisible);
            }

            return (
                <div>
                    {isEnable && !visibleForm && <button onClick={() => set2FaStatus(false, true)}>Disable 2FA</button>}
                    {!isEnable && !visibleForm && <button onClick={() => set2FaStatus(true, true)}>Enable 2FA</button>}
                    {visibleForm && (
                        <form onSubmit={handleSubmit}>
                            <label htmlFor="password">
                                {password ? "Password" : "* Password"}
                                <input
                                    type="password"
                                    id="password"
                                    required={true}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                />
                            </label>
                            <button type="submit">Send message</button>
                        </form>
                    )}
                    <Modal isActive={successStatus} setActive={setStatus}>
                        <Verify endpoint={`api/account/edit/2fa/confirm/${is2Fa}`} method={'PUT'} />
                    </Modal>
                </div>
            );
        }

    // #endregion

    useEffect(() => {
        fetchUser();
    }, [lastUpdate])

    if (!user) {
        return globalMessage ? <ErrorPage statusCode={status} message={globalMessage} /> : <Loader />;
    }

    return (
        <div className="container">
            <div className="data-container">

            </div>
            <div className="change-data-container">
                <Username />
                <Password />
                <TwoFA isEnable={user.is_2fa_enabled} />
                <Email />
            </div>
            {message && <Message message={message} font={icon} />}
        </div>
    );
}

export default Settings;