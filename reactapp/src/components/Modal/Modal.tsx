import React from 'react';

function Modal({ isActive, setActive, children }: ModalProps) {
    return (
        <div className={isActive ? "modal-active" : "modal"}>
            <div className={isActive ? "modal-content-active" : "modal-content"}>
                {children}
            </div>
        </div>
    );
}

export default Modal;