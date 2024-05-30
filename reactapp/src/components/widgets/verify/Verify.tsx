import React, { useState, useRef } from "react";
import './Verify.css'

interface VerifyProps {
    length: number;
    onChange: (code: string) => void;
}

function Verify({ length, onChange }: VerifyProps) {
    const [code, setCode] = useState<string[]>(Array(length).fill(''));
    const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

    const handleChange = (value: string, index: number) => {
        if (value.length <= 1 && /^\d*$/.test(value)) {
            const newCode = [...code];
            newCode[index] = value;
            setCode(newCode);
            onChange(newCode.join(''));

            if (value && index < length - 1) {
                inputRefs.current[index + 1]?.focus();
            }
        }
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>, index: number) => {
        if (e.key === 'Backspace' && !code[index] && index > 0) {
            inputRefs.current[index - 1]?.focus();
        }
    };

    return (
        <div className="verify-code-container">
            {code.map((digit, index) => (
                <input
                    className="verify-code-input"
                    key={index}
                    type="text"
                    value={digit}
                    onChange={(e) => handleChange(e.target.value, index)}
                    onKeyDown={(e) => handleKeyDown(e, index)}
                    maxLength={1}
                    ref={(el) => (inputRefs.current[index] = el)}
                />
            ))}
        </div>
    );
}

export default Verify;