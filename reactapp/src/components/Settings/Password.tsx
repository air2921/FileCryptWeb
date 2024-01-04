import React, { FormEvent, useState } from 'react';
import Message from '../Message/Message';
import AxiosRequest from '../../api/AxiosRequest';
import Input from '../Helpers/Input';
import Button from '../Helpers/Button';

const Password = () => {

    const [message, setMessage] = useState('');
    const [font, setFont] = useState('')

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
            setMessage(response.data.message);
            setFont('done')
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    }

    return (
        <div className="password">
            <form onSubmit={handleSubmit}>
                <Input text='Confirm Password' type="password" id="old" require={true} value={oldPassword} onChange={(e) => setOld(e.target.value)} />
                <Input text='New Password' type="password" id="new" require={true} value={newPassword} onChange={(e) => setNew(e.target.value)} />
                <Button>Save Password</Button>
            </form>
            {message && <Message message={message} font={font} />}
        </div>
    );
}

export default Password;