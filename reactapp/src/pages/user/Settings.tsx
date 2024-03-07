import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import Font from '../../utils/helpers/icon/Font';
import Modal from '../../components/Modal/Modal';
import UserKeys from '../../components/User/UserKeys';
import Message from '../../utils/helpers/message/Message';
import AxiosRequest from '../../utils/api/AxiosRequest';
import UserData from '../../components/User/UserData';

interface TwoFaProps {
    is_enabled_2fa: boolean
}

interface VerifyProps {
    endpoint: string;
    method: string;
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
    const [errorMessage, setErrorMessage] = useState('');
    const [userKeys, setKeys] = useState(null);
    const [userData, setUserData] = useState(null);

    const [message, setMessage] = useState('');
    const [icon, setIcon] = useState('')

    // #region Functions for get default user data
        const fetchDataUser = async () => {
            const response = await AxiosRequest({ endpoint: `api/core/users/data/only`, method: 'GET', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setUserData(response.data);
            }
            else {
                setErrorMessage(response.data);
            }
        }

        const fetchDataKeys = async () => {
            const response = await AxiosRequest({ endpoint: 'api/core/keys/all', method: 'GET', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setKeys(response.data);
                console.log(response.data);
            }
            else {
                setErrorMessage(response.data);
            }
        }
    // #endregion

    useEffect(() => {
        fetchDataUser();
        fetchDataKeys();
    }, []);

    if (!userKeys || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { keys } = userKeys as { keys: any };
    const { user } = userData as { user: any };

    // #region Other components

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
            const TwoFA = ({ is_enabled_2fa }: TwoFaProps) => {
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
                        {is_enabled_2fa && !visibleForm && <button onClick={() => set2FaStatus(false, true)}>Disable 2FA</button>}
                        {!is_enabled_2fa && !visibleForm && <button onClick={() => set2FaStatus(true, true)}>Enable 2FA</button>}
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

        // #region Encryption Keys Change Component "KeyChange"
            const KeyChange = () => {
                const [privateKey, setPrivateKey] = useState('');
                const [internalKey, setInternalKey] = useState('');
                const [isAutoPrivate, setIsAutoPrivate] = useState(false);
                const [isAutoInternal, setIsAutoInternal] = useState(false);

                const handleSubmit = async (e: FormEvent, keyType: string, isAuto: boolean, keyValue: string) => {
                    e.preventDefault();

                    const response = await AxiosRequest({
                        endpoint: `api/core/keys/${keyType.toLowerCase()}?key=${isAuto ? null : keyValue}&auto=${isAuto}`,
                        method: 'PUT',
                        withCookie: true,
                        requestBody: null
                    });

                    if (response.isSuccess) {
                        setMessage(response.data.message);
                        setIcon('done');
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

                const handleReceivedKeySubmit = async () => {
                    const response = await AxiosRequest({ endpoint: 'api/core/keys/received/clean', method: 'PUT', withCookie: true, requestBody: null });

                    if (response.isSuccess) {
                        setMessage(response.data.message);
                        setIcon('done');
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

                const handlePrivateKeySubmit = (e: FormEvent) => {
                    handleSubmit(e, 'private', isAutoPrivate, privateKey);
                };

                const handleInternalKeySubmit = (e: FormEvent) => {
                    handleSubmit(e, 'internal', isAutoInternal, internalKey);
                };

                const handlePrivateCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
                    setIsAutoPrivate(e.target.checked);
                };

                const handleInternalCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
                    setIsAutoInternal(e.target.checked);
                };

                return (
                    <div className="change-keys-container">
                        <div className="private">
                            <form onSubmit={handlePrivateKeySubmit}>
                                {!isAutoPrivate && (
                                    <label htmlFor="private">
                                        Set your new private key
                                        <input
                                            type="text"
                                            id="private"
                                            required={false}
                                            value={privateKey}
                                            onChange={(e) => setPrivateKey(e.target.value)}
                                        />
                                    </label>
                                )}
                                {!privateKey && (
                                    <label htmlFor="auto-private">
                                        Auto-generation key
                                        <input
                                            type="checkbox"
                                            id="auto-private"
                                            checked={isAutoPrivate}
                                            onChange={handlePrivateCheckboxChange}
                                        />
                                    </label>
                                )}
                                <button type="submit"><Font font={'refresh'} /></button>
                            </form>
                        </div>
                        <div className="internal">
                            <form onSubmit={handleInternalKeySubmit}>
                                {!isAutoInternal && (
                                    <label htmlFor="internal">
                                        Set your new internal key
                                        <input
                                            type="text"
                                            id="internal"
                                            required={false}
                                            value={internalKey}
                                            onChange={(e) => setInternalKey(e.target.value)}
                                        />
                                    </label>
                                )}
                                {!internalKey && (
                                    <label htmlFor="auto-internal">Auto-generation key
                                        <input
                                            type="checkbox"
                                            id="auto-internal"
                                            checked={isAutoInternal}
                                            onChange={handleInternalCheckboxChange}
                                        />
                                    </label>
                                )}
                                <button type="submit"><Font font={'refresh'} /></button>
                            </form>
                        </div>
                        <div className="received">
                            <p>Delete Received Key</p>
                            <button onClick={handleReceivedKeySubmit}><Font font={'delete'} /></button>
                        </div>
                    </div>
                );
            }
        // #endregion

        // #region Modal Window Content "KeyTypes"
            const KeyTypes = () => {
                const [activeModal, setActive] = useState(false);

                return (
                    <>
                        <button onClick={() => setActive(true)}>Encryption Key Types</button>
                        <Modal isActive={activeModal} setActive={setActive}>
                            <h2>What is encryption key types ?</h2>

                            <h3>Private Key:</h3>
                            <div>The private key is exclusively available to the account owner.</div>
                            <div>Its value cannot be transferred or disclosed to other users.</div>
                            <div>The owner can modify the key's value using the Base64 format or delegate the generation of a new key to an automatic process.</div>
                            <div>The private key cannot be deleted it must always have some value.</div>

                            <h3>Internal Key:</h3>
                            <div>The internal key can be used to create an offer for exchanging encryption keys withother users.</div>
                            <div> Its value can be changed, and it can be deleted, with the deleted keyhaving a null value.</div>
                            <div>It is important to note that after transferring the key to another user and making changes, the recipient retains the original key</div>
                            <div>It is not automatically updated or deleted.</div>

                            <h3>Received Key:</h3>
                            <div>The received key contains information that is not viewable or editable.</div>
                            <div>Its value is assigned from an offer made by another user.</div>
                            <div>The received key can be removed from the list, but it cannot be transferred or modified.</div>
                            <div>This key is solely used for encrypting and decrypting data provided by another user.</div>
                        </Modal>
                    </>
                );
            }
        // #endregion

    // #endregion

    return (
        <div className="container">
            <div className="data-container">
                <UserData user={user} isOwner={true} showButton={false} />
                <UserKeys keys={keys} />
            </div>
            <div className="change-data-container">
                <Username />
                <Password />
                <TwoFA is_enabled_2fa={user.is_2fa_enabled} />
                <Email />
            </div>
            <div className="keys-container">
                <KeyTypes />
                <KeyChange />
            </div>
            {message && <Message message={message} font={icon} />}
        </div>
    );
}

export default Settings;