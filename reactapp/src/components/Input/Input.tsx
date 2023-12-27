import React, { ChangeEvent } from 'react';

interface InputProps {
    type: string,
    id: string,
    value: string,
    onChange: (e: ChangeEvent<HTMLInputElement>) => void,
    className?: string
}

function Input({ type, id, value, onChange, className }: InputProps) {
    return (
        <input
            type={type}
            id={id}
            value={value}
            onChange={onChange}
            className={`form-control ${className}`}
            required
        />
    );
}

export default Input;