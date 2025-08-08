import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface Task {
  id: number;
  title: string;
  status: 'Completed' | 'In Progress' | 'Pending';
  dueDate: string;
}

const TaskList: React.FC = () => {
  const navigate = useNavigate();
  const [tasks, setTasks] = useState<Task[]>([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [searchText, setSearchText] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          setError("No token found. Please login again.");
          return;
        }

        const response = await fetch('http://localhost:5146/api/tasks', {
          headers: { Authorization: `Bearer ${token}` }
        });

        if (!response.ok) {
          const text = await response.text();
          try {
            const errData = JSON.parse(text);
            throw new Error(errData.message || 'Failed to fetch tasks');
          } catch {
            throw new Error(text || 'Failed to fetch tasks');
          }
        }

        const data: Task[] = await response.json();
        const formattedTasks = data.map(task => ({
          ...task,
          dueDate: new Date(task.dueDate).toLocaleDateString('en-GB')
        }));

        setTasks(formattedTasks);
      } catch (err) {
        if (err instanceof Error) setError(err.message);
        else setError("Unknown error occurred");
      }
    };

    fetchTasks();
  }, []);

  const handleDelete = async (taskId: number) => {
    const confirmDelete = window.confirm("Are you sure you want to delete this task?");
    if (!confirmDelete) return;

    try {
      const token = localStorage.getItem('token');
      if (!token) {
        setError("No token found. Please login again.");
        return;
      }

      const response = await fetch(`http://localhost:5146/api/tasks/${taskId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` }
      });

      if (!response.ok) {
        const errText = await response.text();
        try {
          const errJson = JSON.parse(errText);
          throw new Error(errJson.message || 'Failed to delete task');
        } catch {
          throw new Error(errText || 'Failed to delete task');
        }
      }

      setTasks(prev => prev.filter(task => task.id !== taskId));
    } catch (err) {
      if (err instanceof Error) setError(err.message);
      else setError("Unknown error occurred");
    }
  };

  const filteredTasks = tasks.filter(task => {
    const matchesStatus = !statusFilter || task.status.toLowerCase().includes(statusFilter.toLowerCase());
    const matchesSearch = !searchText || task.title.toLowerCase().includes(searchText.toLowerCase());
    return matchesStatus && matchesSearch;
  });

  return (
    <div className="w-screen min-h-screen bg-[#f8fafc] text-[#333] overflow-y-auto p-10 md:p-16 font-[Segoe_UI]">
      <a href="/dashboard" className="inline-flex items-center text-[20px] font-bold text-[#102d3f] mb-6 hover:text-[#27ae60] transition-colors duration-300 before:content-['←'] before:mr-2 before:text-[24px]">
        Back
      </a>

      <div className="max-w-full">
        <div className="mb-8">
          <h1 className="text-[32px] text-[#102d3f] font-bold relative inline-block after:absolute after:bottom-[-5px] after:left-0 after:w-[60px] after:h-[4px] after:rounded after:bg-gradient-to-r after:from-[#102d3f] after:to-[#27ae60]">
            Task List
          </h1>
          <p className="text-[#6b7280] mt-2 text-[16px]">Manage your tasks efficiently</p>
        </div>

        <div className="flex flex-wrap md:flex-nowrap gap-4 items-center mb-8 bg-white p-5 rounded-[10px] shadow-md">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="flex-1 min-w-[180px] px-4 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-[#102d3f]"
          >
            <option value="">Filter by Status</option>
            <option value="Completed">Completed</option>
            <option value="In Progress">In Progress</option>
            <option value="Pending">Pending</option>
          </select>

          <input
            type="text"
            placeholder="Search Tasks..."
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            className="flex-1 min-w-[180px] px-4 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-[#102d3f]"
          />

          <button
            onClick={() => navigate('/create')}
            className="min-w-[200px] px-6 py-3 bg-gradient-to-br from-[#1a4b6d] to-[#102d3f] text-white font-semibold rounded-lg shadow-md hover:-translate-y-1 hover:shadow-xl transition-all flex items-center justify-center gap-2"
          >
            <span>+</span> Create New Task
          </button>
        </div>

        {error && <p className="text-red-500 mb-4">{error}</p>}

        <div className="overflow-x-auto">
          <table className="min-w-full bg-white rounded-xl overflow-hidden shadow-xl">
            <thead>
              <tr>
                <th className="bg-[#102d3f] text-white px-6 py-4 text-left text-sm font-semibold uppercase">Task Name</th>
                <th className="bg-[#102d3f] text-white px-6 py-4 text-left text-sm font-semibold uppercase">Status</th>
                <th className="bg-[#102d3f] text-white px-6 py-4 text-left text-sm font-semibold uppercase">Due Date</th>
                <th className="bg-[#102d3f] text-white px-6 py-4 text-left text-sm font-semibold uppercase">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredTasks.map(task => (
                <tr key={task.id} className="transition-all hover:bg-[#f8fafc] hover:translate-x-1">
                  <td
  className="px-6 py-4 text-[#374151] text-[15px] cursor-pointer hover:underline"
  onClick={() => navigate(`/tasks/${task.id}`)}
>
  {task.title}
</td>

                  <td className="px-6 py-4">
                    <span className={`inline-block text-xs font-bold text-white rounded-full px-3 py-1 shadow ${
                      task.status === 'Completed'
                        ? 'bg-gradient-to-br from-[#2ecc71] to-[#27ae60]'
                        : task.status === 'In Progress'
                        ? 'bg-gradient-to-br from-[#f39c12] to-[#e67e22]'
                        : 'bg-gradient-to-br from-[#8e44ad] to-[#9b59b6]'
                    }`}>
                      {task.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-[#374151] text-[15px]">{task.dueDate}</td>
                  <td className="px-6 py-4">
                    <div className="flex flex-wrap md:flex-nowrap gap-2">
                      <button
                        onClick={() => navigate(`/edit-task/${task.id}`)}
                        className="px-4 py-2 text-sm font-semibold text-white rounded-md bg-gradient-to-br from-[#3498db] to-[#2980b9] shadow hover:-translate-y-1 hover:shadow-md transition"
                      >
                        ✏️ Edit
                      </button>
                      <button
                        onClick={() => handleDelete(task.id)}
                        className="px-4 py-2 text-sm font-semibold text-white rounded-md bg-gradient-to-br from-[#e74c3c] to-[#c0392b] shadow hover:-translate-y-1 hover:shadow-md transition"
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {filteredTasks.length === 0 && (
                <tr>
                  <td colSpan={4} className="text-center py-6 text-gray-500 text-sm">No tasks found.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default TaskList;
