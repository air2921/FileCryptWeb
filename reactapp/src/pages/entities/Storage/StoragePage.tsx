import React, { useState, useEffect } from "react";
import DateSort from "../../../components/entities/DateSort";
import { StorageProps, deleteStorage, getStorages } from "../../../utils/api/Storages";
import Storage from '../../../components/entities/storages/Storage'
import NoState from "../../../components/noState/NoState";
import ErrorPage from "../../../components/widgets/error-status/ErrorPage";
import Loader from "../../../components/widgets/loader/Loader";
import { DeleteStorage } from "../../../components/widgets/storage-access/StorageAccess";
import Paginator from "../../../components/widgets/paginator/Paginator";

function StoragePage() {
    const count = 10;
    const [skip, setSkip] = useState(0);
    const [orderByDesc, setOrderBy] = useState('true');

    const [storages, setStorages] = useState<StorageProps[] | null>();
    const [errMess, setErrMess] = useState('');
    const [status, setStatus] = useState(500);

    function setOrderByState(state: string) {
        setOrderBy(state);
    }

    async function fetchStorages() {
        const response = await getStorages({ skip, count, orderByDesc });
        if (response.success) {
            setStorages(response.data.storages);
        } else {
            setErrMess(response.message!);
        }
        setStatus(response.statusCode);
    }

    async function deleteEntity(id: number, code: number) {
        const response = await deleteStorage(id, code);
        if (response.success) {

        } else {

        }
    }

    useEffect(() => {
        fetchStorages();
    }, [skip, orderByDesc]);

    if (errMess) {
        return <ErrorPage statusCode={status} message={errMess} />
    } else if (!errMess && !storages) {
        return <Loader />
    } else {

        if (!storages || storages.every(s => s === null)) {
            return <NoState />
        }

        return (
            <div className="storages-page-container">
                <DateSort orderBy={orderByDesc} onOrderByChange={setOrderByState} />
                {storages.map(storage => (
                    <div key={storage.storage_id}>
                        <Storage storage={storage} />
                        <DeleteStorage storageId={storage.storage_id} deleteStorage={deleteEntity} />
                    </div>
                ))}
                <Paginator count={count} currentSkip={skip} onSkipChange={setSkip} />
            </div>
        );
    }
}

export default StoragePage;