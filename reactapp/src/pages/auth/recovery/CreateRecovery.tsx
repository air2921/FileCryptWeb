import React, { FormEvent, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import AxiosRequest from '../../../utils/api/AxiosRequest';

const CreateRecovery = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/auth/recovery/unique/token?email=${email}`, method: 'POST', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setMessage(response.data.message);
            setFont('check_small')
        }
        else {
            setMessage(response.data);
            setFont('error');
        }
    };

    return (
        <div className="recovery-container">
            <div className="recovery-header">
                Recovery Account
            </div>
            <form className="recovery-form" onSubmit={handleSubmit}>
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
                    <Message message={message} font={font} />
                </div>
            }
        </div>
    );
}

export default CreateRecovery;