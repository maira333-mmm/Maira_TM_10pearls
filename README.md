<div align="center">

<img width="100%" src="https://capsule-render.vercel.app/api?type=waving&height=210&color=0:1E3A5F,50:2563EB,100:60A5FA&text=Task%20Management%20System&fontColor=ffffff&fontSize=50&fontAlignY=38&desc=Full-Stack%20%7C%20.NET%20%7C%20React%20%7C%20TypeScript&descAlignY=60&animation=fadeIn" alt="Task Management System Header" />

<br>

<img src="https://readme-typing-svg.demolab.com?font=Inter&weight=600&size=20&duration=2800&pause=700&color=2563EB&center=true&vCenter=true&repeat=true&width=700&height=52&lines=Full-Stack+Task+Management+Application.;Built+with+.NET+Core+%7C+React+%7C+TypeScript.;JWT+Authentication+%7C+Admin+%26+User+Panels.;Unit+Testing+with+xUnit+%26+Moq." alt="Typing Animation" />

<br><br>

A comprehensive **Full-Stack Task Management System** built with **.NET Core (C#) backend** and **React + TypeScript frontend**. Features JWT authentication, role-based access control (Admin/User), task CRUD operations, and an admin dashboard with user management.

<br>

<a href="https://github.com/maira333-mmm/Maira_TM_10pearls">
  <img src="https://img.shields.io/badge/📂_SOURCE_CODE-181717?style=for-the-badge&logo=github&logoColor=white" alt="Source Code"/>
</a>

<a href="https://github.com/maira333-mmm/Maira_TM_10pearls/commits/main">
  <img src="https://img.shields.io/github/last-commit/maira333-mmm/Maira_TM_10pearls?style=for-the-badge&label=LAST%20UPDATE" alt="Last Update"/>
</a>

<br><br>

<img src="https://img.shields.io/badge/.NET_Core-512BD4?style=flat-square&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/React-20232A?style=flat-square&logo=react&logoColor=61DAFB"/>
<img src="https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white"/>
<img src="https://img.shields.io/badge/Entity_Framework-512BD4?style=flat-square&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/JWT-000000?style=flat-square&logo=jsonwebtokens&logoColor=white"/>
<img src="https://img.shields.io/badge/xUnit-5E2B97?style=flat-square&logo=xunit&logoColor=white"/>
<img src="https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=flat-square&logo=tailwind-css&logoColor=white"/>

</div>

---

# 📋 Table of Contents

- 📖 About
- ✨ Features
- 🏗️ Architecture
- 📁 Project Structure
- 🗄️ Database Schema
- 🔐 Authentication & Authorization
- 🚀 Getting Started
- 🔧 Installation
- 💻 Usage Guide
- 📊 API Endpoints
- 🧪 Testing
- 🎨 UI/UX Design
- 🛠 Technologies Used
- 🌍 Browser Compatibility
- 🤝 Contributing
- 📬 Contact
- 📄 License
- 🙏 Acknowledgements

---

# 📖 About

The **Task Management System** is a full-stack web application designed to help users and administrators manage tasks efficiently. It features a modern React frontend with a robust .NET Core backend API, providing secure authentication, role-based access control, and comprehensive task management capabilities.

## 🎯 Key Highlights

- ✅ **User Authentication** - JWT-based secure login/signup
- ✅ **Role-Based Access** - Admin and User roles with different permissions
- ✅ **Task Management** - Create, read, update, delete tasks
- ✅ **Admin Dashboard** - User management, task oversight, activity monitoring
- ✅ **Task Filtering** - Filter by status, search by title
- ✅ **Unit Testing** - xUnit and Moq for backend testing
- ✅ **Modern UI** - Tailwind CSS with responsive design

---

# ✨ Features

## 👤 User Features

| Feature | Description |
|---------|-------------|
| 🔐 **Authentication** | Sign up / Sign in with JWT |
| 📝 **Task CRUD** | Create, view, edit, delete tasks |
| 🔍 **Task Filtering** | Filter by status (Completed, In Progress, Pending) |
| 🔎 **Task Search** | Search tasks by title |
| 📊 **Dashboard** | View task statistics (completed, pending, in-progress) |
| 👤 **Profile** | View and manage profile information |

## 👑 Admin Features

| Feature | Description |
|---------|-------------|
| 👥 **User Management** | View all users, activate/deactivate accounts |
| 📋 **Task Oversight** | View all tasks across all users |
| 🗑️ **Task Management** | Delete any task |
| 📊 **Admin Dashboard** | Comprehensive statistics and user analytics |
| ✏️ **Task Editing** | Edit any task |
| 📈 **Activity Monitoring** | Track user activity |

---

# 🏗️ Architecture

## System Architecture

```text
┌─────────────────────────────────────────────────────────────────┐
│                      Frontend (React + TypeScript)              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │   Pages     │  │ Components  │  │   Services (API Calls)   │ │
│  │  (TSX)      │  │   (TSX)     │  │                         │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Backend (.NET Core 8 API)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │ Controllers │  │  Services   │  │   DTOs / Models          │ │
│  │  (API)      │  │  (Business) │  │                         │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Database (SQL Server)                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │   Users     │  │  UserTasks  │  │  UserLoginAttempts       │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘

# 🏗️ System Architecture

The application follows a modern client-server architecture consisting of a React frontend, ASP.NET Core Web API backend, Entity Framework Core, and SQL Server.

```text
                     ┌──────────────────────────────┐
                     │      React Frontend          │
                     │   (TypeScript + Vite)        │
                     └──────────────┬───────────────┘
                                    │
                             HTTP / REST API
                                    │
                                    ▼
                     ┌──────────────────────────────┐
                     │     ASP.NET Core Web API     │
                     │ JWT Authentication & RBAC    │
                     └──────────────┬───────────────┘
                                    │
                    Entity Framework Core (ORM)
                                    │
                                    ▼
                     ┌──────────────────────────────┐
                     │        SQL Server DB         │
                     │ Users • Tasks • Login Logs   │
                     └──────────────────────────────┘
```

---

# 🔐 Authentication Flow

```text
User Login
    │
    ▼
Enter Email & Password
    │
    ▼
Backend Validates Credentials
    │
    ▼
Generate JWT Token
    │
    ▼
Return Token to React
    │
    ▼
Store Token in Local Storage
    │
    ▼
Attach Token to API Requests
    │
    ▼
Backend Validates JWT
    │
    ▼
Role-Based Authorization
    │
    ▼
Access Protected Resources
```

---

# 📁 Project Structure

```text
Maira_TM_10pearls/
│
├── README.md
├── LICENSE
├── Maira_TM_10Pearls.sln
├── sonar-project.properties
│
├── backend/
│   ├── Controllers/
│   ├── DTO/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   ├── Migrations/
│   ├── Properties/
│   ├── appsettings.json
│   ├── Program.cs
│   └── backend.csproj
│
├── Backend.Tests/
│   ├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── DashboardControllerTests.cs
│   └── Backend.Tests.csproj
│
├── frontend/
│   ├── public/
│   ├── src/
│   │   ├── pages/
│   │   ├── App.tsx
│   │   ├── main.tsx
│   │   └── index.css
│   ├── package.json
│   ├── vite.config.ts
│   └── tailwind.config.js
│
├── .gitignore
└── Project_Video_Demo/
```

---

# 🗄️ Database Design

## Users

| Column | Type |
|---------|------|
| Id | INT |
| FullName | NVARCHAR(100) |
| Email | NVARCHAR(100) |
| PasswordHash | NVARCHAR(255) |
| Role | NVARCHAR(50) |
| IsActive | BIT |
| CreatedAt | DATETIME |
| LastLogin | DATETIME |

---

## UserTasks

| Column | Type |
|---------|------|
| Id | INT |
| Title | NVARCHAR(200) |
| Description | NVARCHAR(MAX) |
| Status | NVARCHAR(50) |
| Priority | NVARCHAR(50) |
| DueDate | DATETIME |
| UserId | INT |

---

## UserLoginAttempts

| Column | Type |
|---------|------|
| Id | INT |
| UserId | INT |
| IsSuccessful | BIT |
| AttemptTime | DATETIME |
| IPAddress | NVARCHAR(50) |
| UserAgent | NVARCHAR(255) |

---

# 🔐 Authentication & Authorization

## JWT Authentication

The application uses JSON Web Tokens (JWT) for secure authentication.

Authentication process:

- User logs in with email and password
- Credentials are validated
- JWT token is generated
- Token is stored in Local Storage
- Every API request includes the token
- Backend validates the token
- User permissions are determined by role

---

## Password Security

Passwords are securely hashed using **SHA-256** before storage.

---

## User Roles

| Role | Permissions |
|------|-------------|
| User | Manage own tasks |
| Admin | Manage all users and tasks |

---

# 🚀 Getting Started

## Requirements

- .NET 8 SDK
- Node.js 18+
- SQL Server
- npm
- Visual Studio 2022 / VS Code

---

## Clone Repository

```bash
git clone https://github.com/maira333-mmm/Maira_TM_10pearls.git

cd Maira_TM_10pearls
```

---

## Backend Setup

```bash
cd backend

dotnet restore

dotnet ef database update

dotnet run
```

Backend

```
http://localhost:5146
```

---

## Frontend Setup

```bash
cd frontend

npm install

npm run dev
```

Frontend

```
http://localhost:5173
```

---

## Environment Variables

Create

```
.env
```

```env
VITE_API_URL=http://localhost:5146/api
```

---

# 💻 Features

## Authentication

- User Registration
- User Login
- JWT Authentication
- Protected Routes
- Role Based Authorization

---

## Task Management

- Create Task
- Edit Task
- Delete Task
- View Task
- Search Tasks
- Filter Tasks
- Status Management

---

## Dashboard

- Task Statistics
- Completed Tasks
- Pending Tasks
- In Progress Tasks
- User Profile

---

## Admin Panel

- User Management
- Activate / Deactivate Users
- View All Tasks
- Delete Any Task
- Dashboard Analytics

---

# 📊 API Endpoints

## Authentication

| Method | Endpoint |
|---------|----------|
| POST | /api/auth/signup |
| POST | /api/auth/login |

---

## Tasks

| Method | Endpoint |
|---------|----------|
| GET | /api/tasks |
| GET | /api/tasks/{id} |
| POST | /api/tasks/create |
| PUT | /api/tasks/{id} |
| DELETE | /api/tasks/{id} |

---

## Dashboard

| Method | Endpoint |
|---------|----------|
| GET | /api/dashboard/summary |

---

## Admin

| Method | Endpoint |
|---------|----------|
| GET | /api/admin-dashboard/summary |
| GET | /api/admin-dashboard/users |
| PUT | /api/admin-dashboard/toggle-active/{id} |
| DELETE | /api/admin-dashboard/delete-task/{id} |
| POST | /api/admin/tasks |
| PUT | /api/admin/tasks/{id} |
| DELETE | /api/admin/tasks/{id} |

---

# 🧪 Testing

Run backend tests

```bash
cd Backend.Tests

dotnet test
```

---

# 🎨 UI Features

- Responsive Layout
- Sidebar Navigation
- Dashboard Cards
- Status Badges
- Glassmorphism UI
- Gradient Buttons
- Mobile Friendly

---

# 🛠️ Tech Stack

## Backend

- ASP.NET Core 8
- C#
- Entity Framework Core
- SQL Server
- JWT Authentication
- xUnit
- Moq
- Serilog

## Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- Axios
- React Router
- Chart.js

---

# 🌐 Browser Support

| Browser | Support |
|----------|---------|
| Chrome | ✅ |
| Firefox | ✅ |
| Edge | ✅ |
| Safari | ✅ |
| Opera | ✅ |
| Mobile | ✅ |

---

# 🤝 Contributing

```text
Fork Repository
      │
      ▼
Create Feature Branch
      │
      ▼
Commit Changes
      │
      ▼
Push Branch
      │
      ▼
Open Pull Request
```

---


# 📬 Contact


<div align="center">

## 👩‍💻 Maira Alam

<a href="https://mail.google.com/mail/?view=cm&fs=1&to=maira.alam33@gmail.com">
  <img src="https://img.shields.io/badge/Gmail-EA4335?style=for-the-badge&logo=gmail&logoColor=white" alt="Gmail"/>
</a>

<a href="https://github.com/maira333-mmm">
<img src="https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white"/>
</a>

<a href="https://www.linkedin.com/in/maira-a-48699630b/">
<img src="https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white"/>
</a>

<a href="https://maira-alam-o2p20gi.gamma.site/">
<img src="https://img.shields.io/badge/Portfolio-2563EB?style=for-the-badge&logo=googlechrome&logoColor=white"/>
</a>

</div>

---

---

# 📄 License

Licensed under the **MIT License**.

---

# 🚀 Future Enhancements

| Feature | Description |
|----------|-------------|
| 📱 Mobile App | React Native |
| 🔔 Notifications | Email & SMS |
| 📊 Analytics | Productivity Reports |
| 🌐 Multi-language | Localization |
| 🗓 Calendar View | Calendar Integration |
| 🏷 Labels | Task Categories |
| 👥 Collaboration | Task Assignment |

---

# 🐛 Troubleshooting

<details>
<summary><b>Backend Not Starting</b></summary>

```bash
dotnet ef database update

dotnet run
```

</details>

<details>
<summary><b>Frontend Build Failing</b></summary>

```bash
rm -rf node_modules package-lock.json

npm install

npm run build
```

</details>

<details>
<summary><b>CORS Errors</b></summary>

Ensure the frontend URL is allowed in **Program.cs** and verify that both frontend and backend are running.

</details>

---

# 🙏 Acknowledgements

- ASP.NET Core
- React
- Entity Framework Core
- SQL Server
- JWT Authentication
- Tailwind CSS
- GitHub

---
<div align="center">

## ❤️ Built with .NET Core, React & TypeScript

### Made with 💙 by **Maira Alam**

⭐ **If you found this project helpful, consider giving it a Star!**

<br>

## 🚀 Live Demo

<a href="https://maira-tm-10pearls.vercel.app">
  <img src="https://img.shields.io/badge/🌐_LIVE_DEMO-000000?style=for-the-badge&logo=vercel&logoColor=white" alt="Live Demo"/>
</a>

<br><br>

<a href="https://github.com/maira333-mmm/Maira_TM_10pearls/commits/main">
  <img src="https://img.shields.io/github/last-commit/maira333-mmm/Maira_TM_10pearls?style=for-the-badge&label=LAST%20UPDATE" alt="Last Update"/>
</a>

</div>









