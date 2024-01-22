import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Register from './pages/Register';
import Login from './pages/Login';
import Home from './pages/no_logic/Home';
import About from './pages/no_logic/About';
import User from './pages/User';
import UserSettings from './pages/UserSettings';
import KeySetting from './pages/KeySettings';
import Offers from './pages/Offers';
import Files from './pages/Files';
import Notifications from './pages/Notifications';
import NotFound from './pages/no_logic/NotFound/NotFound'
import Layout from './components/Layout/Layout';

function App() {
    return (
        <>
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<Home />} />
                    <Route path="about" element={<About />} />
                    <Route path="user/:userId/:username" element={<User />} />
                    <Route path="settings" element={<UserSettings />} />
                    <Route path="settings/keys" element={<KeySetting />} />
                    <Route path="offers" element={<Offers />} />
                    <Route path="files" element={<Files />} />
                    <Route path="notifications" element={<Notifications />} />
                    <Route path="auth/signin" element={<Login />} />
                    <Route path="auth/signup" element={<Register />} />
                    <Route path="*" element={<NotFound />} />
                </Route>
            </Routes>
        </>
    );
}

export default App;