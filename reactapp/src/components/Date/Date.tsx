import React from 'react';
import { format } from 'date-fns';
import Font from '../Font/Font';

function DateComponent({ date }: DateProps) {

    const originalDate = new Date(date);
    const formattedDate = format(originalDate, "dd.MM.yyyy 'at' h:mm:ss a");

    return (
        <div>
            <Font font={'schedule'} /> {formattedDate}
        </div>
    );
}

export default DateComponent;