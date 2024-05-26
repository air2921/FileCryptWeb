import React from "react";

interface SortProps {
    orderBy: string;
    onOrderByChange: (orderby: string) => void;
    closeStatus?: string;
    onCloseStatusChange: (status?: string) => void;
    createStatus?: string;
    onCreateStatus: (status?: string) => void;
    type?: string;
    onTypeChange: (type?: string) => void;
}

function OfferSort(props: SortProps) {
    return (
        <div>
            <div>
                <details>
                    <summary>
                        <span></span>
                    </summary>
                    <select>
                        <option></option>
                    </select>
                </details>
            </div>
        </div>
    );
}

export default OfferSort;