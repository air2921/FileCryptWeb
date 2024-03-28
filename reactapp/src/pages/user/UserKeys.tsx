import React from 'react';
import Font from '../../utils/helpers/icon/Font';

interface UserKeysProps {
    keys: {
        private_key: boolean;
        internal_key: boolean;
        received_key: boolean;
    };
}

function UserKeys({ keys }: UserKeysProps) {
    return (
        <div className="keys-container">
            <div className="private-key">
                <div className="private-key-name">
                    Private Key
                </div>
                <div className="has-key">
                    {keys.private_key ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
            <div className="internal-key">
                <div className="internal-key-name">
                    Internal Key
                </div>
                <div className="has-key">
                    {keys.internal_key ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
            <div className="received-key">
                <div className="received-key-name">
                    Received Key
                </div>
                <div className="has-key">
                    {keys.received_key ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
        </div>
    );
}

export default UserKeys;