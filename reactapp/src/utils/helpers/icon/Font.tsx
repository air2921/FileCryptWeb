import React from 'react';

function Font({ font }: FontProps) {
    return (
        <i className="material-icons-outlined" style={{ background: 'transparent' }}>{font}</i>
    );
}

export default Font;