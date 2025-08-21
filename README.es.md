[<img src="./assets/us.svg" width="35" title="Ver en Inglés">](./README.md)
[<img src="./assets/br.svg" width="35" title="Ver en Portugués">](./README.pt.md)

# osu! Sweep

> Una utilidad de escritorio segura, inteligente y fácil de usar para gestionar y limpiar grandes colecciones de beatmaps de osu! basada en los modos de juego.

osu! Sweep es una utilidad de escritorio para Windows, construida en C# y WPF, diseñada para jugadores del juego de ritmo osu!.
 El objetivo es proporcionar una herramienta para liberar espacio en disco y organizar la biblioteca del jugador, identificando y eliminando beatmaps de modos de juego que él no juega.

## ✨ Características Principales

-   **Identificación de Mapas Híbrida**: Utiliza la API de osu! como método principal y un escaneo profundo de archivos como fallback.
-   **Lógica de Eliminación Inteligente**: Elimina carpetas enteras o solo archivos `.osu` específicos, preservando los modos que deseas conservar.
-   **Seguridad del Usuario Primero**: Cuenta con una pantalla de confirmación clara y, por defecto, mueve los archivos a la Papelera de Reciclaje.

## 🛠️ Tecnologías Utilizadas

-   **Frontend (Aplicación de Escritorio)**: C# con WPF (.NET 8) utilizando el patrón MVVM.
-   **Backend (API Serverless)**: C# en Azure Functions para proteger la clave de la API y cachear las solicitudes.

## 🚀 Cómo Ejecutar

*(Esta sección se llenará más adelante, con instrucciones sobre cómo compilar y ejecutar el proyecto desde el código fuente).*

## 🤝 Contribuciones

*(Podemos añadir directrices aquí más adelante si deseas aceptar contribuciones de la comunidad).*