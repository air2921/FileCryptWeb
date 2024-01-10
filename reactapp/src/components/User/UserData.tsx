import React from 'react';
import Font from '../Font/Font';

function UserData({ user, isOwner }: UserDataProps) {
    return (
        <div className="user-data-container">
            <div>
                {user.is_blocked && 'User is blocked'}
            </div>
            <div>
                <div className="username">
                    {`${user.username}#${user.id}`}
                </div>
                <div className="role">
                    {user.role}
                </div>
            </div>
            {isOwner && (
                <div className="email">
                    <Font font={'mail'} /> {user.email}
                </div>
            )}
        </div>
    );
};

export default UserData;