import React, { FormEvent, useState } from 'react';
import Message from '../../../components/widgets/Message';
import { createRecovery } from '../../../utils/api/Auth';

const CreateRecovery = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [status, setStatus] = useState(false);

    function setMessageState(message: string) {
        setMessage(message);
    }

    const createRecoverySubmit = async (e: FormEvent) => {
        e.preventDefault();

        const result = await createRecovery(email);

        if (result.statusCode === 201) {
            setMessage(result.message);
            setStatus(true);
        } else {
            setMessage(result.message);
            setStatus(false);
        }
    }

    return (
        <div className="recovery-container">
            <div className="recovery-header">
                Recovery Account
            </div>
            <form className="recovery-form" onSubmit={createRecoverySubmit}>
                <div className="recovery-text">Enter your latest email address</div>
                <div className="recovery-email-container">
                    <label htmlFor="recovery-email">
                        <input className="recovery-email-input"
                            type="email"
                            id="recovery-email"
                            required={true}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </label>
                </div>
                <div className="recovery-btn-container">
                    <button className="recovery-btn-submit" type="submit">Send recovery link</button>
                </div>
            </form>
            <div className="recovery-message">
                <Message message={message} success={status} onMessageChange={setMessageState} />
            </div>
        </div>
    );
}

export default CreateRecovery;