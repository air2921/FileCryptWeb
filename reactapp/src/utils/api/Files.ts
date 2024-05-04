import axios from 'axios';
import { BASE_URL, errorHandler } from './Helper';

export interface FilesSortProps {
    skip: number,
    count: 10,
    orderByDesc: string,
    mime?: string,
    category?: string
}

export interface FileProps {
    file_id: number,
    user_id: number,
    file_name: string,
    operation_date: string,
    file_mime: string,
    file_mime_category: string
}

export interface CypherProps {
    file: FormData,
    filename: string,
    storageId: number,
    keyId: number,
    encrypt: any, // boolean
    code: number
}

export async function getFile(fileId: number) {
    try {
        const response = await axios.get(BASE_URL + `api/core/file/${fileId}`)

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getFiles(sort: FilesSortProps) {
    try {
        const endpoint =
            `api/core/file/range?skip=${sort.skip}&count=${sort.count}&byDesc=${sort.orderByDesc}&category=${sort.category}&mime=${sort.mime}`
        const response = await axios.get(BASE_URL + endpoint, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getFilesInterval(byDesc: boolean, start: Date, end: Date) {
    try {
        const response = await axios.get(`api/core/file/activity?byDesc=${byDesc}&start=${start}&end=${end}`);

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function deleteFile(fileId: number) {
    try {
        const response = await axios.delete(BASE_URL + `api/core/file/${fileId}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function cypherFile(props: CypherProps) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/file/cypher/${props.storageId}/${props.keyId}?encrypt=${props.encrypt}&code=${props.code}`,
            props.file,
            {
                withCredentials: true,
                responseType: 'blob'
            }
        );

        downloadFile(response.data, props.filename);

        return {
            success: true
        }
    } catch (error: any) {
        try {
            const errorText = await error.response.data.text();
            const errorJson = JSON.parse(errorText);

            return {
                success: false,
                message: errorJson.message ? errorJson.message : 'Unexpected error',
                statusCode: error.response.status
            }
        } catch (e) {
            return {
                success: false,
                message: 'Unexpected error',
                statusCode: error.response.status
            }
        }
    }
}

function downloadFile(file: Blob, filename: string) {
    const fileURL = URL.createObjectURL(new Blob([file], { type: file.type }));

    const downloadLink = document.createElement('a');
    downloadLink.href = fileURL;
    downloadLink.setAttribute('download', filename);

    document.body.appendChild(downloadLink);
    downloadLink.click();

    document.body.removeChild(downloadLink);
    URL.revokeObjectURL(fileURL);
}
