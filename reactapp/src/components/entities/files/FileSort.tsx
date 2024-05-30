import React from "react";
import DateSort, { DateSortProps } from "../DateSort";

interface SortProps extends DateSortProps {
    category?: string;
    onCategoryChange: (status?: string) => void;
}

function FileSort(props: SortProps) {
    return (
        <div>
            <DateSort orderBy={props.orderBy} onOrderByChange={props.onOrderByChange} />
            <div>
                <details>
                    <summary>
                        <span>Select Category</span>
                    </summary>
                    <select
                        className="sort-entity-file-category"
                        required={true}
                        value={props.category}
                        onChange={(e) => props.onCategoryChange(e.target.value)}>
                        <option value="">All</option>
                        <option value="application">Application</option>
                        <option value="audio">Audio</option>
                        <option value="font">Font</option>
                        <option value="image">Image</option>
                        <option value="message">Message</option>
                        <option value="model">Model</option>
                        <option value="multipart">Multipart</option>
                        <option value="text">Text</option>
                        <option value="video">Video</option>
                    </select>
                </details>
            </div>
        </div>
    );
}

export default FileSort;