import React from "react";
import { KeyProps } from "../../../utils/api/Keys";
import { dateFormate } from "../../../utils/helpers/Formatter";

function Key({ key }: { key: KeyProps }) {
    return (
        <div className="entity-key-container">
            <div>{key.key_name}#{key.key_id}</div>
            <div>Value: {key.key_value}</div>
            <div>Created At: {dateFormate(key.created_at)}</div>
            <div>Refers to storage#{key.storage_id}</div>
        </div>
    );
}

export default Key;