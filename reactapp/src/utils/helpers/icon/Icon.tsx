import React from 'react';

function Icon({ icon }: { icon: string }) {
    return (
        <i className="material-icons-outlined" style={{ background: 'transparent' }}>{icon}</i>
    );
}

export default Icon;