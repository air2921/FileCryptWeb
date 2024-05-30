import React from "react";
import Icon from "../widgets/icon/Icon";

function NoState() {
    return (
        <div className="no-state-container">
            <Icon icon={'storage'} height={32} width={32} />
            <div className="message-container">
                <div className="message">Here's empty for now</div>
            </div>
        </div>
    );
}

export default NoState;