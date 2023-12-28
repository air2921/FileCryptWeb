import React from 'react';

function UserData({ user, isOwner }: UserDataProps) {
    return (
        <div className="user-data-container">
            <div>
                <div className="username">
                    {`${user.username}#${user.id}`}
                </div>
                <div className="role">
                    {user.role}
                </div>
            </div>
            <div className="email">
                {user.email}
            </div>
            {isOwner && <button>Edit</button>}
        </div>
    );
};

export default UserData;