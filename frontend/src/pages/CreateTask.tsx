import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const CreateTask: React.FC = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    dueDate: '',
    status: '',
    priority: ''
  });

  const [message, setMessage] = useState<string | null>(null);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const token = localStorage.getItem('token');
    if (!token) {
      setMessage("You are not logged in");
      return;
    }

    const formattedDueDate = new Date(formData.dueDate).toISOString();

    const dataToSend = {
      title: formData.title,
      description: formData.description,
      dueDate: formattedDueDate,
      status: formData.status,
      priority: formData.priority
    };

    try {
      const response = await fetch('http://localhost:5146/api/tasks/create', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(dataToSend)
      });

      const contentType = response.headers.get("content-type");

      let result;
      if (contentType && contentType.includes("application/json")) {
        result = await response.json();
      } else {
        const text = await response.text();
        result = { message: text };
      }

      if (!response.ok) {
        throw new Error(result.message || 'Something went wrong');
      }

      setMessage(result.message || 'Task created successfully!');
      setTimeout(() => navigate('/dashboard'), 1200);

    } catch (error) {
      if (error instanceof Error) {
        console.error("Error details:", error.message);
        setMessage(error.message);
      } else {
        console.error("Unexpected error:", error);
        setMessage("Unexpected error");
      }
    }
  };

  return (
    <div className="w-screen h-screen bg-[#f8fafc] text-[#333] overflow-y-auto p-10 md:p-16 font-[Segoe UI]">
      <a
        href="/dashboard"
        className="inline-flex items-center text-[20px] font-bold text-[#102d3f] mb-6 hover:text-[#27ae60] transition-colors duration-300 before:content-['←'] before:mr-2 before:text-[24px]"
      >
        Back
      </a>

      <div className="mb-8">
        <h1 className="text-[32px] font-bold text-[#102d3f] relative inline-block mb-2 after:content-[''] after:absolute after:bottom-[-5px] after:left-0 after:w-[60px] after:h-[4px] after:bg-gradient-to-r after:from-[#102d3f] after:to-[#27ae60] after:rounded-md">
          Create New Task
        </h1>
        <p className="text-[16px] text-gray-500">
          Fill out the form below to add a new task to your workflow
        </p>
      </div>

      {message && (
        <div className={`mb-6 p-3 rounded-md border ${message.includes('success') ? 'bg-green-100 text-green-700 border-green-400' : 'bg-red-100 text-red-700 border-red-400'}`}>
          {message}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Title */}
        <div className="mb-6">
          <label htmlFor="title" className="block mb-2 text-[#102d3f] font-semibold text-[15px] tracking-wide">
            Task Title
          </label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            placeholder="Enter a descriptive task title"
            required
            className="w-full p-3 text-[15px] bg-[#f8fafc] border border-gray-300 rounded-lg transition-all focus:outline-none focus:border-[#102d3f] focus:bg-white focus:ring-2 focus:ring-[#102d3f]/10"
          />
        </div>

        {/* Description */}
        <div className="mb-6">
          <label htmlFor="description" className="block mb-2 text-[#102d3f] font-semibold text-[15px] tracking-wide">
            Task Description
          </label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            placeholder="Provide detailed information about the task"
            className="w-full p-3 text-[15px] bg-[#f8fafc] border border-gray-300 rounded-lg h-[120px] resize-y leading-relaxed transition-all focus:outline-none focus:border-[#102d3f] focus:bg-white focus:ring-2 focus:ring-[#102d3f]/10"
          ></textarea>
        </div>

        {/* Due Date */}
        <div className="mb-6">
          <label htmlFor="dueDate" className="block mb-2 text-[#102d3f] font-semibold text-[15px] tracking-wide">
            Due Date
          </label>
          <input
            type="date"
            id="dueDate"
            name="dueDate"
            value={formData.dueDate}
            onChange={handleChange}
            required
            className="w-full p-3 text-[15px] bg-[#f8fafc] border border-gray-300 rounded-lg transition-all focus:outline-none focus:border-[#102d3f] focus:bg-white focus:ring-2 focus:ring-[#102d3f]/10"
          />
        </div>

        {/* Status */}
        <div className="mb-6">
          <label htmlFor="status" className="block mb-2 text-[#102d3f] font-semibold text-[15px] tracking-wide">
            Status
          </label>
          <select
            id="status"
            name="status"
            value={formData.status}
            onChange={handleChange}
            required
            className="w-full p-3 text-[15px] bg-[#f8fafc] border border-gray-300 rounded-lg transition-all focus:outline-none focus:border-[#102d3f] focus:bg-white focus:ring-2 focus:ring-[#102d3f]/10"
          >
            <option value="">Select current status</option>
            <option value="In Progress">In Progress</option>
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
          </select>
        </div>

        {/* Priority */}
        <div className="mb-6">
          <label htmlFor="priority" className="block mb-2 text-[#102d3f] font-semibold text-[15px] tracking-wide">
            Priority
          </label>
          <select
            id="priority"
            name="priority"
            value={formData.priority}
            onChange={handleChange}
            required
            className="w-full p-3 text-[15px] bg-[#f8fafc] border border-gray-300 rounded-lg transition-all focus:outline-none focus:border-[#102d3f] focus:bg-white focus:ring-2 focus:ring-[#102d3f]/10"
          >
            <option value="">Select priority level</option>
            <option value="High">High</option>
            <option value="Medium">Medium</option>
            <option value="Low">Low</option>
          </select>
        </div>

        {/* Buttons */}
        <div className="flex flex-col md:flex-row gap-4 mt-8">
          <button
            type="submit"
            className="relative px-6 py-3 text-white font-semibold text-[15px] rounded-lg min-w-[160px] bg-gradient-to-br from-[#1a4b6d] to-[#102d3f] shadow-md hover:-translate-y-1 transition-transform"
          >
            Create Task
          </button>
          <button
            type="button"
            onClick={() => navigate('/dashboard')}
            className="relative px-6 py-3 text-white font-semibold text-[15px] rounded-lg min-w-[160px] bg-gradient-to-br from-[#6b7280] to-[#4b5563] shadow-md hover:-translate-y-1 transition-transform"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreateTask;
