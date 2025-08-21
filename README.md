[<img src="./assets/br.svg" width="35" title="View in Portuguese">](./README.pt.md)
[<img src="./assets/es.svg" width="35" title="View in Spanish">](./README.es.md)

# osu! Sweep

> A safe, smart, and user-friendly desktop utility to manage and clean large osu! beatmap collections based on game modes.

osu! Sweep is a desktop utility for Windows, built in C# and WPF, designed for osu! rhythm game players. 
The goal is to provide a tool to free up disk space and organize the player's library by identifying and deleting beatmaps from game modes they don't play.

## ✨ Key Features

-   **Hybrid Map Identification**: Uses the osu! API as the primary method and a deep file scan as a fallback.
-   **Smart Deletion Logic**: Deletes entire folders or just specific `.osu` files, preserving the modes you want to keep.
-   **User Safety First**: Features a clear confirmation screen and defaults to moving files to the Recycle Bin.

## 🛠️ Tech Stack

-   **Frontend (Desktop App)**: C# with WPF (.NET 8) using the MVVM pattern.
-   **Backend (Serverless API)**: C# on Azure Functions to protect the API key and cache requests.

## 🚀 How to Run

*(This section will be filled later, with instructions on how to build and run the project from the source code).*

## 🤝 Contributing

*(We can add guidelines here later if you wish to accept contributions from the community).*