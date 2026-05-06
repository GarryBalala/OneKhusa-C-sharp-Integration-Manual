import axios from 'axios';

const API_URL = 'http://localhost:5005/api';
const api = axios.create({ baseURL: API_URL });

export const buyTicket = (eventId, email) => api.post(`/Tickets/buy/${eventId}?email=${email}`);
export const requestSinglePayout = (data) => api.post('/Disbursements/single', data);
export const uploadBatchFile = (file, email) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`/Disbursements/upload-file?email=${email}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
    });
};

export default api;