import React from 'react';
import Font from '../../utils/helpers/icon/Font';
import { useNavigate } from 'react-router-dom';

interface UserDataProps {
    user: {
        username: string;
        id: string;
        role: string;
        email: string;
        is_blocked: boolean
    };
    isOwner: boolean
    showButton: boolean
}

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