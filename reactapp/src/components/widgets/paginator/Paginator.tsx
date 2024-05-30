import React from "react";
import Icon from "../icon/Icon";
import { handleLoadMore, handleLoadLess } from "../../../utils/helpers/Pagination";

interface PaginatorProps {
    count: number;
    currentSkip: number;
    onSkipChange: (newSkip: number) => void;
}

function Paginator(props: PaginatorProps) {
    return (
        <div>
            <div>
                <button onClick={() => handleLoadLess(props.count, props.currentSkip, props.onSkipChange)}>
                    <Icon icon={'back'} height={24} width={24} />
                </button>
            </div>
            <div>
                <button onClick={() => handleLoadMore(props.count, props.currentSkip, props.onSkipChange)}>
                    <Icon icon={'forward'} height={24} width={24} />
                </button>
            </div>
        </div>
    );
}

export default Paginator;