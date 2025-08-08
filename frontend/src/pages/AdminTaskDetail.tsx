import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

interface Task {
  title: string;
  description: string;
  dueDate: string;
  status: string;
  priority: string;
}

const AdminTaskDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [task, setTask] = useState<Task | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTask = async () => {
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(`http://localhost:5146/api/admin-dashboard/tasks/${id}`, {
          headers: {
            'Authorization': `Bearer ${token}`,
          },
        });

        if (!response.ok) {
          throw new Error('Failed to fetch task details');
        }

        const data: Task = await response.json();
        setTask(data);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError('An unexpected error occurred');
        }
      }
    };

    fetchTask();
  }, [id]);

  return (
    <div className="w-screen h-screen bg-[#f8fafc] text-[#333] overflow-y-auto p-10 md:p-16 font-[Segoe UI]">
      <a
        href="/admin-dashboard"
        className="inline-flex items-center text-[20px] font-bold text-[#102d3f] mb-6 hover:text-[#27ae60] transition-colors duration-300 before:content-['←'] before:mr-2 before:text-[24px]"
      >
        Back
      </a>

      <div className="mb-8">
        <h1 className="text-[32px] font-bold text-[#102d3f] relative inline-block mb-2 after:content-[''] after:absolute after:bottom-[-5px] after:left-0 after:w-[60px] after:h-[4px] after:bg-gradient-to-r after:from-[#102d3f] after:to-[#27ae60] after:rounded-md">
          Task Details
        </h1>
        <p className="text-[16px] text-gray-500">
          View full details of the selected task
        </p>
      </div>

      {error && (
        <div className="mb-6 p-3 rounded-md border bg-red-100 text-red-700 border-red-400">
          {error}
        </div>
      )}

      {task ? (
        <div className="grid gap-6 text-[15px]">
          <div>
            <h2 className="text-[#102d3f] font-semibold mb-1">Title</h2>
            <p className="bg-white p-3 rounded border">{task.title}</p>
          </div>

          <div>
            <h2 className="text-[#102d3f] font-semibold mb-1">Description</h2>
            <p className="bg-white p-3 rounded border whitespace-pre-wrap">{task.description}</p>
          </div>

          <div>
            <h2 className="text-[#102d3f] font-semibold mb-1">Due Date</h2>
            <p className="bg-white p-3 rounded border">{new Date(task.dueDate).toLocaleDateString()}</p>
          </div>

          <div>
            <h2 className="text-[#102d3f] font-semibold mb-1">Status</h2>
            <p className="bg-white p-3 rounded border">{task.status}</p>
          </div>

          <div>
            <h2 className="text-[#102d3f] font-semibold mb-1">Priority</h2>
            <p className="bg-white p-3 rounded border">{task.priority}</p>
          </div>
        </div>
      ) : (
        !error && <p className="text-gray-500">Loading task details...</p>
      )}
    </div>
  );
};

export default AdminTaskDetail;
