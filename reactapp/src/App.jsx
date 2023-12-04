import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';

import Register from './components/forms/Register';

function App() {
    return (
        <>
            <header>
            <Link to="/">FileCryptWeb</Link>
            <Link to="/auth/signin">Sign In</Link>
            <Link to="/auth/signup">Sign Up</Link>
            </header>
            <Routes>
                {/*<Route path="/" element={ } />*/}
                {/*<Route path="/auth/signin" element={ } />*/}
                <Route path="/auth/signup" element={ <Register /> } />
            </Routes>
        </>
    );
}

export default App;