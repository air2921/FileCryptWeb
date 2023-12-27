import React from 'react';

function UserKeys({ keys, isOwner }: UserKeysProps) {
    return (
        <div className="keys-container">
            <div className="private-key">
                <span className="private-key-name">
                    Private Key
                </span>
                <span className="has-key">
                    {keys.privateKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                    {isOwner && <button>Change</button>}
                </span>
            </div>
            <div className="internal-key">
                <span className="internal-key-name">
                    Internal Key
                </span>
                <span className="has-key">
                    {keys.internalKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                    {isOwner && <button>Change</button>}
                </span>
            </div>
            <div className="received-key">
                <span className="received-key-name">
                    Received Key
                </span>
                <span className="has-key">
                    {keys.receivedKey ? <i className="material-icons-sharp">check_small</i> : <i className="material-icons-sharp">close_small</i>}
                    {isOwner && <button>Change</button>}
                </span>
            </div>
        </div>
    );
}

export default UserKeys;