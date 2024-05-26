import React from "react";

interface SortProps {
    priority?: string;
    onPriorytyChange: (priority?: string) => void;
    statusChecked?: string;
    onStatusChange: (status?: string) => void;
    orderBy: string;
    onOrderByChange: (orderby: string) => void;
}

function NotificationSort(props: SortProps) {
    return (
        <div>
            <div>
                <details>
                    <summary>
                        <span>Select Order</span>
                    </summary>
                    <select
                        className="sort-entity-notification-order"
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