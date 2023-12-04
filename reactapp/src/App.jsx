import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';

import Register from './components/forms/Register';

function App() {
    return (
        <div>
            <Routes>
                {/*<Route path="/" element={ } />*/}
                {/*<Route path="/auth/signin" element={ } />*/}
                <Route path="/auth/signup" element={ <Register /> } />
            </Routes>
        </div>
    );
}

export default App;