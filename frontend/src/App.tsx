import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AuthPage from './AuthPage';
import Dashboard from './pages/Dashboard';
import CreateTask from './pages/CreateTask';
import TaskList from './pages/TaskList';
import EditTask from './pages/EditTask'; // ✅ Make sure this path matches your folder structure
import './index.css';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<AuthPage />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/create" element={<CreateTask />} />
        <Route path="/tasks" element={<TaskList />} />
        <Route path="/edit-task/:id" element={<EditTask />} /> {/* ✅ Edit Task route added */}
      </Routes>
    </BrowserRouter>
  );
}

export default App;
