import React, { ReactNode } from 'react';

interface ButtonProps {
    onClick?: () => void,
    children: ReactNode,
}

function Button({ onClick, children }: ButtonProps) {
    return (
        <button onClick={() => onClick && onClick()}type="submit" className="btn btn-primary">
            {children}
        </button>
    );
}

export default Button;