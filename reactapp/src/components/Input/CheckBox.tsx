import React, { ChangeEvent } from 'react';

interface CheckBoxProps {
    type: string,
    checked: boolean,
    id: string,
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
}

function CheckBox({ type, id, checked, onChange }: CheckBoxProps) {
    return (
        <label htmlFor={id}>
            <input
                type={type}
                id={id}
                checked={checked}
                onChange={onChange}
            />
        </label>
    );
}

export default CheckBox;