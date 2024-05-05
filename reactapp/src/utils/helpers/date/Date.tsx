import React from 'react';
import { format } from 'date-fns';

function DateComponent({ date }: { date: string }) {

    const originalDate = new Date(date);
    const formattedDate = format(originalDate, "dd.MM.yyyy 'at' h:mm:ss a");

    return (
        <div>
            {formattedDate}
        </div>
    );
}

export default DateComponent;