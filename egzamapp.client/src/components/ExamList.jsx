import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getExams } from '../services/examService';

function ExamList() {
  const [exams, setExams] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchExams = async () => {
      try {
        setIsLoading(true);
        const data = await getExams();
        console.log("API response:", data); // Debug response
        
        // Ensure we have an array to work with
        const examArray = Array.isArray(data) ? data : [];
        setExams(examArray);
        setError('');
      } catch (err) {
        setError('Failed to load exams. Please try again later.');
        console.error('Error fetching exams:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchExams();
  }, []);

  if (isLoading) {
    return <div>Loading exams...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  if (!Array.isArray(exams) || exams.length === 0) {
    return (
      <div className="exam-list-empty">
        <h2>My Exams</h2>
        <p>You don't have any exams yet.</p>
        <Link to="/upload" className="button">Upload an Exam</Link>
      </div>
    );
  }

  return (
    <div className="exam-list-container">
      <h2>My Exams</h2>
      <div className="exam-list">
        {exams.map((exam) => (
          <div key={exam.id} className="exam-item">
            <h3>{exam.examTitle}</h3>
            <p>{exam.examDescription}</p>
            <div className="exam-meta">
              <span>Questions: {exam.questions?.length || 0}</span>
              <span>Passing Score: {exam.passingScore}%</span>
              <span>Time Limit: {exam.timeLimit}</span>
            </div>
            <div className="exam-actions">
              <Link to={`/exams/${exam.id}`} className="button">View Details</Link>
              <Link to={`/exams/${exam.id}/take`} className="button primary">
                Take Exam
              </Link>
            </div>
          </div>
        ))}
      </div>
      <div className="upload-more">
        <Link to="/upload" className="button">Upload Another Exam</Link>
      </div>
    </div>
  );
}

export default ExamList;
