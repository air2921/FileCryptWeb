import axios from 'axios';
import { BASE_URL } from '../api/Url';
import { errorHandler } from './ErrorHandler';

export interface FilesSortProps {
    skip: number,
    count: 10,
    orderByDesc: string,
    type: string,
    mime: string,
    category: string
}

export interface FileProps {
    file_id: number,
    user_id: number,
    file_name: string,
    operation_date: string,
    type: string,
    file_mime: string,
    file_mime_category: string
}

export async function getFiles(sort: FilesSortProps) {
    try {
        const endpoint =
            `api/core/files/all?skip=${sort.skip}&count=${sort.count}&byDesc=${sort.orderByDesc}&type=${sort.type}&category=${sort.category}&mime=${sort.mime}`
        const response = await axios.get(BASE_URL + endpoint, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            message: undefined,
            data: response.data.files as FileProps[]
        }
    } catch (error: any) {
        error = errorHandler(error);

        return {
            success: error.success,
            statusCode: error.statusCode,
            message: error.message,
            data: null
        }
    }
}

export async function deleteFile(id: number) {
    try {
        const response = await axios.delete(BASE_URL + `api/core/files/${id}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function cypherFile(file: FormData, fileType: string, operationType: string, filename: string, signature: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/cryptography/${fileType}/${operationType}?validate=${signature}`,
            file,
            {
                withCredentials: true,
                responseType: 'blob'
            }
        );

        downloadFile(response.data, filename);

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

const downloadFile = (file: Blob, filename: string) => {
    const fileURL = URL.createObjectURL(new Blob([file], { type: file.type }));

    const downloadLink = document.createElement('a');
    downloadLink.href = fileURL;
    downloadLink.setAttribute('download', filename);

    document.body.appendChild(downloadLink);
    downloadLink.click();

    document.body.removeChild(downloadLink);
    URL.revokeObjectURL(fileURL);
}