import axios from 'axios';

const API_URL = '/api/exams';

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
