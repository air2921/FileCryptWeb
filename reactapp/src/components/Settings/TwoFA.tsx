import React, { FormEvent, useState } from 'react';
import Button from '../Helpers/Button';
import Message from '../Message/Message';
import Input from '../Helpers/Input';
import AxiosRequest from '../../api/AxiosRequest';
import Modal from '../Modal/Modal';
import Verify from '../Verify/Verify';

interface TwoFaProps {
    is_enabled_2fa: boolean
}

function TwoFA({ is_enabled_2fa }: TwoFaProps) {
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');
    const [password, setPassword] = useState('');
    const [successStatus, setStatus] = useState(false);
    const [is2Fa, set2Fa] = useState(true);
    const [visibleForm, setFormVisible] = useState(false);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/account/edit/2fa/start?password=${password}`, method: 'POST', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setStatus(true);
            setMessage('');
            setFont('');
        }
        else {
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000);
    }

    const set2FaStatus = (twoFaStatus: boolean, formVisible: boolean) => {
        set2Fa(twoFaStatus);
        setFormVisible(formVisible);
    }

    return (
        <div>
            {is_enabled_2fa && !visibleForm && < Button onClick={() => set2FaStatus(false, true)}>Disable 2FA</Button>}
            {!is_enabled_2fa && !visibleForm && < Button onClick={() => set2FaStatus(true, true)}>Enable 2FA</Button>}
            {visibleForm && (
                <form onSubmit={handleSubmit}>
                    <Input text='Confirm password' type="password" id="password" require={true} value={password} onChange={(e) => setPassword(e.target.value)} />
                    <Button>Send message</Button>
                </form>
            )}
            {message && <Message message={message} font={font} />}
            <Modal isActive={successStatus} setActive={setStatus}>
                <Verify endpoint={`api/account/edit/2fa/confirm/${is2Fa}`} method={'PUT'} />
            </Modal>
        </div>
    );
}

export default TwoFA;