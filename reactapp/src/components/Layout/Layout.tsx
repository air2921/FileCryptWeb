import React, { useEffect, useState } from 'react';
import Font from '../../utils/helpers/icon/Font';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import cookie from 'react-cookies'
import AxiosRequest from '../../utils/api/AxiosRequest';
import './Layout.css'
import useResize from '../UseResize/useResize';
import useAuth from '../UseAuth/useAuth';

function Layout() {
    const [username, setUsername] = useState(cookie.load('auth_username'));
    const [id, setId] = useState(cookie.load('auth_user_id'));
    const [role, setRole] = useState(cookie.load('auth_role'));
    const [profilePath, setPath] = useState('');
    const [isAsideVisible, setAsideVisible] = useState(sessionStorage.getItem('isAsideVisible') === 'true');
    const [inputValue, setInputValue] = useState('');
    const [inputError, setInputError] = useState(false);

    const resetAsideVisible = () => {
        setAsideVisible(false);
    };

    const isAuth = useAuth();
    const isDesktop = useResize();
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
                setInputValue('');
            }

            if (response.statusCode === 200) {
                navigate(`/user/${response.data.id}/${response.data.username}`);
                setInputValue('');
            }
        }
        else {

            //Здесь возможно будет поиск списка юзеров по юзернейму

            setInputError(true);

            setTimeout(() => {
                setInputError(false);
            }, 3000)

            return;
        }
    };

    const logout = async () => {
        const response = await AxiosRequest({ endpoint: 'api/auth/logout', method: 'PUT', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            navigate('/auth/login')
        }
    }

    useEffect(() => {
        checkCookies();
        setPath(`/user/${id}/${username}`);
    }, [isAuth, navigate]);

    return (
        <div className="layout-container">
            {isAuth && isAsideVisible && isDesktop && (
                <aside className="sidebar" style={{ width: isAsideVisible ? "115px" : 0 }}>
                    <nav>
                        <div className="desktop-sidebar-links-container">
                            <div className="desktop-link">
                                <Link to={profilePath}>
                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>account_circle</i>
                                    <h4>Profile</h4>
                                </Link>
                            </div>
                            <div className="desktop-link">
                                <Link to="/settings">
                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>manage_accounts</i>
                                    <h4>Settings</h4>
                                </Link>
                            </div>
                            <div className="desktop-link">
                                <Link to="/files">
                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>storage</i>
                                    <h4>Files</h4>
                                </Link>
                            </div>
                            <div className="desktop-link">
                                <Link to="/offers">
                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>storage</i>
                                    <h4>Offers</h4>
                                </Link>
                            </div>
                            <div className="desktop-link">
                                <Link to="/api">
                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>vpn_key</i>
                                    <h4>API</h4>
                                </Link>
                            </div>
                            {(role === 'Admin' || role === 'HighestAdmin') && (
                                <div className="desktop-link">
                                    <Link to="/admin">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>admin_panel_settings</i>
                                        <h4>Admin</h4>
                                    </Link>
                                </div>
                            )}
                        </div>
                    </nav>
                </aside>
            )}
            {isAsideVisible && !isDesktop && (
                <div className="mobile-menu" onClick={resetAsideVisible}>
                    <nav>
                        <div className="mobile-menu" onClick={resetAsideVisible}>
                            {isAuth && (
                                <>
                                    <div className="mobile-link">
                                        <Link to={profilePath}>
                                            <div className="link-content">
                                                <div className="icon">
                                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>account_circle</i>
                                                </div>
                                                <div className="name">
                                                    Profile
                                                </div>
                                            </div>
                                        </Link>
                                    </div>
                                    <div className="mobile-link">
                                        <Link to="/settings">
                                            <div className="link-content">
                                                <div className="icon">
                                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>manage_accounts</i>
                                                </div>
                                                <div className="name">
                                                    Settings
                                                </div>
                                            </div>
                                        </Link>
                                    </div>
                                    <div className="mobile-link">
                                        <Link to="/files">
                                            <div className="link-content">
                                                <div className="icon">
                                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>storage</i>
                                                </div>
                                                <div className="name">
                                                    Files
                                                </div>
                                            </div>
                                        </Link>
                                    </div>
                                    <div className="mobile-link">
                                        <Link to="/offers">
                                            <div className="link-content">
                                                <div className="icon">
                                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>storage</i>
                                                </div>
                                                <div className="name">
                                                    Offers
                                                </div>
                                            </div>
                                        </Link>
                                    </div>
                                    <div className="mobile-link">
                                        <Link to="/api">
                                            <div className="link-content">
                                                <div className="icon">
                                                    <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>vpn_key</i>
                                                </div>
                                                <div className="name">
                                                    API
                                                </div>
                                            </div>
                                        </Link>
                                    </div>
                                    {(role === 'Admin' || role === 'HighestAdmin') && (
                                        <div className="mobile-link">
                                            <Link to="/admin">
                                                <div className="link-content">
                                                    <div className="icon">
                                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>admin_panel_settings</i>
                                                    </div>
                                                    <div className="name">
                                                        Admin
                                                    </div>
                                                </div>
                                            </Link>
                                        </div>
                                    )}
                                </>
                            )}
                            <div className="mobile-link">
                                <Link to="/">
                                    <div className="link-content">
                                        <div className="icon">
                                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>home</i>
                                        </div>
                                        <div className="name">
                                            Home
                                        </div>
                                    </div>
                                </Link>
                            </div>
                            <div className="mobile-link">
                                <Link to="/about">
                                    <div className="link-content">
                                        <div className="icon">
                                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>info</i>
                                        </div>
                                        <div className="name">
                                            About
                                        </div>
                                    </div>
                                </Link>
                            </div>
                            <div className="mobile-link">
                                <Link to="/policy">
                                    <div className="link-content">
                                        <div className="icon">
                                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>policy</i>
                                        </div>
                                        <div className="name">
                                            Policy
                                        </div>
                                    </div> 
                                </Link>
                            </div>
                            <div className="mobile-auth-btn-container">
                                {isAuth ? (
                                    <div className="mobile-signout-btn-container">
                                        <button className="mobile-signout-btn" onClick={logout}>Sign Out</button>
                                    </div>
                                ) : (
                                    <div className="mobile-is-auth-container">
                                        <div className="mobile-signup-btn-container" onClick={resetAsideVisible}>
                                            <button className="mobile-signup-btn" onClick={() => navigate('/auth/signup')}>Sign Up</button>
                                        </div>
                                        <div className="mobile-signin-btn-container" onClick={resetAsideVisible}>
                                            <button className="mobile-signin-btn" onClick={() => navigate('/auth/login')}>Sign In</button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </nav>
                </div>
            )}
            <div className="header-outlet-container">
                <header className="head">
                    <div className="aside-visible-container">
                        {!isAuth && isDesktop ? (
                            <>
                            </>
                        ) : (
                                <button className="aside-visible-btn" onClick={() => setAsideVisible(!isAsideVisible)}>
                                    <i className="material-icons-outlined" style={{ background: 'transparent' }}>menu</i>
                                </button>
                        )}
                    </div>
                    <nav>
                        <div>
                            <div className="header-links">
                                <Link to="/">FILECRYPT</Link>
                                <Link to="/about">About</Link>
                                <Link to="/policy">Policy</Link>
                            </div>
                            <div className="head-center">
                                <div className="find-container">
                                    <input className={inputError ? 'find-input error' : 'find-input'}
                                        type="text" id="user"
                                        required={true}
                                        value={inputValue}
                                        onChange={(e) => setInputValue(e.target.value)}
                                        placeholder="#2921"
                                    />
                                    <button className="find-btn" onClick={findUser}><Font font={'search'} /></button>
                                </div>
                                {isAuth && (
                                    <div className="notification-container">
                                        <Link to="/notifications"><Font font={'notifications'} /></Link>
                                    </div>
                                )}
                            </div>
                            <div className="auth-btn-container">
                                {isAuth ? (
                                    <div className="signout-btn-container">
                                        <button className="signout-btn" onClick={logout}>Sign Out</button>
                                    </div>
                                ) : (
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
                {isAsideVisible && !isDesktop ? (
                    <>
                    </>
                ) : (
                        <div className="outlet" style={{ marginLeft: isAsideVisible && isDesktop && isAuth ? '150px' : 0 }}>
                            <Outlet />
                        </div>
                )}
            </div>
        </div>
    );
}

export default Layout;