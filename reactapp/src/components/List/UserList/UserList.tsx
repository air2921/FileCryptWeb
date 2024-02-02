import React, { useReducer } from 'react';
import Message from '../../Message/Message';
import Button from '../../Helpers/Button';
import { useNavigate } from 'react-router-dom';

function UserList({ users, isAdmin }: UserListProps) {

    if (!users || users.every(user => user === null)) {
        return <div><Message message={'Your search returned no results'} font='storage' /></div>;
    }

    const navigate = useNavigate();

    return (
        <ul>
            {users
                .filter(user => user !== null)
                .map(user => (
                    <li key={user.id} className="user">
                        <Button onClick={() => navigate(`user/${user.id}/${user.username}`)}>
                            <div className="user-header">
                                {user.is_blocked && (<div className="user-block">Blocked</div>)}
                                <div className="username-id">{user.username}#{user.id}</div>
                                <div className="role">{user.role}</div>
                            </div>
                        </Button>
                    </li>
                ))}
        </ul>
    );
}

export default UserList;