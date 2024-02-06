import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Register from './pages/auth/Registration/Register';
import Login from './pages/auth/Login/Login';
import Home from './pages/no_logic/Home';
import About from './pages/no_logic/About';
import Policy from './pages/no_logic/Policy'
import User from './pages/User/User';
import Settings from './pages/User/Settings/Settings';
import Offers from './pages/List/Offers/Offers';
import Files from './pages/List/Files/Files';
import Api from './pages/List/Api/Api'
import Notifications from './pages/List/Notifications/Notifications';
import RecoveryAccount from './pages/auth/Recovery/RecoveryAccount'
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