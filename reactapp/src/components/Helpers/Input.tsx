import React, { ChangeEvent } from 'react';

interface InputProps {
    type: string,
    id: string,
    value?: any,
    onChange: (e: ChangeEvent<HTMLInputElement>) => void,
    require: boolean,
    text?: string
}

function Input({ type, id, value, onChange, require, text }: InputProps) {
    return (
        <div className="form-group">
            <p>{text}</p>
            <label htmlFor={id}>
                <input
                    type={type}
                    id={id}
                    value={value}
                    required={require}
                    onChange={onChange}
                />
            </label>
        </div>
    );
}

export default Input;