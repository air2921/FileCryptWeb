import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Register from './pages/auth/registration/Register';
import Login from './pages/auth/login/Login';
import Home from './pages/no_logic/Home';
import About from './pages/no_logic/About';
import Policy from './pages/no_logic/Policy'
import User from './pages/user/profile/User';
import Settings from './pages/user/Settings'
import Offers from './pages/list/offers/Offers';
import Files from './pages/list/files/Files';
import Api from './pages/list/api/Api'
import Notifications from './pages/list/notifications/Notifications';
import RecoveryAccount from './pages/auth/recovery/RecoveryAccount'
import ErrorPage from './pages/no_logic/ErrorPage/ErrorPage'
import Layout from './components/Layout/Layout';
import useAuth from './components/UseAuth/UseAuth';

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
                            <Route path="user/:userId/:username" element={<User />} />
                            <Route path="settings" element={<Settings />} />
                            <Route path="offers" element={<Offers />} />
                            <Route path="files" element={<Files />} />
                            <Route path="notifications" element={<Notifications />} />
                            <Route path="api" element={<Api />} />
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