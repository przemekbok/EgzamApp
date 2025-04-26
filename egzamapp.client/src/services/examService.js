import axios from 'axios';

const API_URL = '/api/exams';

// Add axios interceptor for debugging API issues
axios.interceptors.request.use(request => {
  console.log('Request:', request.method.toUpperCase(), request.url);
  return request;
});

axios.interceptors.response.use(
  response => {
    console.log('Response:', response.status, response.config.method.toUpperCase(), response.config.url);
    return response;
  },
  error => {
    console.error('API Error:', {
      url: error.config?.url,
      method: error.config?.method?.toUpperCase(),
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data
    });
    return Promise.reject(error);
  }
);

export const getExams = async () => {
  try {
    const response = await axios.get(API_URL);
    return response.data;
  } catch (error) {
    console.error('Error fetching exams:', error);
    throw error;
  }
};

export const getExam = async (id) => {
  try {
    const response = await axios.get(`${API_URL}/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching exam ${id}:`, error);
    throw error;
  }
};

export const uploadExam = async (file) => {
  try {
    const formData = new FormData();
    formData.append('file', file);

    console.log('Uploading file:', file.name, file.type, file.size);

    const response = await axios.post(`${API_URL}/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  } catch (error) {
    console.error('Error uploading exam:', error);
    throw error;
  }
};

export const startExam = async (examId) => {
  try {
    const response = await axios.post(`${API_URL}/${examId}/start`);
    return response.data;
  } catch (error) {
    console.error(`Error starting exam ${examId}:`, error);
    throw error;
  }
};

export const submitExam = async (userExamId, answers) => {
  try {
    const response = await axios.post(`${API_URL}/submit`, {
      userExamId,
      answers,
    });
    return response.data;
  } catch (error) {
    console.error('Error submitting exam:', error);
    throw error;
  }
};
