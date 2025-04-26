import { useState } from 'react';
import { uploadExam } from '../services/examService';

function ExamUpload() {
  const [file, setFile] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    if (selectedFile && selectedFile.type === 'application/json') {
      setFile(selectedFile);
      setError('');
    } else {
      setFile(null);
      setError('Please select a valid JSON file');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file) {
      setError('Please select a file');
      return;
    }

    try {
      setIsLoading(true);
      setMessage('');
      setError('');

      const result = await uploadExam(file);
      setMessage(`Exam "${result.exam.examTitle}" uploaded successfully!`);
      setFile(null);
      // Reset the file input
      e.target.reset();
    } catch (error) {
      setError(error.response?.data || 'Error uploading exam');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="exam-upload-container">
      <h2>Upload Exam</h2>
      <p>Upload a JSON file containing an exam structure.</p>
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="exam-file">Select Exam File (JSON):</label>
          <input
            type="file"
            id="exam-file"
            accept=".json"
            onChange={handleFileChange}
            disabled={isLoading}
          />
        </div>
        
        {error && <div className="error-message">{error}</div>}
        {message && <div className="success-message">{message}</div>}
        
        <button 
          type="submit" 
          disabled={!file || isLoading}
          className="submit-button"
        >
          {isLoading ? 'Uploading...' : 'Upload Exam'}
        </button>
      </form>
    </div>
  );
}

export default ExamUpload;
