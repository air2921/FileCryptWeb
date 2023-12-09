import React, { Profiler } from 'react';
import { Routes, Route, Link } from 'react-router-dom';

import Register from './pages/Register';
import Login from './pages/Login'
import Home from './pages/Home';
import About from './pages/About';
import User from './pages/User';

function App() {
    return (
        <div>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/about" element={<About />} />
                <Route path="/user/:userId" element={<User />} />
                <Route path="/auth/signin" element={<Login />} />
                <Route path="/auth/signup" element={ <Register /> } />
            </Routes>
        </div>
    );
}

export default App;