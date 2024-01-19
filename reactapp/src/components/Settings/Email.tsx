import React, { FormEvent, useState } from 'react';
import Message from '../Message/Message';
import Input from '../Helpers/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Verify from '../Verify/Verify';
import Button from '../Helpers/Button';

const Email = () => {

    const [errorMessage, setErrorMessage] = useState('');
    const [successStatus, setStatus] = useState(false);

    const [password, setPassword] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/email/start?password=${password}`, method: 'POST', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setStatus(true);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    return (
        <div>
            {successStatus ? (
                <Confirm />
            ) : (
                    <div className="email">
                        <form onSubmit={handleSubmit}>
                            <Input text='Confirm password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                            <Button>Confirm</Button>
                        </form>
                        {errorMessage && <Message message={errorMessage} font='error' />}
                    </div>
                )}
        </div>
    );
}

const Confirm = () => {

    const [errorMessage, setErrorMessage] = useState('');
    const [successStatus, setStatus] = useState(false);

    const [email, setEmail] = useState('');
    const [code, setCode] = useState(0);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/email/confirm/old?email=${email}&code=${code}`, method: 'POST', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setStatus(true);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    return (
        <div>
            {successStatus ? (
                <Verify endpoint='api/account/edit/email/confirm/new' method='PUT' />
            ) : (
                    <div className="email-and-code">
                        <form onSubmit={handleSubmit}>
                            <Input text='Your new email' type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                            <Input text='Confirmation code' type="number" id="code" require={true} value={code} onChange={(e) => setCode(parseInt(e.target.value, 10))} />
                            <Button>Confirm</Button>
                        </form>
                        {errorMessage && <Message message={errorMessage} font='error' />}
                    </div>
            )}
        </div>
    );
}

export default Email;