import React from 'react';

function UserKeys({ keys, isOwner }: UserKeysProps) {
    return (
        <div className="keys-container">
            <div className="private-key">
                <div className="private-key-name">
                    Private Key
                </div>
                <div className="has-key">
                    {keys.privateKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                </div>
            </div>
            <div className="internal-key">
                <div className="internal-key-name">
                    Internal Key
                </div>
                <div className="has-key">
                    {keys.internalKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                </div>
            </div>
            <div className="received-key">
                <div className="received-key-name">
                    Received Key
                </div>
                <div className="has-key">
                    {keys.receivedKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                </div>
            </div>
            {isOwner && <button>Change</button>}
        </div>
    );
}

export default UserKeys;