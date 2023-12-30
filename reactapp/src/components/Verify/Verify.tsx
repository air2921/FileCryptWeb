import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';
import Input from '../Input/Input';

const Verify: React.FC<VerifyProps> = ({ endpoint, method }) => {
    const [code, setCode] = useState(0);
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `${endpoint}?code=${code}`, method: method, withCookie: true, requestBody: null });

        if (response.isSuccess) {
            navigate('/');
        }
        else {
            setErrorMessage(response.data);
        }
    };

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <Input type="number" id="code" require={true} value={code} onChange={(e) => setCode(parseInt(e.target.value, 10))} />
                <button type="submit">
                    Confirm
                </button>
            </form>
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
}

export default Verify;