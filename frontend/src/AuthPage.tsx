import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface AuthData {
  email: string;
  passwordHash: string;
  fullName?: string;
}

interface AuthResponse {
  token?: string;
  role?: string;
  fullName?: string;
  message?: string;
}

const AuthPage: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [message, setMessage] = useState<string | null>(null);
  const navigate = useNavigate();

  const toggleMode = () => {
    setIsLogin(!isLogin);
    setMessage(null);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const formData = new FormData(form);

    const email = formData.get("email")?.toString().trim().toLowerCase() || "";
    const password = formData.get("password")?.toString() || "";
    const fullName = formData.get("fullName")?.toString().trim();

    const data: AuthData = {
      email,
      passwordHash: password,
      ...(fullName ? { fullName } : {})
    };

    try {
      const response = await fetch(`http://localhost:5146/api/auth/${isLogin ? "login" : "signup"}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });

      const text = await response.text();
      let result: AuthResponse = {};

      try {
        result = JSON.parse(text);
      } catch {
        result.message = text;
      }

      if (!response.ok) throw new Error(result.message || "Something went wrong");

      if (isLogin) {
        if (result.token) {
          localStorage.setItem("token", result.token);
          localStorage.setItem("role", result.role || "");
          localStorage.setItem("fullName", result.fullName || "");

          // 🔥 Redirect according to role
          if (result.role === "Admin") {
            navigate("/admin-dashboard");
          } else {
            navigate("/dashboard");
          }

          setMessage(result.message || `Login successful as ${result.role}`);
        } else {
          setMessage("Login failed: No token received");
        }
      } else {
        setMessage(result.message || "Signup successful");
      }

      form.reset();
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Unexpected error occurred";
      setMessage(errorMsg);
    }

    setTimeout(() => setMessage(null), 4000);
  };

  return (
    <div className="w-screen h-screen bg-[#f8fafc] text-[#333] overflow-y-auto p-6 md:p-10 font-[Montserrat] flex items-center justify-center">
      <div className={`relative overflow-hidden w-[768px] max-w-full min-h-[480px] bg-white rounded-[30px] shadow-[0_5px_15px_rgba(0,0,0,0.35)] transition-all duration-500 ${!isLogin ? 'active' : ''}`}>

        {/* Sign Up Form */}
        <div className={`absolute top-0 left-0 h-full w-1/2 z-[1] transition-all duration-500 ${!isLogin ? 'opacity-100 z-[5] translate-x-full animate-fadeIn' : 'opacity-0'}`}>
          <form onSubmit={handleSubmit} className="bg-white flex flex-col justify-center items-center px-10 h-full">
            <h2 className="text-2xl font-semibold mb-4">Create Account</h2>
            {!isLogin && (
              <input type="text" name="fullName" placeholder="Full Name" required
                className="w-full bg-gray-200 text-black placeholder-black px-4 py-2 text-sm rounded-md mb-2 outline-none" />
            )}
            <input type="email" name="email" placeholder="Email" required
              className="w-full bg-gray-200 text-black placeholder-black px-4 py-2 text-sm rounded-md mb-2 outline-none" />
            <input type="password" name="password" placeholder="Password" required
              className="w-full bg-gray-200 text-black placeholder-black px-4 py-2 text-sm rounded-md mb-2 outline-none" />
            <button type="submit"
              className="bg-[#002f34] text-[#dff0d8] uppercase text-xs py-2 px-12 mt-3 rounded-md font-semibold tracking-wide">Sign Up</button>
          </form>
        </div>

        {/* Sign In Form */}
        <div className={`absolute top-0 left-0 h-full w-1/2 z-[2] transition-all duration-500 ${!isLogin ? 'translate-x-full' : ''}`}>
          <form onSubmit={handleSubmit} className="bg-white flex flex-col justify-center items-center px-10 h-full">
            <h2 className="text-2xl font-semibold mb-4">Sign In</h2>
            <input type="email" name="email" placeholder="Email" required
              className="w-full bg-gray-200 text-black placeholder-black px-4 py-2 text-sm rounded-md mb-2 outline-none" />
            <input type="password" name="password" placeholder="Password" required
              className="w-full bg-gray-200 text-black placeholder-black px-4 py-2 text-sm rounded-md mb-2 outline-none" />
            <button type="submit"
              className="bg-[#002f34] text-[#dff0d8] uppercase text-xs py-2 px-12 mt-3 rounded-md font-semibold tracking-wide">Login</button>
          </form>
        </div>

        {/* Message Box */}
        {message && (
          <div className="absolute bottom-5 left-1/2 transform -translate-x-1/2 text-center w-4/5 px-5 py-3 bg-[#dff0d8] text-[#3c763d] border border-[#3c763d] rounded-md z-[999] animate-fadeInOut text-sm font-medium">
            {message}
          </div>
        )}

        {/* Toggle Container */}
        <div className={`absolute top-0 left-1/2 h-full w-1/2 overflow-hidden transition-all duration-500 z-[1000] ${!isLogin ? 'translate-x-[-100%] rounded-r-[100px]' : 'rounded-l-[150px]'}`}>
          <div className={`bg-[#002f34] h-full w-[200%] text-white absolute left-[-100%] flex transition-all duration-500 ${!isLogin ? 'translate-x-1/2' : ''}`}>

            {/* Toggle Left Panel */}
            <div className={`w-1/2 h-full flex flex-col justify-center items-center text-center px-6 transition-all duration-500 ${!isLogin ? 'translate-x-0' : '-translate-x-[200%]'}`}>
              <h2 className="text-2xl font-semibold mb-2">Welcome Back!</h2>
              <p className="text-sm mb-4">Login with your personal info</p>
              <button onClick={toggleMode}
                className="bg-white text-[#002f34] text-xs uppercase py-2 px-6 rounded-md font-semibold tracking-wide">Sign In</button>
            </div>

            {/* Toggle Right Panel */}
            <div className={`w-1/2 h-full flex flex-col justify-center items-center text-center px-6 transition-all duration-500 ${!isLogin ? 'translate-x-[200%]' : 'translate-x-0'}`}>
              <h2 className="text-2xl font-semibold mb-2">Hello, Friend!</h2>
              <p className="text-sm mb-4">Start your journey by signing up</p>
              <button onClick={toggleMode}
                className="bg-white text-[#002f34] text-xs uppercase py-2 px-6 rounded-md font-semibold tracking-wide">Sign Up</button>
            </div>

          </div>
        </div>

      </div>
    </div>
  );
};

export default AuthPage;
