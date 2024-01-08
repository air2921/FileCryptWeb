import React from 'react';

function Font({ font }: FontProps) {
    return (
        <i className="material-icons-sharp">{font}</i>
    );
}

export default Font;