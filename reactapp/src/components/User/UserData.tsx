import React from 'react';
import Font from '../../utils/helpers/icon/Font';
import { useNavigate } from 'react-router-dom';

function UserData({ user, isOwner, showButton }: UserDataProps) {
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
            {isOwner && showButton &&(
                <button onClick={() => navigate("/settings")}>
                    Edit profile
                </button>
            )}
        </div>
    );
};

export default UserData;