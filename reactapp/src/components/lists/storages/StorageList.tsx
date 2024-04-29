import React from 'react';
import { StorageProps } from '../../../utils/api/Storages';

export interface StorageListProps {
    storages: StorageProps[] | null,
    isOwner: boolean,
    deleteStorage?: (offerId: number, code: number) => void,
}

function StorageList() {
    return (
        <div>

        </div>
    );
}

export default StorageList;