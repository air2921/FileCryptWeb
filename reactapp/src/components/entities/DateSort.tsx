import React from "react";

export interface DateSortProps {
    orderBy: string;
    onOrderByChange: (orderby: string) => void;
}

function DateSort({ orderBy, onOrderByChange }: DateSortProps) {
    return (
        <div>
            <details>
                <summary>
                    <span>Select Order</span>
                </summary>
                <select
                    className="sort-entity-order"
                    required={true}
                    value={orderBy}
                    onChange={(e) => onOrderByChange(e.target.value)}>
                    <option value="true">Descending (Time)</option>
                    <option value="false">Ascending (Time)</option>
                </select>
            </details>
        </div>
    );
}

export default DateSort;