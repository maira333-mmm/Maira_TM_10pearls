import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

interface TaskSummary {
  completed: number;
  inProgress: number;
  pending: number;
}

interface UserInfo {
  email: string;
  fullName: string;
}

const Dashboard: React.FC = () => {
  const [summary, setSummary] = useState<TaskSummary>({
    completed: 0,
    inProgress: 0,
    pending: 0,
  });

  const [user, setUser] = useState<UserInfo>({
    email: '',
    fullName: localStorage.getItem('fullName') || '',
  });

  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }

    const fetchData = async () => {
      try {
        const response = await axios.get('http://localhost:5146/api/dashboard/summary', {
          headers: { Authorization: `Bearer ${token}` },
        });

        const { completed, inProgress, pending, user: userData } = response.data;

        setSummary({ completed, inProgress, pending });
        setUser({
          email: userData.email,
          fullName: userData.fullName || '',
        });
      } catch (error) {
        console.error('Error fetching dashboard data', error);
        navigate('/login'); // logout if token is invalid
      }
    };

    fetchData();
  }, [navigate]);

  const logout = () => {
    localStorage.clear();
    navigate('/login');
  };

  return (
    <div className="w-screen h-screen overflow-y-auto bg-[#f8fafc] text-[#333] font-[Montserrat] flex">
      {/* Sidebar */}
      <aside className="w-[250px] bg-[#102d3f] text-white p-6 flex flex-col items-center flex-shrink-0 min-h-screen">
        <h2 className="text-2xl mb-8 text-center">Task Manager</h2>
        <div className="text-center mb-10">
          <div className="w-[60px] h-[60px] bg-[#27ae60] rounded-full flex items-center justify-center mx-auto text-2xl font-bold">
            {user.fullName ? user.fullName[0].toUpperCase() : user.email[0]?.toUpperCase()}
          </div>
          <h3 className="mt-2 text-lg">{user.fullName || user.email}</h3>
          <p className="text-sm text-gray-300">{user.email}</p>
        </div>
        <nav className="w-full">
          <ul className="w-full">
            <li className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]" onClick={() => navigate('/profile')}>Profile</li>
            <li className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]" onClick={() => navigate('/dashboard')}>Dashboard</li>
            <li className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]" onClick={() => navigate('/tasks')}>Task List</li>
            <li className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]" onClick={() => navigate('/create')}>Create Task</li>
            <li className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]" onClick={logout}>Logout</li>
          </ul>
        </nav>
      </aside>

      {/* Main Content */}
      <main className="flex-1 p-8 bg-white">
        <div className="mb-6">
          <h1 className="text-2xl font-semibold">Welcome, {user.fullName || user.email.split('@')[0]}</h1>
          <p className="text-gray-600">{new Date().toDateString()}</p>
        </div>

        <div className="flex gap-5 flex-wrap mb-10 justify-between">
          <div className="flex-1 min-w-[220px] p-5 rounded-lg text-white bg-[#27ae60]">
            <h4 className="text-base mb-2">Completed Tasks</h4>
            <h2 className="text-2xl">{summary.completed}</h2>
          </div>

          <div className="flex-1 min-w-[220px] p-5 rounded-lg text-white bg-[#f39c12]">
            <h4 className="text-base mb-2">In Progress Tasks</h4>
            <h2 className="text-2xl">{summary.inProgress}</h2>
          </div>

          <div className="flex-1 min-w-[220px] p-5 rounded-lg text-white bg-[#8e44ad]">
            <h4 className="text-base mb-2">Pending Tasks</h4>
            <h2 className="text-2xl">{summary.pending}</h2>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;
