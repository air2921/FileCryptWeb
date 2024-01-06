import React, { ChangeEvent } from 'react';

interface InputProps {
    type: string,
    id: string,
    value?: any,
    onChange: (e: ChangeEvent<HTMLInputElement>) => void,
    require: boolean,
    className?: string,
    text: string
}

function Input({ type, id, value, onChange, require, className, text }: InputProps) {
    return (
        <div className="form-group">
            <p>{text}</p>
            <label htmlFor={id}>
                <input
                    type={type}
                    id={id}
                    value={value}
                    onChange={onChange}
                    className={`form-control ${className}`}
                    required={require} 
                />
            </label>
        </div>
    );
}

export default Input;