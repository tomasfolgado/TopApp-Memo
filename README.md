# 📌 TopApp Memo — Quick Sticky Notes for Windows

<div align="center">

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-blue.svg?style=for-the-badge&logo=windows)
![.NET](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4.svg?style=for-the-badge&logo=dotnet)
![Language](https://img.shields.io/badge/Language-C%23-239120.svg?style=for-the-badge&logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Free%20%26%20Open%20Source-orange.svg?style=for-the-badge)

**A modern, lightweight, and versatile sticky notes application for Windows.**  
*Una aplicación moderna, ligera y versátil de notas adhesivas para Windows.*

[English](#-english) • [Español](#-español) • [Installation & Build](#-build-and-run) • [License](#-license)

</div>

---

## 🇬🇧 English

### ✨ Overview
**TopApp Memo** is a feature-rich desktop sticky notes manager crafted with WPF (.NET 8). Designed for productivity, it combines the simplicity of physical post-it notes with digital superpowers: audio memos, speech-to-text dictation, alarms, freehand drawing, and a unified control panel.

### 🚀 Key Features

- **🌐 Instant Bilingual Support**: Switch seamlessly between **English** and **Spanish** (`ES` / `EN` toggle in the header).
- **📌 Always on Top & Docking**: Pin notes above all other windows or keep them subtly in the background.
- **🎨 8 Post-it Colors & Opacity**: Yellow, Blue, Green, Red, Purple, Orange, Cyan, and White with custom opacity and tint adjustments.
- **✏️ Digital Ink & Drawing Canvas**: Sketch, scribble, or highlight handwritten diagrams directly on your notes.
- **🎙️ Voice Memos & Autonomous Audio**: Record and play audio memos attached to any note with progress indicators.
- **🗣️ Speech-to-Text Dictation**: Speak and let the app transcribe your thoughts into rich text automatically.
- **⏰ Scheduled Alarms & Reminders**: Set alarms with customizable duration and notification sounds.
- **🗂️ Central Control Panel**:
  - Live metric counter (Total, Visible, Hidden, With Audio, With Alarm).
  - Instant real-time search across all notes.
  - Quick filter chips (All, Visible, Hidden, Audio, Alarm).
  - Bulk actions: Show all, Hide all, Save all.
- **⌨️ Keyboard Shortcuts**: Fast shortcuts for formatting (`Ctrl+B`, `Ctrl+I`, `Ctrl+U`), saving (`Ctrl+S`), and quick exit (`Esc`).

---

## 🇪🇸 Español

### ✨ Descripción
**TopApp Memo** es un gestor de notas adhesivas moderno y de alto rendimiento desarrollado en WPF (.NET 8). Diseñado para maximizar tu productividad, combina la inmediatez de los post-its tradicionales con funciones avanzadas: notas de voz, dictado inteligente, recordatorios con alarma, dibujo a mano alzada y un panel de control unificado.

### 🚀 Características Principales

- **🌐 Soporte Bilingüe Instantáneo**: Alterna en tiempo real entre **Español** e **Inglés** con el selector (`ES` / `EN`) en la cabecera.
- **📌 Siempre al Frente (Always on Top)**: Fija notas por encima de navegadores y aplicaciones con un solo clic.
- **🎨 8 Colores de Post-it y Opacidad**: Amarillo, Azul, Verde, Rojo, Púrpura, Naranja, Cian y Blanco, con control de transparencia e intensidad.
- **✏️ Tinta Digital y Modo Dibujo**: Dibuja esquemas, firmas o bocetos a mano alzada sobre la nota.
- **🎙️ Notas de Voz Autónomas**: Graba y reproduce mensajes de audio vinculados a cada nota.
- **🗣️ Dictado por Voz Inteligente**: Dicta por micrófono para transcribir texto automáticamente.
- **⏰ Alarmas y Recordatorios**: Configura avisos con presets rápidos (+15m, +1h, mañana) y alertas sonoras.
- **🗂️ Panel de Control Unificado**:
  - Contadores en tiempo real (Total, Visibles, Ocultas, Audio, Alarma).
  - Buscador instantáneo de texto en todas las notas.
  - Filtros por categoría y estado.
  - Acciones globales: Guardar todo, Mostrar todas, Ocultar todas.
- **⌨️ Atajos de Teclado**: Negrita (`Ctrl+B`), cursiva (`Ctrl+I`), subrayado (`Ctrl+U`), guardar (`Ctrl+S`).

---

## 🛠️ Build and Run

### Prerequisites
- Windows 10 (version 1809+) or Windows 11.
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### Build from Source
```bash
# Clone the repository
git clone https://github.com/your-username/topapp-memo.git
cd topapp-memo/PostitApp

# Restore and build in Release mode
dotnet build -c Release

# Publish self-contained or framework-dependent
dotnet publish -c Release -o ../publish
```

### Run
Launch `TopAppMemo.exe` from `PostitApp/bin/Release/net8.0-windows/` or `publish/`.

---

## 📁 Project Structure

```
topapp-memo/
├── PostitApp/
│   ├── Assets/              # Icons, post-it textures, application resources
│   ├── Data/                # Local data storage (notes.json, settings.json)
│   ├── Helpers/             # Win32 window management, MVVM RelayCommands
│   ├── Models/              # Note data model
│   ├── Resources/           # Localization strings (Strings.es.xaml, Strings.en.xaml)
│   ├── Services/            # Note persistence, NAudio recorder, Speech, Localization
│   ├── ViewModels/          # MainViewModel, NoteViewModel
│   ├── Views/               # ManagerWindow, NoteWindow, AlarmWindow, HelpWindow
│   └── PostitApp.csproj     # .NET 8 WPF Project specification
├── .gitignore               # Clean GitHub ignore patterns
├── LICENSE                  # Open source MIT license
└── README.md                # Project documentation
```

---

## ⌨️ Keyboard Shortcuts / Atajos de Teclado

| Shortcut / Atajo | Function (English) | Función (Español) |
|---|---|---|
| `Ctrl + N` | New sticky note | Crear nueva nota adhesiva |
| `Ctrl + S` | Save all notes to disk | Guardar todas las notas en disco |
| `Ctrl + B` | Bold text | Formato de texto en Negrita |
| `Ctrl + I` | Italic text | Formato de texto en Cursiva |
| `Ctrl + U` | Underline text | Formato de texto Subrayado |
| `Esc` | Close dialog or help center | Cerrar diálogo o centro de ayuda |

---

## 📄 License

This project is licensed under the **MIT License** — free and open source for everyone. See the [LICENSE](LICENSE) file for details.

Developed with ❤️ by **Tomas Folgado**.
