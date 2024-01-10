import React from 'react';

function Font({ font }: FontProps) {
    return (
        <i className="material-icons-outlined">{font}</i>
    );
}

export default Font;