import React from 'react';
import Font from '../Font/Font';
import Button from '../Helpers/Button';
import { useNavigate } from 'react-router-dom';

function UserData({ user, isOwner }: UserDataProps) {
    const navigate = useNavigate();

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
            {isOwner && (
                <Button onClick={() => navigate("/settings")}>
                    <Font font={'edit'} />
                </Button>
            )}
        </div>
    );
};

export default UserData;