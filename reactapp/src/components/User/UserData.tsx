import React from 'react';

function UserData({ user, isOwner }: UserDataProps) {
    return (
        <div className="user-data-container">
            <div>
                <span className="username">
                    {`${user.username}#${user.id}`}
                    {isOwner && <button>Edit</button>}
                </span>
                <span className="role">{user.role}</span>
            </div>
            <div>
                <span className="email">
                    {user.email}
                    {isOwner && <button>Edit</button>}
                </span>
            </div>
        </div>
    );
};

export default UserData;