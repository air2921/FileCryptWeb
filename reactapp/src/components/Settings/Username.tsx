import React, { FormEvent, useState } from 'react';
import Input from '../Input/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';

function Username() {

    const [username, setUsername] = useState('');

    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/username/new`, method: 'PUT', withCookie: true, requestBody: { username: username } })

        if (response.isSuccess) {
            setSuccessMessage(response.data.message);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    return (
        <div className="username">
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="email">email</label>
                    <Input type="text" id="username" value={username} onChange={(e) => setUsername(e.target.value)} />
                </div>
                <button type="submit" className="btn btn-primary btn-disabled">
                    Sign In
                </button>
            </form>
            <Message message={successMessage} font='done' />
            <Message message={errorMessage} font='error' />
        </div>
    );
}

export default Username;