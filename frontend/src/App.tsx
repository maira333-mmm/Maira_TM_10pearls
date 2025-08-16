import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import AuthPage from './AuthPage';
import Dashboard from './pages/Dashboard';
import CreateTask from './pages/CreateTask';
import TaskList from './pages/TaskList';
import EditTask from './pages/EditTask';
import TaskDetail from './pages/TaskDetail';
import UserProfile from './pages/UserProfile';
import AdminDashboard from './pages/AdminDashboard';
import AdminTaskDetail from './pages/AdminTaskDetail';
import AdminEditTask from './pages/AdminEditTask';
import AdminCreateTask from './pages/AdminCreateTask'; // ✅ NEW Import

import './index.css';

function App() {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  return (
    <BrowserRouter>
      <Routes>
        {/* Default Route */}
        <Route path="/" element={<Navigate to="/login" />} />

        {/* Auth */}
        <Route path="/login" element={<AuthPage />} />

        {/* User Routes */}
        <Route path="/dashboard" element={token && role === 'User' ? <Dashboard /> : <Navigate to="/login" />} />
        <Route path="/create" element={token && role === 'User' ? <CreateTask /> : <Navigate to="/login" />} />
        <Route path="/tasks" element={token && role === 'User' ? <TaskList /> : <Navigate to="/login" />} />
        <Route path="/edit-task/:id" element={token && role === 'User' ? <EditTask /> : <Navigate to="/login" />} />
        <Route path="/tasks/:id" element={token && role === 'User' ? <TaskDetail /> : <Navigate to="/login" />} />
        <Route path="/profile" element={token && role === 'User' ? <UserProfile /> : <Navigate to="/login" />} />

        {/* Admin Routes */}
        <Route path="/admin-dashboard" element={token && role === 'Admin' ? <AdminDashboard /> : <Navigate to="/login" />} />
        <Route path="/admin/task/:id" element={token && role === 'Admin' ? <AdminTaskDetail /> : <Navigate to="/login" />} />
        <Route path="/admin/edit-task/:id" element={token && role === 'Admin' ? <AdminEditTask /> : <Navigate to="/login" />} />
        <Route path="/admin/create-task" element={token && role === 'Admin' ? <AdminCreateTask /> : <Navigate to="/login" />} /> {/* ✅ NEW Route */}
      </Routes>
    </BrowserRouter>
  );
}

export default App;