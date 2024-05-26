import React from "react";

interface SortProps {
    category?: string;
    onCategoryChange: (status?: string) => void;
    orderBy: string;
    onOrderByChange: (orderby: string) => void;
}

function FileSort(props: SortProps) {
    return (
        <div>
            <div>
                <details>
                    <summary>
                        <span>Select Order</span>
                    </summary>
                    <select
                        className="sort-entity-file-order"
                        required={true}
                        value={props.orderBy}
                        onChange={(e) => props.onOrderByChange(e.target.value)}>
                        <option value="true">Descending (Time)</option>
                        <option value="false">Ascending (Time)</option>
                    </select>
                </details>
            </div>
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