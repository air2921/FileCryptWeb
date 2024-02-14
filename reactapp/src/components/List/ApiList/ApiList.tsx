import React, { useEffect, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import Modal from '../../Modal/Modal';
import DateComponent from '../../../utils/helpers/date/Date';
import Font from '../../../utils/helpers/icon/Font';

function ApiList({ apis, deleteApi }: ApiListProps) {
    const [isActive, setActive] = useState(false);
    const [apiData, setApiData] = useState<ApiProps | null>(null)

    if (!apis || apis.every(api => api === null)) {
        return <div><Message message={'No one active API Key'} font='vpn_key' /></div>;
    }

    const openModal = (api: ApiProps) => {
        setApiData(api);
        setActive(true);
    }

    const ModalContent = () => {

        if (!apiData)
            return;

        return (
            <div>
                {apiData.is_blocked && (
                    <div className="is-blocked">
                        Temporarily blocked
                    </div>
                )}
                <div className="id">
                    <div className="apiId">API ID: #{apiData.api_id}</div>
                    <div className="user-id">API Owner: #{apiData.user_id}</div>
                </div>
                <div className="api-key">API Key: {apiData.api_key}</div>
                <div className="api-type">API Key type: {apiData.type}</div>
                <div className="time">
                    <div className="expiry">
                        {apiData.expiry_date ? <div>Valid until: <DateComponent date={apiData.expiry_date} /></div> : <div>Valid until: No time restrictions</div>}
                    </div>
                    <div className="last-time-activity">
                        Time of last activity: <DateComponent date={apiData.last_time_activity} />
                    </div>
                </div>
                <div className="request-day-limit">
                    Max request of day: {apiData.max_request_of_day}
                </div>
                <div className="api-call-left">API call today left: {apiData.apiCallLeft}</div>
                {deleteApi && (
                    <button className="delete-api" onClick={() => deleteApi(apiData.api_id)}>
                        <Font font={'delete'} />
                    </button>
                )}
            </div>
        );
    }

    return (
        <>
            <ul>
                <Message message={'Your API Keys'} font='vpn_key' />
                {apis
                    .filter(api => api !== null)
                    .map(api => (
                        <li key={api.api_id} className="api">
                            <div className="apiId">API ID#{api.api_id}</div>
                            <div className="apiKey">Key: {api.api_key}</div>
                            <div className="api-type">API Key Type: {api.type}</div>
                            <div className="expiry">
                                {api.expiry_date ? <div>Valid until: <DateComponent date={api.expiry_date} /></div> : <div>Valid until: No time restrictions</div>}
                            </div>
                            <button className="api-details-btn" onClick={() => openModal(api)}>More</button>
                        </li>
                    ))}
            </ul>
            <Modal isActive={isActive} setActive={setActive}>
                <ModalContent />
            </Modal>
        </>
    );
}

export default ApiList;