import React, { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import UserKeys from '../components/User/UserKeys';
import AxiosRequest from '../api/AxiosRequest';
import Message from '../components/Message/Message';
import Input from '../components/Input/Input';
import CheckBox from '../components/Input/CheckBox';

const KeySettings = () => {
    const [userKeys, setKeys] = useState(null);
    const [successStatus, setStatus] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');

    const [privateErrorMessage, setPrivateErrorMessage] = useState('');
    const [privateSuccessMessage, setPrivateSuccessMessage] = useState('');
    const [internalErrorMessage, setInternalErrorMessage] = useState('');
    const [internalSuccessMessage, setInternalSuccessMessage] = useState('');

    const [privateKey, setPrivateKey] = useState('');
    const [internalKey, setInternalKey] = useState('');
    const [isAutoPrivate, setIsAutoPrivate] = useState(false);
    const [isAutoInternal, setIsAutoInternal] = useState(false);

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/keys/all', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setKeys(response.data);
            setStatus(true);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    useEffect(() => {
        fetchData();
    }, []);

    if (!successStatus || !userKeys) {
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
                setPrivateErrorMessage('');
                setPrivateSuccessMessage(response.data.message);
            }
            else {
                setPrivateSuccessMessage('');
                setPrivateErrorMessage(response.data);
            }
        }
        else if (keyType.toLowerCase() === 'internal') {
            if (response.isSuccess) {
                setInternalErrorMessage('');
                setInternalSuccessMessage(response.data.message);
            }
            else {
                setInternalSuccessMessage('');
                setInternalErrorMessage(response.data);
            }
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
                        <Input type="text" id="private" require={false} value={privateKey} onChange={(e) => setPrivateKey(e.target.value)} />
                        <CheckBox type="checkbox" id="auto-private" checked={isAutoPrivate} onChange={handlePrivateCheckboxChange} />
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Update private key
                        </button>
                    </form>
                    {privateSuccessMessage && <Message message={privateSuccessMessage} font='done' />}
                    {privateErrorMessage && <Message message={privateErrorMessage} font='error' />}
                </div>
                <div className="internal">
                    <form onSubmit={handleInternalKeySubmit}>
                        <Input type="text" id="internal" require={false} value={internalKey} onChange={(e) => setInternalKey(e.target.value)} />
                        <CheckBox type="checkbox" id="auto-internal" checked={isAutoInternal} onChange={handleInternalCheckboxChange} />
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Update internal key
                        </button>
                    </form>
                    {internalSuccessMessage && <Message message={internalSuccessMessage} font='done' />}
                    {internalErrorMessage && <Message message={internalErrorMessage} font='error' />}
                </div>
            </div>
        </div>
    );
}

export default KeySettings;