import React, { ChangeEvent } from 'react';
import Button from './Button';
import Font from '../Font/Font';

interface FileButtonProps {
    id: string,
    font: string,
    onChange: (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string) => void,
    fileType: string,
    operationType: string
}

function FileButton({ id, font, onChange, fileType, operationType }: FileButtonProps) {

    const clickElement = (elementId: string) => {
        document.getElementById(elementId)?.click();
    };

    return (
        <div>
            <input
                type="file"
                id={id}
                style={{ display: "none" }}
                required={true}
                onChange={(event) => onChange(event, fileType, operationType)}
            />
            <Button onClick={() => clickElement(id)}>
                <Font font={font} />
            </Button>
        </div>
    );
}

export default FileButton;