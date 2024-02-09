import React, { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AxiosRequest from '../../api/AxiosRequest';
import Message from '../Message/Message';

function Verify({ endpoint, method }: VerifyProps) {
    const [code, setCode] = useState<number>();
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
                <input
                    type="text"
                    id="code"
                    required={true}
                    value={code}
                    onChange={(e) => {
                        const value = e.target.value;
                        if (value === '') {
                            setCode(undefined);
                        } else {
                            const parsedValue = parseInt(value, 10);
                            if (!isNaN(parsedValue)) {
                                setCode(parsedValue);
                            }
                        }
                    }}
                    inputMode="numeric"
                />
                <button type="submit">Confirm</button>
            </form>
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
}

export default Verify;