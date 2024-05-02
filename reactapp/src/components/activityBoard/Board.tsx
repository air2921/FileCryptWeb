import React from "react";

export interface DayProps {
    Date: string,
    ActivityCount: number,
    NumberOfDay: number
}

function Board(days: DayProps[]) {

    const setColor = (count: number) => {
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

    return (
        <>
            <div className="activity-board">
                <div>
                    <ul>
                        {days.map(day => (
                            <li key={day.NumberOfDay}>
                                <div className="day-block" style={{ color: setColor(day.ActivityCount) }}>

                                </div>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>
        </>
    );
}

export default Board;