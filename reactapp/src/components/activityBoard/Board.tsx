import React, { useEffect, useState } from "react";
import './Board.css';

export interface ActivityProps {
    Date: string,
    ActivityCount: number
}

function Board({ days, year }: { days: ActivityProps[], year: number }) {
    const [activity, setActivity] = useState<ActivityProps[]>(Array());
    const [isLeap, setLeap] = useState(false);

    function setColor(count: number): string {
        let colorHex;

        if (count === 0) {
            colorHex = '#808080';
        } else if (count > 0 && count <= 10) {
            colorHex = '#98FB98';
        } else if (count > 10 && count <= 25) {
            colorHex = '#32CD32';
        } else if (count > 25 && count <= 50) {
            colorHex = '#00FF00';
        } else {
            colorHex = '#7FFF00';
        }

        return colorHex;
    }

    function getAllDatesInYear(year: number): string[] {
        const dates: string[] = [];
        let currentDate = new Date(year, 0, 1);
        const endDate = new Date(year + 1, 0, 0);

        while (currentDate <= endDate) {
            const dd = String(currentDate.getDate()).padStart(2, '0');
            const mm = String(currentDate.getMonth() + 1).padStart(2, '0');
            const yyyy = currentDate.getFullYear();

            const dateStr = `${dd}.${mm}.${yyyy}`;
            dates.push(dateStr);

            currentDate.setDate(currentDate.getDate() + 1);
        }

        if (dates.length === 366) {
            setLeap(true);
        }
        return dates;
    }

    function setFullYearActivity(props: ActivityProps[], year: number): void {
        const existingDates = new Set(props.map(prop => prop.Date));

        const arr = getAllDatesInYear(year);
        arr.forEach(date => {
            if (!existingDates.has(date)) {
                props.push({ Date: date, ActivityCount: 0 });
            }
        });

        props.sort((a, b) => {
            const dateA = new Date(a.Date.split('.').reverse().join('-')).getTime();
            const dateB = new Date(b.Date.split('.').reverse().join('-')).getTime();
            return dateA - dateB;
        });

        console.log(props);
        setActivity(props);
    }

    function setNextStep(step: number, isLeap: boolean): number {
        const defaultStep = 52;
        let skip = 0;

        for (let i = 0; i < step; i++) {
            skip += defaultStep;
        }

        return skip;
    }

    function setStep(step: number, isLeap: boolean): number {
        if (step === 5 && isLeap) {
            return 53;
        } else if (step === 6) {
            return 53;
        } else {
            return 52;
        }
    }

    useEffect(() => {
        setFullYearActivity(days, year);
    }, [days, year])

    const BlockComponent = ({ count }: { count: number }) => {
        return (
            <div className="day-block" style={{ backgroundColor: setColor(count) }}>

            </div>
        );
    }

    return (
        <div className="activity-board">
            <table>
                <tbody>
                    {[0, 1, 2, 3, 4, 5, 6].map((row, index) => (
                        <tr key={index + 1} className={`${index + 1}-days-line`}>
                            {activity.slice(setNextStep(index, isLeap), setNextStep(index, isLeap) + setStep(index, isLeap)).map(day => (
                                <td key={day.Date}>
                                    <BlockComponent count={day.ActivityCount} />
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default Board;