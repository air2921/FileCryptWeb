import React from "react";
import { StorageProps } from "../../../utils/api/Storages";
import { dateFormate, lineFormate } from "../../../utils/helpers/Formatter";

function Storage({ storage }: { storage: StorageProps }) {
    return (
        <div className="entity-storage-container">
            <div>{storage.storage_name}#{storage.storage_id}</div>
            <div>Creator: {storage.user_id}</div>
            {storage.description && (
                <div>
                    <div>Desription</div>
                    {lineFormate(storage.description)}
                </div>
            )}
            <div>Last time modified: {dateFormate(storage.last_time_modified)}</div>
        </div>
    );
}

export default Storage;