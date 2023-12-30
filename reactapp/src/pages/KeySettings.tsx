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
            setErrorMessage(errorMessage);
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


    const handleSubmitPrivate = async (e: FormEvent) => {
        if (isAutoPrivate) {
            const response = await AxiosRequest({ endpoint: `api/core/keys/private?auto=${isAutoPrivate}`, method: 'PUT', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setPrivateSuccessMessage(response.data.message);
            }
            else {
                setPrivateErrorMessage(response.data);
            }
        }
        else {
            const response = await AxiosRequest({
                endpoint: `api/core/keys/private?auto=${isAutoPrivate}`, method: 'PUT', withCookie: true, requestBody: { private_key: privateKey }
            });

            if (response.isSuccess) {
                setPrivateSuccessMessage(response.data.message);
            }
            else {
                setPrivateErrorMessage(response.data);
            }
        }
    }

    const handleSubmitInternal = async (e: FormEvent) => {
        if (isAutoInternal) {
            const response = await AxiosRequest({ endpoint: `api/core/keys/internal?auto=${isAutoInternal}`, method: 'PUT', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setInternalSuccessMessage(response.data.message);
            }
            else {
                setInternalErrorMessage(response.data);
            }
        }
        else {
            const response = await AxiosRequest({
                endpoint: `api/core/keys/internal?auto=${isAutoInternal}`, method: 'PUT', withCookie: true, requestBody: { internal_key: internalKey }
            });

            if (response.isSuccess) {
                setInternalSuccessMessage(response.data.message);
            }
            else {
                setInternalErrorMessage(response.data);
            }
        }
    }

    return (
        <div>
            <UserKeys keys={keys} />
            <div className="keys">
                <div className="private">
                    <form onSubmit={handleSubmitPrivate}>
                        <Input type="text" id="private" value={privateKey} onChange={(e) => setPrivateKey(e.target.value)} />
                        <CheckBox type="checkbox" id="auto-private" checked={isAutoPrivate} onChange={handlePrivateCheckboxChange} />
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Change private key
                        </button>
                    </form>
                    {privateSuccessMessage && <Message message={privateSuccessMessage} font='done' />}
                    {privateErrorMessage && <Message message={privateErrorMessage} font='error' />}
                </div>
                <div className="internal">
                    <form onSubmit={handleSubmitInternal}>
                        <Input type="text" id="internal" value={internalKey} onChange={(e) => setInternalKey(e.target.value)} />
                        <CheckBox type="checkbox" id="auto-internal" checked={isAutoInternal} onChange={handleInternalCheckboxChange} />
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Change private key
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