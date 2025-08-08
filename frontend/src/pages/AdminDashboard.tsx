import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { Pie } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(ArcElement, Tooltip, Legend);

interface Task {
  id: number;
  title: string;
  status: string;
  user: string;
}

interface User {
  id: number;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

const AdminDashboard: React.FC = () => {
  const [taskStats, setTaskStats] = useState({ completed: 0, pending: 0, inProgress: 0 });
  const [userStats, setUserStats] = useState({ total: 0, active: 0, new: 0 });
  const [recentTasks, setRecentTasks] = useState<Task[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const navigate = useNavigate();

  const fullName = localStorage.getItem('fullName') || 'Admin';

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }

    axios
      .get('http://localhost:5146/api/admin-dashboard/summary', {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => {
        setTaskStats(res.data.taskStats);
        setUserStats(res.data.userStats);
        setRecentTasks(res.data.recentTasks);
        setUsers(res.data.users);
      })
      .catch((err) => {
        console.error(err);
        if (err.response?.status === 401) {
          localStorage.clear();
          navigate('/login');
        }
      });
  }, [navigate]);

  const logout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const handleToggleUserStatus = async (id: number) => {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      const res = await axios.put(
        `http://localhost:5146/api/admin-dashboard/toggle-active/${id}`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
      );

      const updatedUsers = users.map(user =>
        user.id === id ? { ...user, isActive: res.data.isActive } : user
      );

      setUsers(updatedUsers);
    } catch (error) {
      console.error('Failed to toggle user status:', error);
      alert('Something went wrong while updating user status.');
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    const token = localStorage.getItem('token');
    if (!token) return;

    const confirmDelete = window.confirm('Are you sure you want to delete this task?');
    if (!confirmDelete) return;

    try {
      await axios.delete(`http://localhost:5146/api/admin-dashboard/delete-task/${taskId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setRecentTasks(recentTasks.filter(task => task.id !== taskId));
    } catch (error) {
      console.error('Failed to delete task:', error);
      alert('Something went wrong while deleting the task.');
    }
  };

  const pieData = {
    labels: ['Completed', 'In Progress', 'Pending'],
    datasets: [
      {
        data: [taskStats.completed, taskStats.inProgress, taskStats.pending],
        backgroundColor: ['#4caf50', '#ff9800', '#f44336'],
        hoverOffset: 10,
        borderWidth: 2,
        borderColor: '#fff',
      },
    ],
  };

  return (
    <div className="w-screen min-h-screen bg-[#f8fafc] text-[#333] font-[Montserrat] flex">
      {/* Sidebar */}
      <aside className="w-[250px] bg-[#002f34] text-white p-6 flex flex-col items-center flex-shrink-0 min-h-screen">
        <h2 className="text-2xl mb-8 text-center">Admin Panel</h2>
        <div className="text-center mb-10">
          <div className="w-[60px] h-[60px] bg-[#27ae60] rounded-full flex items-center justify-center mx-auto text-2xl font-bold">
            {fullName[0]?.toUpperCase()}
          </div>
          <h3 className="mt-2 text-lg">{fullName}</h3>
        </div>
        <nav className="w-full">
  <ul className="w-full">
    <li
      className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]"
      onClick={() => navigate('/admin-dashboard')}
    >
      Dashboard
    </li>
    <li
      className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]"
      onClick={() => navigate('/admin/create-task')}
    >
      Create Task {/* ✅ New Create Task Nav */}
    </li>
    <li
      className="py-3 border-b border-white/10 text-center cursor-pointer hover:text-[#00e0e0]"
      onClick={logout}
    >
      Logout
    </li>
  </ul>
</nav>

      </aside>

      {/* Main Content */}
      <main className="flex-1 p-8">
        <h1 className="text-3xl font-bold mb-8 text-[#002f34]">Welcome, {fullName}</h1>

        {/* User Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
          <div className="bg-[#002f34] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">Total Users</h2>
            <p className="text-3xl mt-2">{userStats.total}</p>
          </div>
          <div className="bg-[#006f6f] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">Active Users</h2>
            <p className="text-3xl mt-2">{userStats.active}</p>
          </div>
          <div className="bg-[#009688] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">New Users (7d)</h2>
            <p className="text-3xl mt-2">{userStats.new}</p>
          </div>
        </div>

        {/* Task Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
          <div className="bg-[#4caf50] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">Completed Tasks</h2>
            <p className="text-3xl mt-2">{taskStats.completed}</p>
          </div>
          <div className="bg-[#ff9800] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">In Progress</h2>
            <p className="text-3xl mt-2">{taskStats.inProgress}</p>
          </div>
          <div className="bg-[#f44336] text-white p-6 rounded-xl shadow-md">
            <h2 className="text-lg font-semibold">Pending Tasks</h2>
            <p className="text-3xl mt-2">{taskStats.pending}</p>
          </div>
        </div>

        {/* Pie Chart for Task Distribution */}
        <div className="bg-white p-6 mb-10 rounded-xl shadow-md w-full">
          <h2 className="text-xl font-semibold text-center text-[#002f34] mb-4">Task Distribution</h2>
          <div className="max-w-[300px] mx-auto">
            <Pie data={pieData} />
          </div>
        </div>

        {/* Recent Tasks */}
        <h2 className="text-2xl font-bold mb-4 text-[#002f34]">Recent Tasks</h2>
        <div className="overflow-x-auto mb-10 rounded-lg shadow">
          <table className="min-w-full bg-white text-sm">
            <thead className="bg-[#002f34] text-white">
              <tr>
                <th className="p-3 text-left">Title</th>
                <th className="p-3 text-left">Status</th>
                <th className="p-3 text-left">User</th>
                <th className="p-3 text-left">Actions</th>
              </tr>
            </thead>
            <tbody>
              {recentTasks.map((task, index) => (
                <tr key={index} className="border-b hover:bg-gray-100">
                  <td className="p-3 text-black cursor-pointer" onClick={() => navigate(`/admin/task/${task.id}`)}>
                    {task.title}
                  </td>
                  <td className="p-3">{task.status}</td>
                  <td className="p-3">{task.user}</td>
                  <td className="p-3 space-x-2">
                    <button onClick={() => navigate(`/admin/edit-task/${task.id}`)} className="bg-blue-600 hover:bg-blue-700 text-white px-2 py-1 rounded text-xs">
                      Edit
                    </button>
                    <button onClick={() => handleDeleteTask(task.id)} className="bg-red-600 hover:bg-red-700 text-white px-2 py-1 rounded text-xs">
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* User Table */}
        <h2 className="text-2xl font-bold mb-4 text-[#002f34]">User Management</h2>
        <div className="overflow-x-auto rounded-lg shadow">
          <table className="min-w-full bg-white text-sm">
            <thead className="bg-[#002f34] text-white">
              <tr>
                <th className="p-3 text-left">Name</th>
                <th className="p-3 text-left">Email</th>
                <th className="p-3 text-left">Role</th>
                <th className="p-3 text-left">Active</th>
                <th className="p-3 text-left">Joined</th>
                <th className="p-3 text-left">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user, index) => (
                <tr key={index} className="border-b hover:bg-gray-100">
                  <td className="p-3">{user.fullName}</td>
                  <td className="p-3">{user.email}</td>
                  <td className="p-3">{user.role}</td>
                  <td className="p-3">{user.isActive ? '✅' : '❌'}</td>
                  <td className="p-3">{new Date(user.createdAt).toLocaleDateString()}</td>
                  <td className="p-3">
                    {user.role !== 'Admin' && (
                      <button
                        onClick={() => handleToggleUserStatus(user.id)}
                        className={`px-3 py-1 rounded text-white text-xs font-medium ${user.isActive ? 'bg-red-500 hover:bg-red-600' : 'bg-green-500 hover:bg-green-600'}`}
                      >
                        {user.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </main>
    </div>
  );
};

export default AdminDashboard;
