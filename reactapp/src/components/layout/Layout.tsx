import React, { useEffect, useState } from 'react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import cookie from 'react-cookies'
import './Layout.css'
import useResize from '../hooks/useResize';
import useAuth from '../hooks/useAuth';
import { getAuth, logout } from '../../utils/api/Auth';
import Icon from '../../utils/helpers/icon/Icon';

function Layout() {
    const [id, setId] = useState(cookie.load('auth_user_id'));
    const [role, setRole] = useState(cookie.load('auth_role'));
    const [profilePath, setPath] = useState('');
    const [isAsideVisible, setAsideVisible] = useState(sessionStorage.getItem('isAsideVisible') === 'true');
    const [inputValue, setInputValue] = useState('');
    const [inputError, setInputError] = useState(false);

    const isAuth = useAuth();
    const isDesktop = useResize();
    const navigate = useNavigate();

    const resetAsideVisible = () => {
        setAsideVisible(false);
    };

    const getAuthStatus = async () => {
        const response = await getAuth()

        if (response.success) {
            return true;
        }
        else {
            return false;
        }
    }

    const checkCookies = async () => {
        const newId = cookie.load('auth_user_id');
        const newRole = cookie.load('auth_role');

        if (isAuth) {
            if (!newId || !newRole || !id || !role) {
                const isAuthStatus = await getAuthStatus()

                if (isAuthStatus) {
                    setId(newId);
                    setRole(newRole);
                }
            } else {
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
            const findUserId = parseInt(inputValue.substring(hashtagIndex + 1), 10);

            if (!isAuth) {
                navigate('*');
                setInputValue('');
                return;
            }

            navigate(`/user/${findUserId}`);
        } else {

            //Здесь возможно будет поиск списка юзеров по юзернейму

            setInputError(true);

            setTimeout(() => {
                setInputError(false);
            }, 3000)

            return;
        }
    };

    const logoutMe = async () => {
        const response = await logout();

        if (response.success) {
            navigate('/auth/login')
        }
    }

    const DesktopSidebar = () => {
        return (
            <>
                <div className="layout-desktop-sidebar-links-container">
                    <div className="layout-desktop-link">
                        <Link to={profilePath}>
                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>account_circle</i>
                            <h4>Profile</h4>
                        </Link>
                    </div>
                    <div className="layout-desktop-link">
                        <Link to="/settings">
                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>manage_accounts</i>
                            <h4>Settings</h4>
                        </Link>
                    </div>
                    <div className="layout-desktop-link">
                        <Link to="/files">
                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>storage</i>
                            <h4>Files</h4>
                        </Link>
                    </div>
                    <div className="layout-desktop-link">
                        <Link to="/offers">
                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>storage</i>
                            <h4>Offers</h4>
                        </Link>
                    </div>
                    <div className="layout-desktop-link">
                        <Link to="/api">
                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>vpn_key</i>
                            <h4>API</h4>
                        </Link>
                    </div>
                    {(role === 'Admin' || role === 'HighestAdmin') && (
                        <div className="layout-desktop-link">
                            <Link to="/admin">
                                <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '26px' }}>admin_panel_settings</i>
                                <h4>Admin</h4>
                            </Link>
                        </div>
                    )}
                </div>
            </>
        );
    }

    const MobileSidebar = () => {
        return (
            <div className="layout-mobile-menu" onClick={resetAsideVisible}>
                {isAuth && (
                    <>
                        <div className="layout-mobile-link">
                            <Link to={profilePath}>
                                <div className="layout-mobile-link-content">
                                    <div className="layout-mobile-icon">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>account_circle</i>
                                    </div>
                                    <div className="layout-mobile-name">
                                        Profile
                                    </div>
                                </div>
                            </Link>
                        </div>
                        <div className="layout-mobile-link">
                            <Link to="/settings">
                                <div className="layout-mobile-link-content">
                                    <div className="layout-mobile-icon">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>manage_accounts</i>
                                    </div>
                                    <div className="layout-mobile-name">
                                        Settings
                                    </div>
                                </div>
                            </Link>
                        </div>
                        <div className="layout-mobile-link">
                            <Link to="/files">
                                <div className="layout-mobile-link-content">
                                    <div className="layout-mobile-icon">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>storage</i>
                                    </div>
                                    <div className="layout-mobile-name">
                                        Files
                                    </div>
                                </div>
                            </Link>
                        </div>
                        <div className="layout-mobile-link">
                            <Link to="/offers">
                                <div className="layout-mobile-link-content">
                                    <div className="layout-mobile-icon">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>storage</i>
                                    </div>
                                    <div className="layout-mobile-name">
                                        Offers
                                    </div>
                                </div>
                            </Link>
                        </div>
                        <div className="layout-mobile-link">
                            <Link to="/api">
                                <div className="layout-mobile-link-content">
                                    <div className="layout-mobile-icon">
                                        <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>vpn_key</i>
                                    </div>
                                    <div className="layout-mobile-name">
                                        API
                                    </div>
                                </div>
                            </Link>
                        </div>
                        {(role === 'Admin' || role === 'HighestAdmin') && (
                            <div className="layout-mobile-link">
                                <Link to="/admin">
                                    <div className="layout-mobile-link-content">
                                        <div className="layout-mobile-icon">
                                            <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>admin_panel_settings</i>
                                        </div>
                                        <div className="layout-mobile-name">
                                            Admin
                                        </div>
                                    </div>
                                </Link>
                            </div>
                        )}
                    </>
                )}
                <div className="layout-mobile-link">
                    <Link to="/">
                        <div className="layout-mobile-link-content">
                            <div className="layout-mobile-icon">
                                <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>home</i>
                            </div>
                            <div className="layout-mobile-name">
                                Home
                            </div>
                        </div>
                    </Link>
                </div>
                <div className="layout-mobile-link">
                    <Link to="/about">
                        <div className="layout-mobile-link-content">
                            <div className="layout-mobile-icon">
                                <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>info</i>
                            </div>
                            <div className="layout-mobile-name">
                                About
                            </div>
                        </div>
                    </Link>
                </div>
                <div className="layout-mobile-link">
                    <Link to="/policy">
                        <div className="layout-mobile-link-content">
                            <div className="layout-mobile-icon">
                                <i className="material-icons-outlined" style={{ background: 'transparent', fontSize: '28px' }}>policy</i>
                            </div>
                            <div className="layout-mobile-name">
                                Policy
                            </div>
                        </div>
                    </Link>
                </div>
                <div className="layout-mobile-auth-btn-container">
                    {isAuth ? (
                        <div className="layout-mobile-signout-btn-container">
                            <button className="layout-mobile-signout-btn" onClick={logout}>Sign Out</button>
                        </div>
                    ) : (
                            <div className="layout-mobile-is-auth-container">
                                <div className="layout-mobile-signup-btn-container" onClick={resetAsideVisible}>
                                    <button className="layout-mobile-signup-btn" onClick={() => navigate('/auth/signup')}>Sign Up</button>
                                </div>
                                <div className="layout-mobile-signin-btn-container" onClick={resetAsideVisible}>
                                    <button className="layout-mobile-signin-btn" onClick={() => navigate('/auth/login')}>Sign In</button>
                                </div>
                            </div>
                    )}
                </div>
            </div>
        );
    }

    const DesktopHeader = () => {
        return (
            <div>
                <div className="layout-header-links">
                    <Link to="/">FILECRYPT</Link>
                    <Link to="/about">About</Link>
                    <Link to="/policy">Policy</Link>
                </div>
                <div className="layout-head-center">
                    <div className="layout-find-container">
                        <input className={inputError ? 'find-input error' : 'find-input'}
                            type="text" id="user"
                            required={true}
                            value={inputValue}
                            onChange={(e) => setInputValue(e.target.value)}
                            placeholder="#2921"
                        />
                        <button className="layout-find-btn" onClick={findUser}><Icon icon={'search'} /></button>
                    </div>
                    {isAuth && (
                        <div className="layout-notification-container">
                            <Link to="/notifications"><Icon icon={'notifications'} /></Link>
                        </div>
                    )}
                </div>
                <div className="layout-header-auth-btn-container">
                    {isAuth ? (
                        <div className="layout-header-signout-btn-container">
                            <button className="layout-header-signout-btn" onClick={logoutMe}>Sign Out</button>
                        </div>
                    ) : (
                            <div className="layout-header-is-auth-container">
                                <div className="layout-header-signup-btn-container">
                                    <button className="layout-header-signup-btn" onClick={() => navigate('/auth/signup')}>Sign Up</button>
                                </div>
                                <div className="layout-header-signin-btn-container">
                                    <button className="layout-header-signin-btn" onClick={() => navigate('/auth/login')}>Sign In</button>
                                </div>
                            </div>
                    )}
                </div>
            </div>
        );
    }

    useEffect(() => {
        checkCookies();
        setPath(`/user/${id}`);
    }, [isAuth, navigate]);

    return (
        <div className="layout-container">
            {isAuth && isAsideVisible && isDesktop && (
                <aside className="layout-desktop-sidebar" style={{ width: isAsideVisible ? "115px" : 0 }}>
                    <nav>
                        <DesktopSidebar />
                    </nav>
                </aside>
            )}
            {isAsideVisible && !isDesktop && (
                <nav>
                    <MobileSidebar />
                </nav>
            )}
            <div className="layout-header-outlet-container">
                <header className="layout-header-container">
                    <div className="layout-aside-visible-container">
                        {!isAuth && isDesktop ? (
                            <>
                            </>
                        ) : (
                                <button className="layout-aside-visible-btn" onClick={() => setAsideVisible(!isAsideVisible)}>
                                    <i className="layout-material-icons-outlined" style={{ background: 'transparent' }}>menu</i>
                                </button>
                        )}
                    </div>
                    <nav>
                        <DesktopHeader />
                    </nav>
                </header>
                {isAsideVisible && !isDesktop ? (
                    <>
                    </>
                ) : (
                        <div className="layout-outlet" style={{ marginLeft: isAsideVisible && isDesktop && isAuth ? '150px' : 0 }}>
                            <Outlet />
                        </div>
                )}
            </div>
        </div>
    );
}

export default Layout;
