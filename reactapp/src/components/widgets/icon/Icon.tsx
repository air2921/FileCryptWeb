import React from "react";

export interface IconProps {
    icon: string;
    width: number;
    height: number;
}

const Icon = ({ icon, width, height }: IconProps) => {
    const iconPath = `../../../../public/icons/${icon}.svg`;

    return (
        <div>
            <img src={iconPath} alt={icon} style={{ width: `${width}px`, height: `${height}px` }} />
        </div>
    );
}

export default Icon;