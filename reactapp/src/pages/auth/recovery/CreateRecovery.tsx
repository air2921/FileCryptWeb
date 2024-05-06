import React, { FormEvent, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import { createRecovery } from '../../../utils/api/Auth';

const CreateRecovery = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const createRecoverySubmit = async (e: FormEvent) => {
        e.preventDefault();

        const result = await createRecovery(email);

        if (result.statusCode === 201) {
            setMessage(result.message);
            setFont('check_small')
        } else {
            setMessage(result.message);
            setFont('error')
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
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
            {message &&
                <div className="recovery-message">
                    <Message message={message} icon={font} />
                </div>
            }
        </div>
    );
}

export default CreateRecovery;