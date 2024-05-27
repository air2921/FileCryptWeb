import React, { useEffect, useState } from 'react';
import { Link, Outlet, useNavigate } from 'react-router-dom';
import cookie from 'react-cookies'
import useResize from '../hooks/useResize';
import useAuth from '../hooks/useAuth';
import { getAuth, logout } from '../../utils/api/Auth';
import './Layout.css'
import Icon from '../widgets/icon/Icon';

interface LinkProps {
    icon: string,
    name: string,
    path: string,
    id: string
}

function Layout() {
    const [id, setId] = useState(cookie.load('auth_user_id'));
    const [role, setRole] = useState(cookie.load('auth_role'));
    const [profilePath, setPath] = useState('');
    const [isAsideVisible, setAsideVisible] = useState(sessionStorage.getItem('isAsideVisible') === 'true');

    const authLinks: LinkProps[] = setAuthLinks();
    const defaultLinks: LinkProps[] = setDefaultLinks();
    const isAuth = useAuth();
    const isDesktop = useResize();
    const navigate = useNavigate();

    function setAuthLinks(): LinkProps[] {
        const authLinks: LinkProps[] = new Array();

        authLinks.push({ icon: 'profile', name: 'Profile', path: profilePath, id: 'profile' });
        authLinks.push({ icon: 'settings', name: 'Settings', path: "/settings", id: 'settings' });
        authLinks.push({ icon: 'users', name: 'Users', path: "/users/all", id: 'users' });
        authLinks.push({ icon: 'notification', name: 'Notifications', path: "/inbox", id: 'inbox' });
        authLinks.push({ icon: 'storage', name: 'Storages', path: "/storages", id: 'storages' });
        authLinks.push({ icon: 'folder-lock', name: 'Files', path: "/files", id: 'files' });
        authLinks.push({ icon: 'trade', name: 'Offers', path: "/offers/hub", id: 'offers' });
        authLinks.push({ icon: 'terminal', name: 'Admin Panel', path: "/admin", id: 'admin' });

        return authLinks;
    }

    function setDefaultLinks(): LinkProps[] {
        const defaultLinks: LinkProps[] = new Array();

        defaultLinks.push({ icon: 'home', name: 'Home', path: "/", id: 'home' });
        defaultLinks.push({ icon: 'about', name: 'About', path: "/about", id: 'about' });
        defaultLinks.push({ icon: 'policy', name: 'Policy', path: "/policy", id: 'policy' });

        return defaultLinks;
    }

    const getAuthStatus = async () => {
        const response = await getAuth()

        if (response.success) {
            return true;
        } else {
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
                    {authLinks.map(link => (
                        <>
                            {link.id === 'admin' ? (
                                <>
                                    {(role === 'Admin' || role === 'HighestAdmin') && (
                                        <div className="layout-desktop-link">
                                            <Link to={link.path}>
                                                <Icon icon={link.icon} height={36} width={36} />
                                                <h4>{link.name}</h4>
                                            </Link>
                                        </div>
                                    )}
                                </>
                            ) : (
                                    <div className="layout-desktop-link">
                                        <Link to={link.path}>
                                            <Icon icon={link.icon} height={36} width={36} />
                                            <h4>{link.name}</h4>
                                        </Link>
                                    </div>
                            )}
                        </>
                    ))}
                </div>
            </>
        );
    }

    const MobileSidebar = () => {
        return (
            <div className="layout-mobile-menu" onClick={() => setAsideVisible(false)}>
                <div className="layout-mobile-links-container">
                    {isAuth && (
                        <>
                            {authLinks.map(link => (
                                <>
                                    {link.id === 'admin' ? (
                                        <>
                                            {(role === 'Admin' || role === 'HighestAdmin') && (
                                                <div className="layout-mobile-link">
                                                    <Link to={link.path}>
                                                        <div className="layout-mobile-link-content">
                                                            <div className="layout-mobile-icon">
                                                                <Icon icon={link.icon} height={36} width={36} />
                                                            </div>
                                                            <div className="layout-mobile-name">
                                                                {link.name}
                                                            </div>
                                                        </div>
                                                    </Link>
                                                </div>
                                            )}
                                        </>
                                    ) : (
                                            <div className="layout-mobile-link">
                                                <Link to={link.path}>
                                                    <div className="layout-mobile-link-content">
                                                        <div className="layout-mobile-icon">
                                                            <Icon icon={link.icon} height={36} width={36} />
                                                        </div>
                                                        <div className="layout-mobile-name">
                                                            {link.name}
                                                        </div>
                                                    </div>
                                                </Link>
                                            </div>
                                    )}
                                </>
                            ))}
                        </>
                    )}
                    <>
                        {defaultLinks.map(link => (
                            <div className="layout-mobile-link">
                                <Link to={link.path}>
                                    <div className="layout-mobile-link-content">
                                        <div className="layout-mobile-icon">
                                            <Icon icon={link.icon} height={36} width={36} />
                                        </div>
                                        <div className="layout-mobile-name">
                                            {link.name}
                                        </div>
                                    </div>
                                </Link>
                            </div>
                        ))}
                    </>
                    <div className="layout-mobile-auth-btn-container">
                        {isAuth ? (
                            <div className="layout-mobile-signout-btn-container">
                                <button className="layout-mobile-signout-btn" onClick={logout}>Sign Out</button>
                            </div>
                        ) : (
                                <div className="layout-mobile-is-auth-container">
                                    <div className="layout-mobile-signup-btn-container" onClick={() => setAsideVisible(false)}>
                                        <button className="layout-mobile-signup-btn" onClick={() => navigate('/auth/signup')}>Sign Up</button>
                                    </div>
                                    <div className="layout-mobile-signin-btn-container" onClick={() => setAsideVisible(false)}>
                                        <button className="layout-mobile-signin-btn" onClick={() => navigate('/auth/login')}>Sign In</button>
                                    </div>
                                </div>
                        )}
                    </div>
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
                                    <Icon icon={'menu'} height={36} width={36} />
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
