﻿import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import UserKeys from '../components/User/UserKeys';
import AxiosRequest from '../api/AxiosRequest';
import Message from '../components/Message/Message';
import Input from '../components/Helpers/Input';
import CheckBox from '../components/Helpers/CheckBox';
import Button from '../components/Helpers/Button';

const KeySettings = () => {
    const [userKeys, setKeys] = useState(null);
    const [errorMessage, setErrorMessage] = useState('');

    const [privateMessage, setPrivateMessage] = useState('');
    const [internalMessage, setInternalMessage] = useState('');
    const [privateFont, setPrivateFont] = useState('');
    const [internalFont, setInternalFont] = useState('');
    const [receivedMessage, setReceivedMessage] = useState('');
    const [receivedFont, setReceivedFont] = useState('');

    const [privateKey, setPrivateKey] = useState('');
    const [internalKey, setInternalKey] = useState('');
    const [isAutoPrivate, setIsAutoPrivate] = useState(false);
    const [isAutoInternal, setIsAutoInternal] = useState(false);

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/keys/all', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setKeys(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    useEffect(() => {
        fetchData();
    }, []);

    if (!userKeys) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { keys } = userKeys as { keys: any };

    const handlePrivateCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        setIsAutoPrivate(e.target.checked);
    };

    const handleInternalCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        setIsAutoInternal(e.target.checked);
    };


    const handleSubmit = async (e: FormEvent, keyType: string, isAuto: boolean, keyValue: string) => {
        e.preventDefault();

        const body = isAuto ? null : { [`${keyType.toLowerCase()}_key`]: keyValue };

        const response = await AxiosRequest({
            endpoint: `api/core/keys/${keyType.toLowerCase()}?auto=${isAuto}`,
            method: 'PUT',
            withCookie: true,
            requestBody: body
        });

        if (keyType.toLowerCase() === 'private') {
            if (response.isSuccess) {
                setPrivateMessage(response.data.message);
                setPrivateFont('done');
            }
            else {
                setPrivateMessage(response.data);
                setPrivateFont('error');
            }
        }
        else if (keyType.toLowerCase() === 'internal') {
            if (response.isSuccess) {
                setInternalMessage(response.data.message);
                setInternalFont('done');
            }
            else {
                setInternalMessage(response.data);
                setInternalFont('error');
            }
        }
    }

    const handleReceivedKeySubmit = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/keys/received/clean', method: 'PUT', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setReceivedMessage(response.data.message);
            setReceivedFont('done');
        }
        else {
            setReceivedMessage(response.data);
            setReceivedFont('error');
        }
    }

    const handlePrivateKeySubmit = (e: FormEvent) => {
        handleSubmit(e, 'private', isAutoPrivate, privateKey);
    };

    const handleInternalKeySubmit = (e: FormEvent) => {
        handleSubmit(e, 'internal', isAutoInternal, internalKey);
    };

    return (
        <div>
            <UserKeys keys={keys} />
            <div className="keys">
                <div className="private">
                    <form onSubmit={handlePrivateKeySubmit}>
                        {!isAutoPrivate && (
                            <Input text='Set your new private key' type="text" id="private" require={false} value={privateKey} onChange={(e) => setPrivateKey(e.target.value)}/>
                        )}
                        {!privateKey && (
                            <CheckBox text='Auto-generation key' type="checkbox" id="auto-private" checked={isAutoPrivate} onChange={handlePrivateCheckboxChange} />
                        )}
                        <Button>Update Private Key</Button>
                    </form>
                    {privateMessage && <Message message={privateMessage} font={privateFont} />}
                </div>
                <div className="internal">
                    <form onSubmit={handleInternalKeySubmit}>
                        {!isAutoInternal && (
                            <Input text='Set your new internal key' type="text" id="internal" require={false} value={internalKey} onChange={(e) => setInternalKey(e.target.value)} />
                        )}
                        {!internalKey && (
                            <CheckBox text='Auto-generation key' type="checkbox" id="auto-internal" checked={isAutoInternal} onChange={handleInternalCheckboxChange} />
                        )}
                        <Button>Update Internal Key</Button>
                    </form>
                    {internalMessage && <Message message={internalMessage} font={internalFont} />}
                </div>
                <div className="received">
                    <p>Update Received Key to null</p>
                    <Button onClick={handleReceivedKeySubmit}>Update</Button>
                    {receivedMessage && <Message message={receivedMessage} font={receivedFont} />}
                </div>
            </div>
        </div>
    );
}

export default KeySettings;