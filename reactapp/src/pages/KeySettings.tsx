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

    const [updatePrivateErrorMessage, setUpdatePrivateErrorMessage] = useState('');
    const [updatePrivateSuccessMessage, setUpdatePrivateSuccessMessage] = useState('');
    const [updateInternalErrorMessage, setUpdateInternalErrorMessage] = useState('');
    const [updateInternalSuccessMessage, setUpdateInternalSuccessMessage] = useState('');

    const [privateKey, setPrivateKey] = useState('');
    const [internalKey, setInternalKey] = useState('');
    const [isPrivateChecked, setIsPrivateChecked] = useState(false);
    const [isInternalChecked, setIsInternalChecked] = useState(false);

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
        setIsPrivateChecked(e.target.checked);
    };

    const handleInternalCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
        setIsInternalChecked(e.target.checked);
    };


    const handleSubmitPrivate = async (e: FormEvent) => {
        if (isPrivateChecked) {
            const response = await AxiosRequest({ endpoint: `api/core/keys/private?auto=${isPrivateChecked}`, method: 'PUT', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setUpdatePrivateSuccessMessage(response.data.message);
            }
            else {
                setUpdatePrivateErrorMessage(response.data);
            }
        }
        else {
            const response = await AxiosRequest({
                endpoint: `api/core/keys/private?auto=${isPrivateChecked}`, method: 'PUT', withCookie: true, requestBody: { private_key: { privateKey } }});

            if (response.isSuccess) {
                setUpdatePrivateSuccessMessage(response.data.message);
            }
            else {
                setUpdatePrivateErrorMessage(response.data);
            }
        }
    }

    const handleSubmitInternal = async (e: FormEvent) => {
        if (isInternalChecked) {
            const response = await AxiosRequest({ endpoint: `api/core/keys/internal?auto=${isInternalChecked}`, method: 'PUT', withCookie: true, requestBody: null });

            if (response.isSuccess) {
                setUpdateInternalSuccessMessage(response.data.message);
            }
            else {
                setUpdateInternalErrorMessage(response.data);
            }
        }
        else {
            const response = await AxiosRequest({
                endpoint: `api/core/keys/internal?auto=${isInternalChecked}`, method: 'PUT', withCookie: true, requestBody: { person_internal_key: { internalKey } }});

            if (response.isSuccess) {
                setUpdateInternalSuccessMessage(response.data.message);
            }
            else {
                setUpdateInternalErrorMessage(response.data);
            }
        }
    }

    return (
        <div>
            <UserKeys keys={keys} isOwner={true} />
            <div className="keys">
                <div className="private">
                    <form onSubmit={handleSubmitPrivate}>
                        <div className="form-group">
                            <label htmlFor="private">Set your new private key</label>
                            <Input type="text" id="private" value={privateKey} onChange={(e) => setPrivateKey(e.target.value)} />
                            <CheckBox type="checkbox" id="auto-private" checked={isPrivateChecked} onChange={handlePrivateCheckboxChange} />
                        </div>
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Change private key
                        </button>
                    </form>
                    {updatePrivateSuccessMessage && <Message message={updatePrivateSuccessMessage} font='done' />}
                    {updatePrivateErrorMessage && <Message message={updatePrivateErrorMessage} font='error' />}
                </div>
                <div className="internal">
                    <form onSubmit={handleSubmitInternal}>
                        <div className="form-group">
                            <label htmlFor="internal">Set your new internal key</label>
                            <Input type="text" id="internal" value={internalKey} onChange={(e) => setInternalKey(e.target.value)} />
                            <CheckBox type="checkbox" id="auto-internal" checked={isInternalChecked} onChange={handleInternalCheckboxChange} />
                        </div>
                        <button type="submit" className="btn btn-primary btn-disabled">
                            Change private key
                        </button>
                    </form>
                    {updateInternalSuccessMessage && <Message message={updateInternalSuccessMessage} font='done' />}
                    {updateInternalErrorMessage && <Message message={updateInternalErrorMessage} font='error' />}
                </div>
            </div>
        </div>
    );
}

export default KeySettings;