import React, { FormEvent, useState } from 'react';
import Input from '../Helpers/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';

const Username = () => {

    const [username, setUsername] = useState('');

    const [message, setMessage] = useState('');
    const [font, setFont] = useState('')

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/username?username=${username}`, method: 'PUT', withCookie: true, requestBody: { username: username } })

        if (response.isSuccess) {
            setMessage(response.data.message);
            setFont('done')
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    }

    return (
        <div className="username">
            <form onSubmit={handleSubmit}>
                <Input text='Your new username' type="text" id="username" require={true} value={username} onChange={(e) => setUsername(e.target.value)} />
                <button type="submit" className="btn btn-primary btn-disabled">
                    Save username
                </button>
            </form>
            {message && <Message message={message} font={font} />}
        </div>
    );
}

export default Username;