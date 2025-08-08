import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

interface UserInfo {
  email: string;
  fullName: string;
  role: string;
  joinDate: string;
}

const UserProfile: React.FC = () => {
  const [user, setUser] = useState<UserInfo>({
    email: '',
    fullName: '',
    role: '',
    joinDate: '',
  });

  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }

    const fetchUser = async () => {
      try {
        const response = await axios.get('http://localhost:5146/api/dashboard/summary', {
          headers: { Authorization: `Bearer ${token}` },
        });

        const { user: userData } = response.data;

        setUser({
          fullName: userData.fullName || '',
          email: userData.email || '',
          role: userData.role || 'User',
          joinDate: userData.joinDate || '',
        });
      } catch (err) {
        console.error('Error fetching user info', err);
        navigate('/login');
      }
    };

    fetchUser();
  }, [navigate]);

  const logout = () => {
    localStorage.clear();
    navigate('/login');
  };

  return (
    <div className="w-screen min-h-screen bg-gradient-to-tr from-[#e0f7f1] via-white to-[#d4e7ff] p-6 md:p-16 font-[Segoe_UI] overflow-hidden">
      
      {/* Back Button */}
      <a
        href="/dashboard"
        className="inline-flex items-center text-[18px] font-semibold text-[#1a4b6d] mb-8 hover:text-[#27ae60] transition-all duration-300"
      >
        <span className="mr-2 text-xl">←</span> Back to Dashboard
      </a>

      {/* Profile Container */}
      <div className="w-full h-full max-w-7xl mx-auto bg-white shadow-2xl rounded-3xl p-8 md:p-14 lg:p-20 flex flex-col md:flex-row items-center gap-10 border border-gray-100 transition-transform duration-300 hover:scale-[1.01]">

        {/* Avatar + Logout */}
        <div className="flex flex-col items-center md:items-start">
          <div className="w-36 h-36 rounded-full bg-gradient-to-br from-[#27ae60] to-[#1a4b6d] text-white shadow-xl flex items-center justify-center text-6xl font-bold tracking-wide animate-pulse">
            {user.fullName ? user.fullName[0].toUpperCase() : user.email[0]?.toUpperCase()}
          </div>

          <button
            onClick={logout}
            className="mt-6 px-6 py-2 rounded-xl bg-[#102d3f] text-white font-medium hover:bg-[#0c2232] transition-all duration-200 shadow-md"
          >
            Logout
          </button>
        </div>

        {/* Info Section */}
        <div className="w-full space-y-3">
          <h1 className="text-4xl font-bold text-[#102d3f]">Welcome to Your Profile</h1>
          <p className="text-gray-500 text-[15px] mb-4">
            Stay on top of your tasks with a personalized dashboard view.
          </p>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div>
              <p className="text-gray-600 font-semibold">Full Name</p>
              <p className="text-lg font-medium text-[#102d3f]">{user.fullName}</p>
            </div>
            <div>
              <p className="text-gray-600 font-semibold">Email</p>
              <p className="text-lg font-medium text-[#102d3f]">{user.email}</p>
            </div>
            <div>
              <p className="text-gray-600 font-semibold">Role</p>
              <p className="text-lg font-medium text-[#102d3f]">{user.role}</p>
            </div>
            {user.joinDate && (
              <div>
                <p className="text-gray-600 font-semibold">Join Date</p>
                <p className="text-lg font-medium text-[#102d3f]">{new Date(user.joinDate).toDateString()}</p>
              </div>
            )}
            <div>
              <p className="text-gray-600 font-semibold">Status</p>
              <p className="text-green-600 font-semibold text-lg">Active</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default UserProfile;
