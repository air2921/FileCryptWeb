import React from 'react';
import './Modal.css'

interface ModalProps {
    isActive: boolean,
    setActive: any,
    children: React.ReactNode
}

function Modal({ isActive, setActive, children }: ModalProps) {
    return (
        <div className={isActive ? "modal active" : "modal"} onClick={() => setActive(false)}>
            <div className={isActive ? "modal-content active" : "modal-content"} onClick={e => e.stopPropagation()}>
                {children}
            </div>
        </div>
    );
}

export default Modal;