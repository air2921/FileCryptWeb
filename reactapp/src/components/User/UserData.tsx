import React from 'react';

function UserData({ user }: UserDataProps) {
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
        </div>
    );
};

export default UserData;