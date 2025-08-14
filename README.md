# Maira_TM_10pearls
A full‑stack Task Management System built with **ASP.NET Core Web API** and **React (TypeScript)**, using **SQL Server**, **JWT auth**, **Serilog logging**, and **SonarQube** for code quality.

---

# Task Management System

## 📌 Overview
This project is a **Task Management System** with:
- **Backend**: ASP.NET Core Web API (.NET 6+)
- **Frontend**: React + Vite + TypeScript
- **Database**: SQL Server
- **Auth**: JWT
- **Logging**: Serilog
- **Code Quality**: SonarQube

---

## 📂 Project Structure
```
Maira_TM_10Pearls/
│
├── Backend/                     # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Properties/
│   └── Program.cs
│
├── frontend/                    # React + Vite + TypeScript
│   ├── src/
│   ├── public/
│   └── vite.config.ts
│
└── README.md
```

---

## 📋 Table of Contents
- [Prerequisites](#prerequisites)
- [Backend Setup](#backend-setup)
- [Frontend Setup](#frontend-setup)
- [SonarQube Analysis](#sonarqube-analysis)
- [Serilog Setup](#serilog-setup)
- [Configuration Files](#configuration-files)
- [Tech Stack](#tech-stack)
- [Additional Information](#additional-information)

---

## 🔹 Prerequisites
Install the following:
- [.NET SDK 6.0+](https://dotnet.microsoft.com/en-us/download)
- [Node.js 14+](https://nodejs.org/)
- npm (comes with Node.js)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SonarQube](https://www.sonarqube.org/downloads/)
- (Optional) [dotnet-sonarscanner](https://docs.sonarsource.com/sonarqube/latest/analyzing-source-code/scanners/sonarscanner-for-dotnet/)

---

## 🚀 Backend Setup

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/maira333-mmm/Maira_TM_10pearls.git
cd Maira_TM_10pearls/Backend
```

### 2️⃣ Configure Database and JWT
Create or edit **Backend/appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME\\SQLEXPRESS;Database=TaskManagement;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "<YOUR_JWT_SECRET_KEY>",
    "Issuer": "Taskmanagement.com",
    "Audience": "Taskmanagement.com"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "Logs/log-.txt", "rollingInterval": "Day" } }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> 🔒 **Security Note**: Never commit real secrets to Git. Use [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables for `Jwt:Key` in development, and a secure secret store in production.

### 3️⃣ Run EF Core Migrations
```bash
dotnet tool install --global dotnet-ef   # run once if not installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4️⃣ Restore & Run
```bash
dotnet restore
dotnet run
```
Backend API default URL:
```
http://localhost:5146
```

> If your port differs, check `Properties/launchSettings.json` or console output.

---

## 🎨 Frontend Setup

### 1️⃣ Navigate to Frontend
```bash
cd ../frontend
```

### 2️⃣ Install Dependencies
```bash
npm install
```

### 3️⃣ Start Frontend
```bash
npm run dev
```
Frontend default URL:
```
http://localhost:5173
```

---

## 📊 SonarQube Analysis
Run analysis from the **Backend** directory (ensure SonarQube server is running at `http://localhost:9000` and you have a token). Replace placeholders with your values.

```bash
# 1) Begin (replace values)
dotnet sonarscanner begin \
/k:"Maira_TM_10Pearls_Backend" \
/d:sonar.login="<YOUR_SONAR_TOKEN>" \
/d:sonar.host.url="http://localhost:9000" \
/d:sonar.cs.opencover.reportsPaths="C:\\Path\\To\\Backend.Tests\\TestResults\\<GUID>\\coverage.cobertura.xml"

# 2) Build
dotnet build

# 3) End (replace token)
dotnet sonarscanner end /d:sonar.login="<YOUR_SONAR_TOKEN>"
```

> 💡 **Tip**: If you use `coverlet.collector` with xUnit/NUnit/MSTest, run tests with coverage first, then pass the generated `coverage.cobertura.xml` path to Sonar.

---

## 🧾 Serilog Setup

### Option A — Configure via `Program.cs`
```csharp
using Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Serilog: read from appsettings.json and add sinks
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// 2) DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 4) JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            Log.Error(exceptionHandlerPathFeature.Error, "Unhandled exception occurred");
        }

        await context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error. Please try again later."
        });
    });
});

// Request logging
app.UseSerilogRequestLogging();

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Flush logs on shutdown
Log.CloseAndFlush();
```

### Option B — Configure via `appsettings.json`
Already included above (see `Serilog` section). Logs will be written to console and `Logs/log-.txt` with daily rolling.

---

## ⚙ Configuration Files

**`Backend/Properties/launchSettings.json`**
```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Backend": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5146",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false",
      "applicationUrl": "https://localhost:7103;http://localhost:5146",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**`frontend/vite.config.ts`**
```ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    allowedHosts: ['.ngrok-free.app']
  }
})
```

**`frontend/package.json`**
```json
{
  "name": "frontend",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "lint": "eslint .",
    "preview": "vite preview"
  },
  "dependencies": {
    "axios": "^1.11.0",
    "chart.js": "^4.5.0",
    "react": "^19.1.0",
    "react-chartjs-2": "^5.3.0",
    "react-dom": "^19.1.0",
    "react-helmet": "^6.1.0",
    "react-router-dom": "^7.7.1"
  },
  "devDependencies": {
    "@eslint/js": "^9.30.1",
    "@types/react": "^19.1.9",
    "@types/react-dom": "^19.1.7",
    "@vitejs/plugin-react": "^4.6.0",
    "autoprefixer": "^10.4.21",
    "eslint": "^9.30.1",
    "eslint-plugin-react-hooks": "^5.2.0",
    "eslint-plugin-react-refresh": "^0.4.20",
    "globals": "^16.3.0",
    "postcss": "^8.5.6",
    "tailwindcss": "^3.4.3",
    "typescript": "~5.8.3",
    "typescript-eslint": "^8.35.1",
    "vite": "^7.0.4"
  }
}
```

---

## 🛠 Tech Stack
**Backend:**
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Serilog Logging
- JWT Authentication

**Frontend:**
- React 19 + TypeScript
- Vite
- Axios
- TailwindCSS
- Chart.js

**Testing & Quality:**
- xUnit (planned)
- SonarQube

---

## 📌 Additional Information
- API (dev): `http://localhost:5146`
- Frontend (dev): `http://localhost:5173`
- Ensure **SQL Server** is running before starting the backend.
- Replace all placeholders (`YOUR_SERVER_NAME`, `<YOUR_JWT_SECRET_KEY>`, `<YOUR_SONAR_TOKEN>`, coverage path, etc.) with your actual values.

---
