import React, { Profiler } from 'react';
import { Routes, Route, Link } from 'react-router-dom';

import Register from './pages/Register';
import Home from './pages/Home';
import User from './pages/User';

function App() {
    return (
        <div>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/user/:userId" element={< User />} />
                <Route path="/auth/signup" element={ <Register /> } />
            </Routes>
        </div>
    );
}

export default App;