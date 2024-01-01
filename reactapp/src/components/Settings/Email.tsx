import React, { FormEvent, useState } from 'react';
import Message from '../Message/Message';
import Input from '../Input/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Verify from '../Verify/Verify';

const Email = () => {

    const [errorMessage, setErrorMessage] = useState('');
    const [successStatus, setStatus] = useState(false);

    const [password, setPassword] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: 'api/account/edit/email/start', method: 'POST', withCookie: true, requestBody: { password_hash: password } })

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
                            <Input type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                            <button type="submit" className="btn btn-primary btn-disabled">
                                Confirm
                            </button>
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

        const response = await AxiosRequest({ endpoint: `api/account/edit/email/confirm/old?code=${code}`, method: 'POST', withCookie: true, requestBody: { email: email } })

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
                            <Input type="email" id="email" require={true} value={email} onChange={(e) => setEmail(e.target.value)} />
                            <Input type="number" id="code" require={true} value={code} onChange={(e) => setCode(parseInt(e.target.value, 10))} />
                            <button type="submit" className="btn btn-primary btn-disabled">
                                Confirm
                            </button>
                        </form>
                        {errorMessage && <Message message={errorMessage} font='error' />}
                    </div>
            )}
        </div>
    );
}

export default Email;