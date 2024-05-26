import React from "react";
import DateSort, { DateSortProps } from "../DateSort";

interface SortProps extends DateSortProps {
    priority?: string;
    onPriorytyChange: (priority?: string) => void;
    statusChecked?: string;
    onStatusChange: (status?: string) => void;
}

function NotificationSort(props: SortProps) {
    return (
        <div>
            <DateSort orderBy={props.orderBy} onOrderByChange={props.onOrderByChange} />
            <div>
                <details>
                    <summary>
                        <span>Select Priority</span>
                    </summary>
                    <select
                        className="sort-entity-notification-priority"
                        required={true}
                        value={props.priority}
                        onChange={(e) => props.onPriorytyChange(e.target.value)}>
                        <option value="">All</option>
                        <option value="101">Info</option>
                        <option value="102">Offer</option>
                        <option value="301">Warning</option>
                        <option value="302">Security</option>
                    </select>
                </details>
            </div>
            <div>
                <details>
                    <summary>
                        <span>Select Check Status</span>
                    </summary>
                    <select
                        className="sort-entity-notification-status"
                        required={true}
                        value={props.statusChecked}
                        onChange={(e) => props.onStatusChange(e.target.value)}>
                        <option value="true">Checked</option>
                        <option value="false">None Checked</option>
                    </select>
                </details>
            </div>
        </div>
    );
}

export default NotificationSort;