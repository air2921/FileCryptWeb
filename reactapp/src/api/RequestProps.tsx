interface RequestProps {
    endpoint: string,
    method: string,
    withCookie: boolean,
    requestBody: any,
    responseType?: 'arraybuffer' | 'blob' | 'document' | 'json' | 'text'
}