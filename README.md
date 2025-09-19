# osu! Sweep 🧹🎶

<p align="center">
  <a href="./docs/README.pt.md">
    <img src="./assets/flag_br.png" alt="Português" width="30"/>
    &nbsp;Português
  </a>
  &nbsp;&nbsp;
  <a href="./docs/README.es.md">
    <img src="./assets/flag_es.png" alt="Español" width="30"/>
    &nbsp;Español
  </a>
</p>

A **secure, smart, and user-friendly desktop tool** for managing and cleaning large collections of **osu!** beatmaps, based on the game modes you actually play.

<p align="center">
  <img src="https://i.imgur.com/link_para_seu_gif_aqui.gif" alt="osu! Sweep Demo" width="700"/>
</p>

---

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Languages](https://img.shields.io/badge/languages-3-blue.svg)
![Tech](https://img.shields.io/badge/.NET-8.0-purple.svg)

## 🚦 Project Status

**In Active Development (Alpha Version)**

This project is a work in progress. The core architecture and business logic (map analysis, API communication, deletion calculations, and unit tests) are well-structured and functional. The user interface (UI) is being migrated from WPF to Avalonia UI for cross-platform support.

## 📌 About the Project

**osu! Sweep** was created to address the challenge of managing massive osu! beatmap libraries, which can take up **hundreds of GBs** of disk space. The goal is to provide a tool that helps players **reclaim space and organize their maps** in a **safe and customizable** way.

---

## ✨ Features

- **🔍 Hybrid Beatmap Identification**
  - Uses the **osu! API v2** as the primary method, with OAuth 2.0 authentication.
  - Falls back to **deep scanning of `.osu` files** for local maps.

- **🧠 Smart Deletion Logic**
  - Removes entire folders or just `.osu` files for unwanted game modes.
  - Preserves the game modes you choose to keep.

- **🛡️ User Safety First**
  - Displays a clear confirmation screen before any deletion.
  - By default, moves files to the **Recycle Bin**, with an option for permanent deletion.

- **🌐 Internationalization (i18n)**
  - Supports multiple languages (Portuguese, English, Spanish) with real-time switching.

---

## 🛠️ Technologies Used

- **Frontend (Desktop App):**
  - C# with **Avalonia UI** (.NET 8) for native support on Windows and Linux.
  - **MVVM (Model-View-ViewModel)** pattern.

- **Backend (Serverless API):**
  - Azure Functions in C# (.NET 8 Isolated Worker).
  - Secure proxy for the osu! API v2 with OAuth 2.0 token management.

- **Testing:**
  - MSTest (for unit and integration tests).
  - Moq (for mocking dependencies).

---

## 📂 Project Structure

```bash
OsuSweep.sln
├── OsuSweep/                 # Desktop application (Frontend - UI)
├── OsuSweep.Backend/         # Azure Functions (Backend - API Proxy)
├── OsuSweep.Core/            # Business logic, contracts, and shared models
├── OsuSweep.Tests.Unit/      # Fast unit tests
└── OsuSweep.Tests.Integration/ # Integration tests (I/O)
```

---

## 🚀 How to Run

Instructions for building and running the project from source.

### 📋 Prerequisites

- .NET 8 SDK
- Visual Studio 2022 (with ".NET desktop development" and "Azure development" workloads)
- An osu! account to generate API credentials (OAuth)

### ▶️ Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/OsuSweep.git
   cd OsuSweep
   ```

2. Configure API credentials:

   - In the `OsuSweep.Backend` project, create a file named `local.settings.json`.
   - Fill it with your `Client ID` and `Client Secret`:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "OSU_API_CLIENT_ID": "YOUR_CLIENT_ID_HERE",
       "OSU_API_CLIENT_SECRET": "YOUR_CLIENT_SECRET_HERE",
       "Host": {
         "CORS": "*"
       }
     }
   }
   ```

3. Run the application:

   - Open the `OsuSweep.sln` solution in Visual Studio.
   - Right-click the Solution > *Configure Startup Projects...* and set both `OsuSweep` and `OsuSweep.Backend` to start.
   - Press F5 to run.

---

## 🗺️ Roadmap

- [x] Initial project structure and business logic.
- [x] Implementation of osu! API v2 and manual analysis.
- [x] Testing architecture (unit and integration).
- [x] Multi-language support (i18n).
- [ ] UI migration to Avalonia: Rewrite the presentation layer (Views) from WPF to Avalonia UI to create a cross-platform foundation.
- [ ] Implement Linux support: Develop Linux-specific filesystem logic (e.g., locate the `Songs` folder, move to Trash).
- [ ] UI/UX improvements with an osu!-inspired theme (post-migration).
- [ ] Advanced settings (e.g., delete videos/storyboards).
- [ ] Publish an installable release on GitHub.

---

## 📜 License

Distributed under the MIT License. See the [LICENSE](LICENSE) file for more information.