export function errorHandler(error: any) {
    console.error(error);
    let statusCode = 500;
    let errorMessage = 'An error occurred during the request';
    if (error.response) {
        errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
        statusCode = error.response.status;
    }

    return {
        success: false,
        statusCode: statusCode,
        message: errorMessage
    }
}