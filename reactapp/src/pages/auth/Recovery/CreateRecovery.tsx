import React, { FormEvent, useState } from 'react';
import Input from '../../../components/Helpers/Input';
import Button from '../../../components/Helpers/Button';
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
                <Input text='Enter your login' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                <Button>Send recovery link</Button>
            </form>
            {message && <Message message={message} font={font} />}
        </div>
    );
}

export default CreateRecovery;