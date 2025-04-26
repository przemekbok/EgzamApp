import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';

// Import components
import Navigation from './components/Navigation';
import ExamList from './components/ExamList';
import ExamDetail from './components/ExamDetail';
import ExamTake from './components/ExamTake';
import ExamUpload from './components/ExamUpload';

function App() {
  return (
    <Router>
      <div className="app-container">
        <Navigation />
        
        <main className="app-content">
          <Routes>
            <Route path="/" element={<Navigate to="/exams" replace />} />
            <Route path="/exams" element={<ExamList />} />
            <Route path="/exams/:id" element={<ExamDetail />} />
            <Route path="/exams/:id/take" element={<ExamTake />} />
            <Route path="/upload" element={<ExamUpload />} />
          </Routes>
        </main>
        
        <footer className="app-footer">
          <p>&copy; {new Date().getFullYear()} EgzamApp - Training Exam Platform</p>
        </footer>
      </div>
    </Router>
  );
}

export default App;
