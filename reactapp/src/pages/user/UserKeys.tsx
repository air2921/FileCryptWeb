import React from 'react';
import Font from '../../utils/helpers/icon/Font';

interface UserKeysProps {
    keys: {
        privateKey: boolean;
        internalKey: boolean;
        receivedKey: boolean;
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
                    {keys.privateKey ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
            <div className="internal-key">
                <div className="internal-key-name">
                    Internal Key
                </div>
                <div className="has-key">
                    {keys.internalKey ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
            <div className="received-key">
                <div className="received-key-name">
                    Received Key
                </div>
                <div className="has-key">
                    {keys.receivedKey ? <Font font={'check_small'} /> : <Font font={'close_small'} />}
                </div>
            </div>
        </div>
    );
}

export default UserKeys;