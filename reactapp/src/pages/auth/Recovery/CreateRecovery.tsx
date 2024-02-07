import React, { FormEvent, useState } from 'react';
import Message from '../../../components/Message/Message';
import AxiosRequest from '../../../api/AxiosRequest';

const CreateRecovery = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/auth/recovery/unique/token?email=${email}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setMessage(response.data.message);
            setFont('check_small')
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    };

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <label htmlFor="email">
                    {email ? "Email:" : "Email*"}
                    <input
                        type="email"
                        id="email"
                        required={true}
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </label>
                <button type="submit">Send recovery link</button>
            </form>
            {message && <Message message={message} font={font} />}
        </div>
    );
}

export default CreateRecovery;