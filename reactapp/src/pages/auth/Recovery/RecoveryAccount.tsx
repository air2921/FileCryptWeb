import React, { FormEvent, useState } from 'react';
import Message from '../../../components/Message/Message';
import { useLocation, useNavigate } from 'react-router-dom';
import AxiosRequest from '../../../api/AxiosRequest';

const RecoveryAccount = () => {
    const location = useLocation();
    const searchParams = new URLSearchParams(location.search);
    const token = searchParams.get('token');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/auth/recovery/account?password=${password}&token=${token}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            navigate('/auth/login');
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    };

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <label htmlFor="password">
                    {password ? "Password:" : "Password*"}
                    <input
                        type="password"
                        id="password"
                        required={true}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </label>
                <button type="submit">Submit</button>
            </form>
            {message && <Message message={message} font={font} />}
        </div>
    );
}

export default RecoveryAccount;