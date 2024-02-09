import React, { useEffect, useState } from 'react';
import ApiList from '../../../components/List/ApiList/ApiList';
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';

const Api = () => {
    const [errorMessage, setErrorMessage] = useState('');
    const [apiList, setApis] = useState(null);
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [apiType, setApiType] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/api/all', method: 'GET', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setApis(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const createApi = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/api/${apiType}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setLastTimeModified(Date.now());
            setApiType('');
        }
        else {
            setApiType('');
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    const deleteApi = async (apiId: number) => {
        const response = await AxiosRequest({ endpoint: `api/core/api/revoke/${apiId}`, method: 'DELETE', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setLastTimeModified(Date.now());
        }
        else {
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    useEffect(() => {
        fetchData();
    }, [lastTimeModified]);

    if (!apiList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { api } = apiList as { api: any[] }

    return (
        <>
            <div className="create-api">
            <p>Select API type</p>
                <select
                    className="set-api-type"
                    id="type"
                    required={true}
                    value={apiType}
                    onChange={(e) => setApiType(e.target.value)}>

                    <option value="Classic">Classic</option>
                    <option value="Development">Development</option>
                    <option value="Production">Production</option>
                </select>
                <button onClick={createApi}>Submit</button>
            </div>
            {message && font && < Message message={message} font={font} />}
            <ApiList apis={api} deleteApi={deleteApi} />
        </>
    );
}

export default Api;