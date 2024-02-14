import axios from 'axios';
import cookie from 'react-cookies'

const Interceptor = axios.create({ baseURL: 'https://localhost:7067' });
Interceptor.interceptors.request.use(request => requestInterceptor(request))

const requestInterceptor = (request: any) => {
    request.headers['X-XSRF-TOKEN'] = cookie.load('.AspNetCore.Xsrf')
    return request;
}

export default Interceptor;