import React, { useState } from 'react';
import './auth.css';

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

  const toggleMode = () => {
    setIsLogin(!isLogin);
    setMessage(null);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const formData = new FormData(form);

    const email = formData.get("email")?.toString().trim() || "";
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

      const text = await response.text(); // body stream used here only once
      let result: AuthResponse = {};

      try {
        result = JSON.parse(text);
      } catch {
        result.message = text; // fallback if not valid JSON
      }

      if (!response.ok) {
        throw new Error(result.message || "Something went wrong");
      }

      if (isLogin) {
        if (result.token) {
          localStorage.setItem("token", result.token);
          localStorage.setItem("role", result.role || "");
          localStorage.setItem("fullName", result.fullName || "");
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
    <div className={`container ${!isLogin ? 'active' : ''}`}>
      {/* Sign Up */}
      <div className="form-container sign-up">
        <form onSubmit={handleSubmit}>
          <h2>Create Account</h2>
          {!isLogin && <input type="text" name="fullName" placeholder="Full Name" required />}
          <input type="email" name="email" placeholder="Email" required />
          <input type="password" name="password" placeholder="Password" required />
          <button type="submit">Sign Up</button>
        </form>
      </div>

      {/* Sign In */}
      <div className="form-container sign-in">
        <form onSubmit={handleSubmit}>
          <h2>Sign In</h2>
          <input type="email" name="email" placeholder="Email" required />
          <input type="password" name="password" placeholder="Password" required />
          <button type="submit">Login</button>
        </form>
      </div>

      {/* ✅ Message Box */}
      {message && <div className="message-box">{message}</div>}

      {/* Toggle Panel */}
      <div className="toggle-container">
        <div className="toggle">
          <div className="toggle-panel toggle-left">
            <h2>Welcome Back!</h2>
            <p>Login with your personal info</p>
            <button className="hidden" onClick={toggleMode}>Sign In</button>
          </div>
          <div className="toggle-panel toggle-right">
            <h2>Hello, Friend!</h2>
            <p>Start your journey by signing up</p>
            <button className="hidden" onClick={toggleMode}>Sign Up</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AuthPage;
