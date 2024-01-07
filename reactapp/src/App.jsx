import React, { Profiler } from 'react';
import { Routes, Route, Link } from 'react-router-dom';

import Register from './pages/Register';
import Login from './pages/Login'
import Home from './pages/no_logic/Home';
import About from './pages/no_logic/About';
import User from './pages/User';
import UserSettings from './pages/UserSettings'
import KeySetting from './pages/KeySettings'
import Offers from './pages/Offers'
import Files from './pages/Files'

function App() {
    return (
        <div>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/about" element={<About />} />
                <Route path="/user/:userId/:username" element={<User />} />
                <Route path="/settings" element={<UserSettings />} />
                <Route path="/settings/keys" element={<KeySetting />} />
                <Route path="/offers" element={<Offers />} />
                <Route path="/files" element={<Files />} />
                <Route path="/auth/signin" element={<Login />} />
                <Route path="/auth/signup" element={ <Register /> } />
            </Routes>
        </div>
    );
}

export default App;