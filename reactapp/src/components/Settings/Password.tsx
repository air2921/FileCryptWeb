import React, { FormEvent, useState } from 'react';
import Message from '../Message/Message';
import AxiosRequest from '../../api/AxiosRequest';
import Input from '../Input/Input';

const Password = () => {

    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    const [oldPassword, setOld] = useState('');
    const [newPassword, setNew] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({
            endpoint: `api/account/edit/password`,
            method: 'PUT',
            withCookie: true,
            requestBody: {
                OldPassword: oldPassword,
                NewPassword: newPassword
            }
        })

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
        <div className="password">
            <form onSubmit={handleSubmit}>
                <Input type="password" id="old" require={true} value={oldPassword} onChange={(e) => setOld(e.target.value)} />
                <Input type="password" id="new" require={true} value={newPassword} onChange={(e) => setNew(e.target.value)} />
                <button type="submit" className="btn btn-primary btn-disabled">
                    Update password
                </button>
            </form>
            {successMessage && <Message message={successMessage} font='done' />}
            {errorMessage && <Message message={errorMessage} font='error' />}
        </div>
    );
}

export default Password;