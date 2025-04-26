import { Link, useLocation } from 'react-router-dom';

function Navigation() {
  const location = useLocation();
  
  return (
    <nav className="app-navigation">
      <div className="nav-logo">
        <Link to="/">EgzamApp</Link>
      </div>
      
      <ul className="nav-links">
        <li className={location.pathname === '/exams' || location.pathname === '/' ? 'active' : ''}>
          <Link to="/exams">My Exams</Link>
        </li>
        <li className={location.pathname === '/upload' ? 'active' : ''}>
          <Link to="/upload">Upload Exam</Link>
        </li>
      </ul>
      
      <div className="nav-account">
        <span className="user">Demo User</span>
      </div>
    </nav>
  );
}

export default Navigation;
