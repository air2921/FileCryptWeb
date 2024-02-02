import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import AxiosRequest from '../../../api/AxiosRequest';
import UserData from '../../../components/User/UserData';
import Username from '../../../components/Settings/Username';
import Password from '../../../components/Settings/Password';
import Email from '../../../components/Settings/Email';
import TwoFA from '../../../components/Settings/TwoFA';
import Button from '../../../components/Helpers/Button';
import Message from '../../../components/Message/Message';
import Font from '../../../components/Font/Font';
import CheckBox from '../../../components/Helpers/CheckBox';
import Input from '../../../components/Helpers/Input';
import Modal from '../../../components/Modal/Modal';
import UserKeys from '../../../components/User/UserKeys';

const Settings = () => {
    const [errorMessage, setErrorMessage] = useState('');

    const [userKeys, setKeys] = useState(null);
    const [userData, setUserData] = useState(null);

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
        }
        else {
            setErrorMessage(response.data);
        }
    }

    useEffect(() => {
        fetchDataUser();
        fetchDataKeys();
    }, []);

    if (!userKeys || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { keys } = userKeys as { keys: any };
    const { user } = userData as { user: any };

    const KeyTypes = () => {
        const [activeModal, setActive] = useState(false);

        return (
            <>
                <Button onClick={() => setActive(true)}>Encryption Key Types</Button>
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

    const KeyChange = () => {
        const [keyMessage, setKeyMessage] = useState('');
        const [keyFont, setKeyFont] = useState('');
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
                setKeyMessage(response.data.message);
                setKeyFont('done');
            }
            else {
                setKeyMessage(response.data);
                setKeyFont('error');
            }

            setTimeout(() => {
                setKeyMessage('');
                setKeyFont('');
            }, 5000)
        }

        const handleReceivedKeySubmit = async () => {
            const response = await AxiosRequest({ endpoint: 'api/core/keys/received/clean', method: 'PUT', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setKeyMessage(response.data.message);
                setKeyFont('done');
            }
            else {
                setKeyMessage(response.data);
                setKeyFont('error');
            }

            setTimeout(() => {
                setKeyMessage('');
                setKeyFont('');
            }, 5000)
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
                            <Input text='Set your new private key' type="text" id="private" require={false} value={privateKey} onChange={(e) => setPrivateKey(e.target.value)} />
                        )}
                        {!privateKey && (
                            <CheckBox text='Auto-generation key' type="checkbox" id="auto-private" checked={isAutoPrivate} onChange={handlePrivateCheckboxChange} />
                        )}
                        <Button> <Font font={'refresh'} /> </Button>
                    </form>
                </div>
                <div className="internal">
                    <form onSubmit={handleInternalKeySubmit}>
                        {!isAutoInternal && (
                            <Input text='Set your new internal key' type="text" id="internal" require={false} value={internalKey} onChange={(e) => setInternalKey(e.target.value)} />
                        )}
                        {!internalKey && (
                            <CheckBox text='Auto-generation key' type="checkbox" id="auto-internal" checked={isAutoInternal} onChange={handleInternalCheckboxChange} />
                        )}
                        <Button> <Font font={'refresh'} /> </Button>
                    </form>
                </div>
                <div className="received">
                    <p>Delete Received Key</p>
                    <Button onClick={handleReceivedKeySubmit}> <Font font={'delete'} /> </Button>
                </div>
                <Message message={keyMessage} font={keyFont} />
            </div>
        );
    }

    return (
        <div className="container">
            <div className="data-container">
                <UserData user={user} isOwner={true} />
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
        </div>
    );
}

export default Settings;