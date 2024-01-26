import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Register from './pages/auth/Registration/Register';
import Login from './pages/auth/Login/Login';
import Home from './pages/no_logic/Home';
import About from './pages/no_logic/About';
import Policy from './pages/no_logic/Policy'
import User from './pages/User/User';
import UserSettings from './pages/User/Settings/UserSettings';
import KeySetting from './pages/User/Settings/KeySettings';
import Offers from './pages/List/Offers/Offers';
import Files from './pages/List/Files/Files';
import Notifications from './pages/List/Notifications/Notifications';
import RecoveryAccount from './pages/auth/Recovery/RecoveryAccount'
import NotFound from './pages/no_logic/NotFound/NotFound'
import Layout from './components/Layout/Layout';

function App() {
    return (
        <>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="about" element={<About />} />
                    <Route path="policy" element={<Policy/>} />
                    <Route path="user/:userId/:username" element={<User />} />
                    <Route path="settings" element={<UserSettings />} />
                    <Route path="settings/keys" element={<KeySetting />} />
                    <Route path="offers" element={<Offers />} />
                    <Route path="files" element={<Files />} />
                    <Route path="notifications" element={<Notifications />} />

                    <Route path="auth/login" element={<Login />} />
                    <Route path="auth/signup" element={<Register />} />
                    <Route path="auth/recovery" element={<RecoveryAccount />} />
                    <Route path="*" element={<NotFound />} />
                </Route>
            </Routes>
        </>
    );
}

export default App;