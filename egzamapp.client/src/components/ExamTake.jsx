import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getExam, startExam, submitExam } from '../services/examService';

function ExamTake() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [exam, setExam] = useState(null);
  const [userExam, setUserExam] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [currentQuestion, setCurrentQuestion] = useState(0);
  const [answers, setAnswers] = useState({});
  const [timeLeft, setTimeLeft] = useState(null);
  const [examStarted, setExamStarted] = useState(false);
  const [examSubmitted, setExamSubmitted] = useState(false);
  const [result, setResult] = useState(null);

  // Fetch the exam details
  useEffect(() => {
    const fetchExam = async () => {
      try {
        const data = await getExam(id);
        console.log("Exam data:", data); // Debug log
        
        // Make sure exam has the required properties
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

  // Start the exam timer when exam starts
  useEffect(() => {
    let timer;
    
    if (examStarted && timeLeft > 0 && !examSubmitted) {
      timer = setTimeout(() => {
        setTimeLeft(prevTime => {
          if (prevTime <= 1) {
            handleSubmit();
            return 0;
          }
          return prevTime - 1;
        });
      }, 1000);
    }

    return () => clearTimeout(timer);
  }, [examStarted, timeLeft, examSubmitted]);

  const handleStartExam = async () => {
    try {
      setIsLoading(true);
      const startedExam = await startExam(id);
      setUserExam(startedExam);
      setExamStarted(true);
      
      // Parse time limit (e.g., "120 minutes" to seconds)
      const timeMatch = exam.timeLimit?.match(/(\d+)\s*minutes?/i);
      const minutes = timeMatch ? parseInt(timeMatch[1]) : 60;
      setTimeLeft(minutes * 60);
      
      setIsLoading(false);
    } catch (err) {
      setError('Failed to start the exam. Please try again.');
      setIsLoading(false);
    }
  };

  const handleAnswerSelect = (questionIdx, answerIdx) => {
    setAnswers({
      ...answers,
      [questionIdx]: answerIdx
    });
  };

  const handleNextQuestion = () => {
    if (currentQuestion < (exam?.questions?.length || 0) - 1) {
      setCurrentQuestion(currentQuestion + 1);
    }
  };

  const handlePrevQuestion = () => {
    if (currentQuestion > 0) {
      setCurrentQuestion(currentQuestion - 1);
    }
  };

  const handleSubmit = async () => {
    try {
      setIsLoading(true);
      
      // Format answers for submission
      const formattedAnswers = Object.keys(answers).map(questionIdx => {
        const questionIndex = parseInt(questionIdx);
        return {
          questionId: exam.questions[questionIndex]?.id || 0,
          selectedAnswer: answers[questionIdx]
        };
      });
      
      const submissionResult = await submitExam(userExam.id, formattedAnswers);
      setResult(submissionResult);
      setExamSubmitted(true);
      setIsLoading(false);
    } catch (err) {
      setError('Failed to submit the exam. Please try again.');
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  if (!exam) {
    return <div>Exam not found</div>;
  }

  // Format time (seconds) to MM:SS
  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  // Exam results view
  if (examSubmitted && result) {
    return (
      <div className="exam-result-container">
        <h2>Exam Completed</h2>
        <div className="result-summary">
          <h3>{exam.examTitle}</h3>
          <p>Your Score: {result.score}%</p>
          <p>Passing Score: {exam.passingScore}%</p>
          <p className={result.score >= exam.passingScore ? "pass" : "fail"}>
            {result.score >= exam.passingScore ? "PASSED" : "FAILED"}
          </p>
        </div>
        
        <button 
          onClick={() => navigate('/exams')}
          className="button"
        >
          Back to Exams
        </button>
      </div>
    );
  }

  // Exam start screen
  if (!examStarted) {
    return (
      <div className="exam-start-container">
        <h2>{exam.examTitle}</h2>
        <p>{exam.examDescription}</p>
        
        <div className="exam-info">
          <p><strong>Questions:</strong> {exam.questions?.length || 0}</p>
          <p><strong>Passing Score:</strong> {exam.passingScore}%</p>
          <p><strong>Time Limit:</strong> {exam.timeLimit}</p>
        </div>
        
        <button 
          onClick={handleStartExam}
          className="button primary"
        >
          Start Exam
        </button>
        
        <button 
          onClick={() => navigate('/exams')}
          className="button"
        >
          Back to Exams
        </button>
      </div>
    );
  }

  // Safety check for exam questions
  if (!Array.isArray(exam.questions) || exam.questions.length === 0) {
    return (
      <div className="error-message">
        <h2>Error: No Questions Available</h2>
        <p>This exam doesn't have any questions or the question data is invalid.</p>
        <button 
          onClick={() => navigate('/exams')}
          className="button"
        >
          Back to Exams
        </button>
      </div>
    );
  }

  // Current question being displayed
  const question = exam.questions[currentQuestion];
  const selectedAnswer = answers[currentQuestion];

  // Check if the current question exists and has required properties
  if (!question || !Array.isArray(question.options)) {
    return (
      <div className="error-message">
        <h2>Error: Invalid Question</h2>
        <p>The current question data is invalid or incomplete.</p>
        <button 
          onClick={() => navigate('/exams')}
          className="button"
        >
          Back to Exams
        </button>
      </div>
    );
  }

  // Exam taking view
  return (
    <div className="exam-take-container">
      <div className="exam-header">
        <h2>{exam.examTitle}</h2>
        <div className="exam-progress">
          <span>Question {currentQuestion + 1}/{exam.questions.length}</span>
          <span className="timer">Time Left: {formatTime(timeLeft)}</span>
        </div>
      </div>
      
      <div className="question-container">
        <h3>Question {currentQuestion + 1}</h3>
        <div className="question-text">
          {question.questionText}
        </div>
        
        <div className="options-container">
          {question.options.map((option, idx) => (
            <div 
              key={idx}
              className={`option ${selectedAnswer === idx ? 'selected' : ''}`}
              onClick={() => handleAnswerSelect(currentQuestion, idx)}
            >
              <span className="option-letter">{String.fromCharCode(65 + idx)}</span>
              <span className="option-text">{option}</span>
            </div>
          ))}
        </div>
      </div>
      
      <div className="exam-navigation">
        <button 
          onClick={handlePrevQuestion} 
          disabled={currentQuestion === 0}
          className="button"
        >
          Previous
        </button>
        
        {currentQuestion < exam.questions.length - 1 ? (
          <button 
            onClick={handleNextQuestion}
            className="button"
          >
            Next
          </button>
        ) : (
          <button 
            onClick={handleSubmit}
            className="button primary"
          >
            Submit Exam
          </button>
        )}
      </div>
      
      <div className="question-navigation">
        {exam.questions.map((_, idx) => (
          <button 
            key={idx}
            onClick={() => setCurrentQuestion(idx)}
            className={`question-dot ${idx === currentQuestion ? 'active' : ''} ${answers[idx] !== undefined ? 'answered' : ''}`}
          >
            {idx + 1}
          </button>
        ))}
      </div>
    </div>
  );
}

export default ExamTake;
