import { useState } from 'react';
import { uploadExam } from '../services/examService';

function ExamUpload() {
  const [file, setFile] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [detailedError, setDetailedError] = useState('');

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    if (selectedFile) {
      // Accept any file extension but warn if not JSON
      if (selectedFile.type !== 'application/json') {
        setError('Warning: The file selected is not a JSON file. Upload may fail.');
      } else {
        setError('');
      }
      setFile(selectedFile);
      setDetailedError('');
    } else {
      setFile(null);
      setError('No file selected');
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
      setDetailedError('');

      console.log('Uploading file:', file.name, file.type, file.size);
      
      const result = await uploadExam(file);
      
      if (result && result.exam) {
        setMessage(`Exam "${result.exam.examTitle}" uploaded successfully!`);
        setFile(null);
        // Reset the file input
        e.target.reset();
      } else {
        setError('Upload successful but received unexpected response format');
        setDetailedError(JSON.stringify(result, null, 2));
      }
    } catch (error) {
      console.error('Upload error details:', error);
      
      // Improved error handling
      let errorMsg = 'Error uploading exam';
      
      if (error.response) {
        // The server responded with a status code outside the 2xx range
        errorMsg = `Server Error (${error.response.status}): ${error.response.data || error.response.statusText}`;
        setDetailedError(JSON.stringify(error.response, null, 2));
      } else if (error.request) {
        // The request was made but no response was received
        errorMsg = 'No response from server. Please check your network connection or API availability.';
      } else {
        // Something else happened while setting up the request
        errorMsg = `Error: ${error.message}`;
      }
      
      setError(errorMsg);
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
        {detailedError && (
          <div className="detailed-error">
            <details>
              <summary>Error Details</summary>
              <pre>{detailedError}</pre>
            </details>
          </div>
        )}
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
