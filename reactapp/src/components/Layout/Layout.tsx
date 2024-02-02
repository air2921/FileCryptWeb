import React, { useEffect, useState } from 'react';
import Font from '../Font/Font';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import cookie from 'react-cookies'
import useAuth from '../UseAuth/UseAuth'
import AxiosRequest from '../../api/AxiosRequest';
import './Layout.css'
function Layout() {
    const [username, setUsername] = useState(cookie.load('auth_username'));
    const [id, setId] = useState(cookie.load('auth_user_id'));
    const [role, setRole] = useState(cookie.load('auth_role'));
    const [profilePath, setPath] = useState('');
    const [isAsideVisible, setAsideVisible] = useState(sessionStorage.getItem('isAsideVisible') === 'true');
    const [inputValue, setInputValue] = useState('');
    const [inputError, setInputError] = useState(false);

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

    const findUser = async () => {
        if (inputValue === '') {
            setInputError(true);

            setTimeout(() => {
                setInputError(false);
            }, 3000)

            return;
        }

        const hashtagIndex = inputValue.indexOf('#');
        if (hashtagIndex !== -1) {
            const findUsername = inputValue.substring(0, hashtagIndex);
            const findUserId = parseInt(inputValue.substring(hashtagIndex + 1), 10);

            const response = await AxiosRequest({
                endpoint: `api/core/users/find?username=${findUsername}&userId=${findUserId ? findUserId : 0}`,
                method: 'GET',
                withCookie: true,
                requestBody: null
            });

            if (response.statusCode === 404) {
                navigate('*');
            }

            if (response.statusCode === 200) {
                navigate(`/user/${response.data.id}/${response.data.username}`);
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
        <div className="layout-container">
            {isAuth && (
                <div>
                    {isAsideVisible && (
                        <aside className="sidebar" style={{ width: isAsideVisible ? "120px" : 0 }}>
                            <nav>
                                <div className="links-container">
                                    <div className="link">
                                        <Link to={profilePath}>
                                            <Font font={'account_circle'} />
                                            <h4>Profile</h4>
                                        </Link>
                                    </div>
                                    <div className="link">
                                        <Link to="/settings">
                                            <Font font={'manage_accounts'} />
                                            <h4>Settings</h4>
                                        </Link>
                                    </div>
                                    <div className="link">
                                        <Link to="/files">
                                            <Font font={'storage'} />
                                            <h4>Files</h4>
                                        </Link>
                                    </div>
                                    <div className="link">
                                        <Link to="/offers">
                                            <Font font={'storage'} />
                                            <h4>Offers</h4>
                                        </Link>
                                    </div>
                                    <div className="link">
                                        <Link to="/api">
                                            <Font font={'vpn_key'} />
                                            <h4>API</h4>
                                        </Link>
                                    </div>
                                    {(role === 'Admin' || role === 'HighestAdmin') && (
                                        <div className="link">
                                            <Link to="/admin">
                                                <Font font={'admin_panel_settings'} />
                                                <h4>Admin</h4>
                                            </Link>
                                        </div>
                                    )}
                                </div>
                            </nav>
                        </aside>
                    )}
                </div>
            )}
            <div className="header-outlet-container">
                <header className="head">
                    {isAuth && (
                        <div className="aside-visible-container">
                            <button className="aside-visible-btn" onClick={() => setAsideVisible(!isAsideVisible)}>
                                <Font font={'menu'} />
                            </button>
                        </div>
                    )}
                    <nav>
                        <div>
                            <div className="header-links">
                                <Link to="/">FILECRYPT</Link>
                                <Link to="/about">About</Link>
                                <Link to="/policy">Policy</Link>
                            </div>
                            {isAuth && (
                                <div className="head-center">
                                    <div className="find-container">
                                        <input className={inputError ? 'find-input error' : 'find-input'}
                                            type="text" id="user"
                                            required={true}
                                            value={inputValue}
                                            onChange={(e) => setInputValue(e.target.value)}
                                        />
                                        <button className="find-btn" onClick={findUser}><Font font={'search'} /></button>
                                    </div>
                                    <div className="notification-container">
                                        <Link to="/notifications"><Font font={'notifications'} /></Link>
                                    </div>
                                </div>
                            )}
                            <div className="auth-btn-container">
                                {isAuth && (
                                    <div className="signout-btn-container">
                                        <button className="signout-btn" onClick={logout}>Sign Out</button>
                                    </div>
                                )}
                                {!isAuth && (
                                    <div className="is-auth-container">
                                        <div className="signup-btn-container">
                                            <button className="signup-btn" onClick={() => navigate('/auth/signup')}>Sign Up</button>
                                        </div>
                                        <div className="signin-btn-container">
                                            <button className="signin-btn" onClick={() => navigate('/auth/login')}>Sign In</button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </nav>
                </header>
                <div className="outlet" style={{ marginLeft: isAsideVisible ? '152px' : "32px" }}>
                    <Outlet />
                </div>
            </div>
        </div>
    );
}

export default Layout;