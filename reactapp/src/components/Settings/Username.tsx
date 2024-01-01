import React, { FormEvent, useState } from 'react';
import Input from '../Input/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';

const Username = () => {

    const [username, setUsername] = useState('');

    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/username?username=${username}`, method: 'PUT', withCookie: true, requestBody: { username: username } })

        if (response.isSuccess) {
            setErrorMessage('');
            setSuccessMessage(response.data.message);
        }
        else {
            setSuccessMessage('');
            setErrorMessage(response.data);
        }
    }

    return (
        <div className="username">
            <form onSubmit={handleSubmit}>
                <Input type="text" id="username" require={true} value={username} onChange={(e) => setUsername(e.target.value)} />
                <button type="submit" className="btn btn-primary btn-disabled">
                    Update username
                </button>
            </form>
            {successMessage && <Message message={successMessage} font='done' />}
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
}

export default Username;