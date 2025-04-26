import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getExam } from '../services/examService';

function ExamDetail() {
  const { id } = useParams();
  const [exam, setExam] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchExam = async () => {
      try {
        setIsLoading(true);
        const data = await getExam(id);
        console.log("Exam detail data:", data); // Debug log
        
        // Make sure data is valid
        if (data && typeof data === 'object') {
          // Ensure questions is an array
          if (!Array.isArray(data.questions)) {
            data.questions = [];
          }
          setExam(data);
          setError('');
        } else {
          setError('Invalid exam data received');
        }
      } catch (err) {
        setError('Failed to load exam details. Please try again later.');
        console.error('Error fetching exam:', err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchExam();
  }, [id]);

  if (isLoading) {
    return <div>Loading exam details...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  if (!exam) {
    return <div>Exam not found</div>;
  }

  return (
    <div className="exam-detail-container">
      <h2>{exam.examTitle}</h2>
      <p className="exam-description">{exam.examDescription}</p>
      
      <div className="exam-meta">
        <div className="meta-item">
          <span className="meta-label">Questions:</span>
          <span className="meta-value">{exam.questions?.length || 0}</span>
        </div>
        <div className="meta-item">
          <span className="meta-label">Passing Score:</span>
          <span className="meta-value">{exam.passingScore}%</span>
        </div>
        <div className="meta-item">
          <span className="meta-label">Time Limit:</span>
          <span className="meta-value">{exam.timeLimit}</span>
        </div>
        <div className="meta-item">
          <span className="meta-label">Upload Date:</span>
          <span className="meta-value">
            {exam.uploadDate ? new Date(exam.uploadDate).toLocaleDateString() : 'N/A'}
          </span>
        </div>
      </div>
      
      <div className="exam-actions">
        <Link to={`/exams/${exam.id}/take`} className="button primary">
          Take Exam
        </Link>
        <Link to="/exams" className="button">
          Back to Exams
        </Link>
      </div>
      
      <div className="questions-preview">
        <h3>Questions Preview</h3>
        <p className="preview-note">
          Note: This is just a preview. Questions will be presented one at a time during the actual exam.
        </p>
        
        {Array.isArray(exam.questions) && exam.questions.length > 0 ? (
          exam.questions.map((question, index) => (
            <div key={index} className="question-preview">
              <h4>Question {index + 1}</h4>
              <p className="question-text">{question.questionText}</p>
              <div className="question-meta">
                <span className="difficulty">Difficulty: {question.difficulty || 'N/A'}</span>
                <span className="type">Type: {question.type || 'N/A'}</span>
              </div>
            </div>
          ))
        ) : (
          <p>No questions available for this exam.</p>
        )}
      </div>
    </div>
  );
}

export default ExamDetail;
