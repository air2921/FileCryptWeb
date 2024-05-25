import React from "react";
import { FileProps } from "../../utils/api/Files";
import { dateFormate } from "../../utils/helpers/Formatter";

function File({ file }: { file: FileProps }) {
    return (
        <div className="entity-file-container">
            <div className="">File: {file.file_name}#{file.file_id}</div>
            <div>Creator: #{file.user_id}</div>
            <div> Created At: {dateFormate(file.operation_date)}</div>
            <div>
                <div>Mime Category: {file.file_mime_category}</div>
                <div>Content Type: {file.file_mime}</div>
            </div>
        </div>
    );
}

export default File;