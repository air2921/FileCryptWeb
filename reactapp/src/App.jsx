import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Register from './pages/auth/registration/Register';
import Login from './pages/auth/login/Login';
import Home from './pages/static/main/Home';
import About from './pages/static/main/About';
import Policy from './pages/static/main/Policy'
import User from './pages/user/User';
import Settings from './pages/user/Settings'
import Files from './pages/list/files/Files';
import RecoveryAccount from './pages/auth/recovery/RecoveryAccount'
import ErrorPage from './pages/static/error-status/ErrorPage'
import Layout from './components/Layout/Layout';
import useAuth from './components/hooks/useAuth';

function App() {
    const isAuth = useAuth();

    if (isAuth === null) {
        return (
            <>
            </>
        )
    }

    return (
        <>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="about" element={<About />} />
                    <Route path="policy" element={<Policy />} />
                    {isAuth ? (
                        <>
                            <Route path="user/:userId" element={<User />} />
                            <Route path="settings" element={<Settings />} />
                            <Route path="files" element={<Files />} />
                        </>
                    ) : (
                            <Route path="*" element={<ErrorPage statusCode={401} message={'Unauthorized'} />} />
                    )}
                    {!isAuth && (
                        <>
                            <Route path="auth/login" element={<Login />} />
                            <Route path="auth/signup" element={<Register />} />
                            <Route path="auth/recovery" element={<RecoveryAccount />} />
                        </>
                    )}
                    <Route path="*" element={<ErrorPage statusCode={404} message={'Not Found'} />} />
                </Route>
            </Routes>
        </>
    );
}

export default App;