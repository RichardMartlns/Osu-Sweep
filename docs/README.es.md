# osu! Sweep 🧹🎶

<p align="center">
  <a href="../README.md">
    <img src="../assets/us.svg" alt="English" width="30"/>
    &nbsp;English
  </a>
  &nbsp;&nbsp;
  <a href="./README.pt.md"> 
     <img src="../assets/flag_br.png" alt="Português" width="30"/>     
     &nbsp;Português
  </a>
</p>

Una **herramienta de escritorio segura, inteligente y amigable** para gestionar y limpiar grandes colecciones de beatmaps de **osu!**, basada en los modos de juego que realmente utilizas.

<p align="center">
  <img src="https://i.imgur.com/link_para_seu_gif_aqui.gif" alt="osu! Sweep Demo" width="700"/>
</p>

---

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Languages](https://img.shields.io/badge/idiomas-3-blue.svg)
![Tech](https://img.shields.io/badge/.NET-8.0-purple.svg)

## 🚦 Estado del Proyecto

**En Desarrollo Activo (Versión Alfa)**

Este proyecto está en curso. La arquitectura y la lógica de negocio principal (análisis de mapas, comunicación con la API, cálculos de eliminación y pruebas unitarias) están bien estructuradas y funcionales. La interfaz de usuario (UI) está siendo migrada de WPF a Avalonia UI para soporte multiplataforma.

## 📌 Sobre el Proyecto

**osu! Sweep** nació de la necesidad de gestionar bibliotecas masivas de osu!, que pueden ocupar **cientos de GB** en el disco. El objetivo es ofrecer una herramienta que ayude a los jugadores a **recuperar espacio y organizar sus mapas** de forma **segura y personalizable**.

---

## ✨ Funcionalidades

- **🔍 Identificación Híbrida de Beatmaps**
  - Utiliza la **API v2 de osu!** como método principal, con autenticación OAuth 2.0.
  - Escaneo profundo de archivos `.osu` como respaldo para mapas locales.

- **🧠 Lógica de Eliminación Inteligente**
  - Elimina carpetas completas o solo archivos `.osu` de modos no deseados.
  - Conserva los modos de juego que elijas mantener.

- **🛡️ Prioridad en la Seguridad del Usuario**
  - Muestra una pantalla de confirmación clara antes de cualquier eliminación.
  - Por defecto, mueve los archivos a la **Papelera de Reciclaje**, con opción de eliminación permanente.

- **🌐 Internacionalización (i18n)**
  - Soporte para múltiples idiomas (Portugués, Inglés, Español) con cambio en tiempo real.

---

## 🛠️ Tecnologías Utilizadas

- **Frontend (Aplicación de Escritorio):**
  - C# con **Avalonia UI** (.NET 8) para soporte nativo en Windows y Linux.
  - Patrón **MVVM (Model-View-ViewModel)**.

- **Backend (API sin Servidor):**
  - Azure Functions en C# (.NET 8 Isolated Worker).
  - Proxy seguro para la API v2 de osu! con gestión de tokens OAuth 2.0.

- **Pruebas:**
  - MSTest (para pruebas unitarias y de integración).
  - Moq (para simulación de dependencias).

---

## 📂 Estructura del Proyecto

```bash
OsuSweep.sln
├── OsuSweep/                 # Aplicación de escritorio (Frontend - UI)
├── OsuSweep.Backend/         # Azure Functions (Backend - API Proxy)
├── OsuSweep.Core/            # Lógica de negocio, contratos y modelos compartidos
├── OsuSweep.Tests.Unit/      # Pruebas unitarias rápidas
└── OsuSweep.Tests.Integration/ # Pruebas de integración (I/O)
```

---

## 🚀 Cómo Ejecutar

Instrucciones para compilar y ejecutar el proyecto desde el código fuente.

### 📋 Requisitos Previos

- .NET 8 SDK
- Visual Studio 2022 (con los workloads ".NET desktop development" y "Azure development")
- Una cuenta de osu! para generar credenciales de API (OAuth)

### ▶️ Pasos

1. Clona el repositorio:

   ```bash
   git clone https://github.com/tu-usuario/OsuSweep.git
   cd OsuSweep
   ```

2. Configura las credenciales de la API:

   - En el proyecto `OsuSweep.Backend`, crea un archivo llamado `local.settings.json`.
   - Completa con tus credenciales `Client ID` y `Client Secret`:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "OSU_API_CLIENT_ID": "TU_CLIENT_ID_AQUÍ",
       "OSU_API_CLIENT_SECRET": "TU_CLIENT_SECRET_AQUÍ",
       "Host": {
         "CORS": "*"
       }
     }
   }
   ```

3. Ejecuta la aplicación:

   - Abre la solución `OsuSweep.sln` en Visual Studio.
   - Haz clic derecho en la Solución > *Configure Startup Projects...* y configura para iniciar ambos proyectos `OsuSweep` y `OsuSweep.Backend`.
   - Presiona F5 para ejecutar.

---

## 🗺️ Hoja de Ruta

- [x] Estructura inicial del proyecto y lógica de negocio.
- [x] Implementación de la API v2 de osu! y análisis manual.
- [x] Arquitectura de pruebas (unitarias y de integración).
- [x] Soporte para múltiples idiomas (i18n).
- [ ] Migración de la UI a Avalonia: Reescribir la capa de presentación (Views) de WPF a Avalonia UI para crear una base multiplataforma.
- [ ] Implementar soporte para Linux: Desarrollar lógica específica para el sistema de archivos de Linux (ej.: localizar la carpeta `Songs`, mover a la Papelera).
- [ ] Mejoras de UI/UX con un tema inspirado en osu! (post-migración).
- [ ] Configuraciones avanzadas (ej.: eliminar videos/storyboards).
- [ ] Publicación de una versión instalable en GitHub.

---

## 📜 Licencia

Distribuido bajo la Licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más información.