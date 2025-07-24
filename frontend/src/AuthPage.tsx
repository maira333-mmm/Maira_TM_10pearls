import React, { useState } from 'react';
import './auth.css';

interface AuthData {
  email: string;
  passwordHash: string;
  fullName?: string;
}

interface Message {
  type: 'success' | 'error';
  text: string;
}

const AuthPage: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [message, setMessage] = useState<Message | null>(null);

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
    const fullName = !isLogin ? formData.get("fullName")?.toString().trim() || "" : undefined;

    if (!email || !password || (!isLogin && !fullName)) {
      setMessage({ type: "error", text: "Please fill in all required fields." });
      return;
    }

    const data: AuthData = { email, passwordHash: password, fullName };

    try {
      const url = isLogin
        ? "http://localhost:5146/api/auth/login"
        : "http://localhost:5146/api/auth/signup";

      const response = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });

      // Read response as text only once
      const text = await response.text();

      let resData;
      try {
        resData = JSON.parse(text);
      } catch {
        throw new Error(text || 'Unexpected response from server');
      }

      if (!response.ok) {
        throw new Error(resData.message || 'Something went wrong');
      }

      // ✅ Success
      setMessage({ type: "success", text: resData.message });
      form.reset();
    } catch (err: unknown) {
      const errorText = err instanceof Error ? err.message : 'Unknown error occurred';
      setMessage({ type: "error", text: errorText });
    }

    setTimeout(() => setMessage(null), 3000);
  };

  return (
    <div className={`container ${!isLogin ? 'active' : ''}`}>
      <div className="form-container sign-up">
        <form onSubmit={handleSubmit}>
          <h2>Create Account</h2>
          {!isLogin && message && <div className={`message ${message.type}`}>{message.text}</div>}
          {!isLogin && (
            <>
              <input type="text" name="fullName" placeholder="Full Name" required />
              <input type="email" name="email" placeholder="Email" required />
              <input type="password" name="password" placeholder="Password" required />
              <button type="submit">Sign Up</button>
            </>
          )}
        </form>
      </div>

      <div className="form-container sign-in">
        <form onSubmit={handleSubmit}>
          <h2>Sign In</h2>
          {isLogin && message && <div className={`message ${message.type}`}>{message.text}</div>}
          {isLogin && (
            <>
              <input type="email" name="email" placeholder="Email" required />
              <input type="password" name="password" placeholder="Password" required />
              <button type="submit">Login</button>
            </>
          )}
        </form>
      </div>

      <div className="toggle-container">
        <div className="toggle">
          <div className="toggle-panel toggle-left">
            <h2>Welcome Back!</h2>
            <button className="hidden" type="button" onClick={toggleMode}>Sign In</button>
          </div>
          <div className="toggle-panel toggle-right">
            <h2>Hello, Friend!</h2>
            <button className="hidden" type="button" onClick={toggleMode}>Sign Up</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AuthPage;
