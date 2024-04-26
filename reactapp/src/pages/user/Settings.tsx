import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import Modal from '../../components/modal/Modal';
import Message from '../../utils/helpers/message/Message';
import * as api from '../../utils/api/user/User';
import { UserProps } from './UserProps';
import Loader from '../static/loader/Loader';
import ErrorPage from '../static/error-status/ErrorPage';

interface TwoFaProps {
    isEnable: boolean;
}

const Settings = () => {
    const [globalMessage, setGlobalMessage] = useState('');
    const [status, setStatus] = useState(500)
    const [user, setUser] = useState<UserProps | null>();
    const [lastUpdate, setLastUpdate] = useState(Date.now())

    const [message, setMessage] = useState('');
    const [icon, setIcon] = useState('')

    async function fetchUser() {
        const response = await api.getUser(0, true);
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

                const response = await api.updateUsername(username);
                setMessage(response.message);
                setIcon(response.success ? 'done' : 'error');

                if (response.success) {
                    setLastUpdate(Date.now());
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
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

                const response = await api.updatePassword(oldPassword, newPassword);
                setMessage(response.message);
                setIcon(response.success ? 'done' : 'error');

                if (response.success) {
                    setLastUpdate(Date.now());
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
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

                const response = await api.veridyPasswordAndSendCode(password);
                if (response.success) {
                    setStatus(true);
                } else {
                    setMessage(response.message);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
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

        const Confirm = () => {
            const [successStatus, setStatus] = useState(false);

            const [email, setEmail] = useState('');

            const [firstCode, setFirstCode] = useState<number>();

            const handleVerifyCodeAndSendCode = async (e: FormEvent) => {
                e.preventDefault();

                const response = await api.verifyCodeAndSendCode(firstCode!, email);

                if (response.success) {
                    setStatus(true);
                } else {
                    setMessage(response.message);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
                }, 3000)
            }

            const handleVerifyCodeAndUpdate = async (e: FormEvent, code: number) => {
                e.preventDefault();

                const response = await api.verifyCodeAndUpdate(code);

                setMessage(response.message);
                setIcon(response.success ? 'done' : 'error')

                if (response.success) {
                    setLastUpdate(Date.now());
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
                }, 3000)
            }

            return (
                <div>
                    <div className="email-and-code">
                        <form onSubmit={handleVerifyCodeAndSendCode}>
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
                                    value={firstCode}
                                    onChange={(e) => {
                                        const value = e.target.value;
                                        if (value === '') {
                                            setFirstCode(undefined);
                                        } else {
                                            const parsedValue = parseInt(value, 10);
                                            if (!isNaN(parsedValue)) {
                                                setFirstCode(parsedValue);
                                            }
                                        }
                                    }}
                                    inputMode="numeric"
                                />
                            </label>
                            <button type="submit">Confirm</button>
                        </form>
                    </div>
                    <Modal isActive={successStatus} setActive={setStatus}>
                        <Verify apiCall={handleVerifyCodeAndUpdate} />
                    </Modal>
                </div>
            );
        }
    // #endregion

    // #region 2FA Component

        const TwoFA = ({ isEnable }: TwoFaProps) => {
            const [password, setPassword] = useState('');
            const [successStatus, setStatus] = useState(false);
            const [visibleForm, setFormVisible] = useState(false);
            const [twoFa, setTwoFa] = useState<boolean>();

            const handleSubmitConfirmPassword = async (e: FormEvent) => {
                e.preventDefault();

                const response = await api.twoFaPasswordConfirm(password);

                if (response.success) {
                    setStatus(true);
                } else {
                    setMessage(response.message);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
                }, 3000)
            }

            const handleSubmitEmailVerify = async (e: FormEvent, code: number) => {
                e.preventDefault();

                const response = await api.twoFaEmailVerify(code, twoFa!);

                if (response.success) {
                    setStatus(true);
                    setLastUpdate(Date.now());
                } else {
                    setMessage(response.message);
                    setIcon('error');
                }

                setTimeout(() => {
                    setMessage('');
                    setIcon('');
                }, 3000)
            }

            const setState = (formVisible: boolean, twoFaState: boolean) => {
                setFormVisible(formVisible);
                setTwoFa(twoFaState);
            }

            return (
                <div>
                    <button onClick={() => setState(true, !isEnable)}>
                        {isEnable ? "Disable 2FA" : "Enable 2FA"}
                    </button>
                    {visibleForm && (
                        <form onSubmit={handleSubmitConfirmPassword}>
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
                        <Verify apiCall={handleSubmitEmailVerify} />
                    </Modal>
                </div>
            );
        }

    // #endregion

    interface VerifyProps {
        apiCall: (e: FormEvent, code: number) => Promise<void>;
    }

    const Verify = ({ apiCall }: VerifyProps) => {
        const [code, setCode] = useState<number>();

        return (
            <div>
                <p>Verify Action</p>
                <form onSubmit={(e) => apiCall(e, code!)}>
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
            </div>
        )
    }

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