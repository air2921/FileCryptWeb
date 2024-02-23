import axios from 'axios';

const BASE_URL = 'https://localhost:7067/';

const AxiosRequest = async ({ endpoint, method, withCookie, requestBody, responseType }: RequestProps) => {
    try {
        var response;

        switch (method) {
            case 'POST':
                response = await axios.post(`${BASE_URL + endpoint}`, requestBody, { withCredentials: withCookie, responseType: responseType });
                break;
            case 'GET':
                response = await axios.get(`${BASE_URL + endpoint}`, { withCredentials: withCookie });
                break;
            case 'PUT':
                response = await axios.put(`${BASE_URL + endpoint}`, requestBody, { withCredentials: withCookie });
                break;
            case 'DELETE':
                response = await axios.delete(`${BASE_URL + endpoint}`, { withCredentials: withCookie });
                break;
            default:
                throw new Error('Invalid request method');
        }

        return { isSuccess: true, data: response.data, statusCode: response.status };

    } catch (error: any) {
        console.log(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }
        return { isSuccess: false, data: errorMessage, statusCode: statusCode };
    }
}

export default AxiosRequest;