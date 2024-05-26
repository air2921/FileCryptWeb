import React from "react";
import DateSort, { DateSortProps } from "../DateSort";

interface SortProps extends DateSortProps {
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
            <DateSort orderBy={props.orderBy} onOrderByChange={props.onOrderByChange} />
            <div>
                <details>
                    <summary>
                        <span>Select Offer Status</span>
                    </summary>
                    <select
                        className="sort-entity-offer-close-status"
                        required={true}
                        value={props.closeStatus}
                        onChange={(e) => props.onCloseStatusChange(e.target.value)}>
                        <option value="">All</option>
                        <option value="true">Only closed</option>
                        <option value="false">Only opened</option>
                    </select>
                </details>
            </div>
            <div>
                <details>
                    <summary>
                        <span>Select Sending Status</span>
                    </summary>
                    <select
                        className="sort-entity-offer-sending-status"
                        required={true}
                        value={props.createStatus}
                        onChange={(e) => props.onCreateStatus(e.target.value)}>
                        <option value="">All</option>
                        <option value="true">Only sent</option>
                        <option value="false">Only received</option>
                    </select>
                </details>
            </div>
            <div>
                <details>
                    <summary>
                        <span>Select Type</span>
                    </summary>
                    <select
                        className="sort-entity-offer-type"
                        required={true}
                        value={props.type}
                        onChange={(e) => props.onTypeChange(e.target.value)}>
                        <option value="">All</option>
                        <option value="101">Key</option>
                    </select>
                </details>
            </div>
        </div>
    );
}

export default OfferSort;