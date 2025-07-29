import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

const EditTask: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    status: '',
    priority: ''
  });

  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const fetchTask = async () => {
      const token = localStorage.getItem('token');
      const res = await fetch(`http://localhost:5146/api/tasks/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      if (res.ok) {
        const data = await res.json();
        setFormData({
          title: data.title,
          description: data.description,
          dueDate: data.dueDate.split('T')[0],
          status: data.status,
          priority: data.priority
        });
      } else {
        const err = await res.text();
        setMessage(err);
      }
    };

    fetchTask();
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const token = localStorage.getItem('token');

    const formattedDueDate = new Date(formData.dueDate).toISOString();

    const response = await fetch(`http://localhost:5146/api/tasks/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({ ...formData, dueDate: formattedDueDate })
    });

    const resData = await response.json();
    if (!response.ok) {
      setMessage(resData.message || 'Update failed');
    } else {
      setMessage('Task updated successfully');
      setTimeout(() => navigate('/dashboard'), 1200);
    }
  };

  return (
    <div className="w-screen min-h-screen bg-[#f8fafc] text-[#333] p-10 md:p-16 font-[Segoe_UI]">
      <a href="/dashboard" className="inline-flex items-center text-[20px] font-bold text-[#102d3f] mb-6 hover:text-[#27ae60] transition-colors duration-300 before:content-['←'] before:mr-2 before:text-[24px]">
        Back
      </a>

      <div className="mb-8">
        <h1 className="text-[32px] font-bold text-[#102d3f]">Edit Task</h1>
        <p className="text-[16px] text-gray-500">Update the details of your task</p>
      </div>

      {message && <div className="mb-4 p-3 border rounded bg-red-100 text-red-700">{message}</div>}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label className="block text-sm font-medium text-[#102d3f] mb-1">Title</label>
          <input
            type="text"
            name="title"
            required
            value={formData.title}
            onChange={handleChange}
            className="w-full p-3 border rounded bg-white text-black focus:ring-2 focus:ring-[#102d3f]"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-[#102d3f] mb-1">Description</label>
          <textarea
            name="description"
            value={formData.description}
            onChange={handleChange}
            className="w-full p-3 border rounded bg-white text-black focus:ring-2 focus:ring-[#102d3f]"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-[#102d3f] mb-1">Due Date</label>
          <input
            type="date"
            name="dueDate"
            value={formData.dueDate}
            onChange={handleChange}
            className="w-full p-3 border rounded bg-white text-black focus:ring-2 focus:ring-[#102d3f]"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-[#102d3f] mb-1">Status</label>
          <select
            name="status"
            value={formData.status}
            onChange={handleChange}
            className="w-full p-3 border rounded bg-white text-black focus:ring-2 focus:ring-[#102d3f]"
          >
            <option value="">Select status</option>
            <option value="Pending">Pending</option>
            <option value="In Progress">In Progress</option>
            <option value="Completed">Completed</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-[#102d3f] mb-1">Priority</label>
          <select
            name="priority"
            value={formData.priority}
            onChange={handleChange}
            className="w-full p-3 border rounded bg-white text-black focus:ring-2 focus:ring-[#102d3f]"
          >
            <option value="">Select priority</option>
            <option value="High">High</option>
            <option value="Medium">Medium</option>
            <option value="Low">Low</option>
          </select>
        </div>

        <div className="flex gap-4">
          <button type="submit" className="px-6 py-3 bg-[#102d3f] text-white rounded-lg shadow hover:bg-[#1a4b6d]">Update</button>
          <button onClick={() => navigate('/dashboard')} type="button" className="px-6 py-3 bg-gray-500 text-white rounded-lg hover:bg-gray-700">Cancel</button>
        </div>
      </form>
    </div>
  );
};

export default EditTask;
