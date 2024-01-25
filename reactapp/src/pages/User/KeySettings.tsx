import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import UserKeys from '../../components/User/UserKeys';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../../components/Message/Message';
import Input from '../../components/Helpers/Input';
import CheckBox from '../../components/Helpers/CheckBox';
import Button from '../../components/Helpers/Button';
import Font from '../../components/Font/Font';
import Modal from '../../components/Modal/Modal';

const KeySettings = () => {
    const [userKeys, setKeys] = useState(null);
    const [errorMessage, setErrorMessage] = useState('');
    const [keyMessage, setKeyMessage] = useState('');
    const [keyFont, setKeyFont] = useState('');
    const [privateKey, setPrivateKey] = useState('');
    const [internalKey, setInternalKey] = useState('');
    const [isAutoPrivate, setIsAutoPrivate] = useState(false);
    const [isAutoInternal, setIsAutoInternal] = useState(false);
    const [activeModal, setActive] = useState(false);

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/keys/all', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setKeys(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const handlePrivateCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        setIsAutoPrivate(e.target.checked);
    };

    const handleInternalCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        setIsAutoInternal(e.target.checked);
    };


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

    useEffect(() => {
        fetchData();
    }, []);

    if (!userKeys) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { keys } = userKeys as { keys: any };

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

    return (
        <div className="container">
            <UserKeys keys={keys} />
            <Button onClick={() => setActive(true)}>Encryption Key Types</Button>
            <Modal isActive={activeModal} setActive={setActive}>
                <h2>What is encryption key types ?</h2>

                <h3>Private Key:</h3>
                <div>The private key is exclusively available to the account owner.</div>
                <div>Its value cannot be transferred or disclosed to other users.</div>
                <div>The owner can modify the key's value using the Base64 format or delegate the generation of a new key to an automatic process.</div>
                <div>The private key cannot be deleted; it must always have some value.</div>

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
            <div className="keys">
                <div className="private">
                    <form onSubmit={handlePrivateKeySubmit}>
                        {!isAutoPrivate && (
                            <Input text='Set your new private key' type="text" id="private" require={false} value={privateKey} onChange={(e) => setPrivateKey(e.target.value)}/>
                        )}
                        {!privateKey && (
                            <CheckBox text='Auto-generation key' type="checkbox" id="auto-private" checked={isAutoPrivate} onChange={handlePrivateCheckboxChange} />
                        )}
                        <Button>
                            <Font font={'refresh'} />
                        </Button>
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
                        <Button>
                            <Font font={'refresh'} />
                        </Button>
                    </form>
                </div>
                <div className="received">
                    <p>Delete Received Key</p>
                    <Button onClick={handleReceivedKeySubmit}>
                        <Font font={'delete'} />
                    </Button>
                </div>
            </div>
            <Message message={keyMessage} font={keyFont} />
        </div>
    );
}

export default KeySettings;