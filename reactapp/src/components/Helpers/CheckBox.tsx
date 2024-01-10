import React, { ChangeEvent } from 'react';

interface CheckBoxProps {
    type: string,
    checked: boolean,
    id: string,
    onChange: (event: ChangeEvent<HTMLInputElement>) => void,
    text?: string
}

function CheckBox({ type, id, checked, onChange, text }: CheckBoxProps) {
    return (
        <div>{text}
            <label htmlFor={id}>
                <input
                    type={type}
                    id={id}
                    checked={checked}
                    onChange={onChange}
                />
            </label>
        </div>
    );
}

export default CheckBox;