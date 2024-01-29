﻿import React, { useEffect, useState } from 'react';
import Font from '../Font/Font';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import cookie from 'react-cookies'
import useAuth from '../UseAuth/UseAuth'
import AxiosRequest from '../../api/AxiosRequest';
import Button from '../Helpers/Button';

function Layout() {
    const [username, setUsername] = useState(cookie.load('auth_username'));
    const [id, setId] = useState(cookie.load('auth_user_id'));
    const [role, setRole] = useState(cookie.load('auth_role'));
    const [profilePath, setPath] = useState('');
    const [isAsideVisible, setAsideVisible] = useState(sessionStorage.getItem('isAsideVisible') === 'true');
    const isAuth = useAuth();
    const navigate = useNavigate();

    const getAuthStatus = async () => {
        const response = await AxiosRequest({ endpoint: 'api/auth/check', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            return true;
        }
        else {
            return false;
        }
    }

    const checkCookies = async () => {
        const newUsername = cookie.load('auth_username');
        const newId = cookie.load('auth_user_id');
        const newRole = cookie.load('auth_role');

        if (isAuth) {
            if (newUsername === undefined || newId === undefined || newRole === undefined || username === undefined || id === undefined || role === undefined) {
                const isAuthStatus = await getAuthStatus()

                if (isAuthStatus) {
                    setUsername(newUsername);
                    setId(newId);
                    setRole(newRole);
                }
            }
            else {
                setUsername(newUsername);
                setId(newId);
                setRole(newRole);
            }
        }
    };

    const logout = async () => {
        const response = await AxiosRequest({ endpoint: 'api/auth/logout', method: 'PUT', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            navigate('/')
        }
    }

    useEffect(() => {
        checkCookies();
        setPath(`/user/${id}/${username}`);
    }, [isAuth, navigate]);

    return (
        <div style={{ display: 'flex', minHeight: '100vh' }}>
            {isAsideVisible && (
                <aside style={{ background: '#000', padding: '1rem', width: '200px', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <nav>
                        <div>
                            <Link to={profilePath}>
                                <Font font={'account_circle'} />
                                <h3>Profile</h3>
                            </Link>
                            <Link to="/settings">
                                <Font font={'manage_accounts'} />
                                <h3>Account</h3>
                            </Link>
                            <Link to="/notifications">
                                <Font font={'notifications'} />
                                <h3>Notifications</h3>
                            </Link>
                            <Link to="/offers">
                                <Font font={'storage'} />
                                <h3>Offers</h3>
                            </Link>
                            <Link to="/files">
                                <Font font={'storage'} />
                                <h3>Files</h3>
                            </Link>
                            {role === 'Admin' || role === 'HighestAdmin' && (
                                <Link to="/admin">
                                    <Font font={'admin_panel_settings'} />
                                    <h3>Admin Panel</h3>
                                </Link>
                            )}
                        </div>
                    </nav>
                </aside>
            )}
            {isAuth && (
                <div className="aside-visible-btn">
                    {!isAsideVisible && (
                        <Button onClick={() => setAsideVisible(true)}>Open Sidebar</Button>
                    )}
                    {isAsideVisible && (
                        <Button onClick={() => setAsideVisible(false)}>Close Sidebar</Button>
                    )}
                </div>
            )}
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                <header style={{ background: '#333', color: '#fff', padding: '1rem', textAlign: 'center' }}>
                    <nav>
                        <div>
                            <Link to="/">Home</Link>
                            <Link to="/about">About</Link>
                            <Link to="/policy">Policy</Link>
                            {!isAuth && (
                                <div className="auth">
                                    <Button onClick={() => navigate('/auth/signup')}>Sign Up</Button>
                                    <Button onClick={() => navigate('/auth/login')}>Sign In</Button>
                                </div>
                            )}
                            {isAuth && (
                                <Button onClick={logout}>Log Out</Button>
                            )}
                        </div>
                    </nav>
                </header>
                <div style={{ flex: 1, padding: '1rem' }}>
                    <Outlet />
                </div>
            </div>
        </div>
    );
}

export default Layout;