# Smart Vidkon Automation Pro — Desktop Video Automation & Conference Studio
### Official Open Portfolio & Desktop Production Suite — FokusKonten Digital

**Smart Vidkon Automation Pro** adalah software aplikasi desktop Windows profesional untuk otomasi produksi video vertikal (Shorts/Reels/TikTok), pembuatan storyboard otomatis, transkripsi audio presisi tinggi, dan perakitan video berbasis akselerasi perangkat keras NVIDIA NVENC.

---

## 1. ARSITEKTUR PERANGKAT LUNAK

Aplikasi ini dibangun menggunakan arsitektur *Hybrid Enterprise Desktop*:
* **Frontend GUI Native**: C# .NET Windows Forms dengan arsitektur multi-view terpisah (`MainForm.StudioView.cs`, `MainForm.Pipeline.cs`, `MainForm.MediaPreview.cs`, `MainForm.AiVoice.cs`).
* **Processing Engine (Microservices)**:
  - `assemble_video_nvenc_vertical.js`: Engine perakitan video 9:16 ultra-cepat memanfaatkan GPU NVIDIA NVENC dan FFmpeg.
  - `extract_audio_timing_whisper.py`: Engine transkripsi dan ekstraksi jeda kata (*word-level timestamp*) menggunakan Whisper CUDA.
  - `generate_kinetic_subtitles.js`: Generator tipografi bergerak dan teks animasi kinetik bersinkronisasi audio.
  - `generate_storyboard_contact_sheet.js`: Penyusun lembar pratinjau kontak visual *storyboard* otomatis.
  - `fetch_stock_videos_pixabay.js` & `scan_and_check_footage.js`: Pustaka pencarian dan validasi footage lokal & cloud.
* **Installer & Deployment**: Inno Setup (`SmartVidkonAutomation.iss`), PowerShell build automation (`build_installer.ps1`), dan dukungan registri Windows Package Manager (Winget).

---

## 2. FITUR UTAMA

* **Hardware-Accelerated NVENC Pipeline**: Rendering video Full HD 1080x1920 60 FPS langsung menggunakan hardware encoder GPU tanpa membebani CPU.
* **AI Voiceover & Multi-Engine TTS**: Mendukung integrasi generasi suara alami berbasis Gemini Studio TTS dan ElevenLabs dengan pembersih pengucapan teknis bahasa Indonesia.
* **Whisper Word-Level Accurate Alignment**: Ekstraksi waktu bicara per-kata secara presisi untuk menempatkan highlight teks kinetik di video.
* **BGM & Footage Ledger Registry**: Pengelolaan basis data SQLite untuk katalog footage B-roll dan soundtrack bebas royalti secara otomatis.
* **Offline Standalone Capability**: Seluruh proses transkripsi lokal, rendering FFmpeg, dan pengolahan video dapat berjalan 100% offline tanpa ketergantungan server luar.

---

## 3. PERSYARATAN SISTEM

* **Sistem Operasi**: Windows 10 / Windows 11 (64-bit)
* **Runtime**: .NET Framework 4.8 / .NET Core Desktop Runtime
* **Akselerator Rekomendasi**: GPU NVIDIA (GTX 1650 / 1660 SUPER atau lebih tinggi dengan dukungan NVENC)
* **Kebutuhan Storage**: ~500 MB untuk aplikasi dasar dan core engine.

---

## 4. STRUKTUR DIREKTORI

```
Smart_Vidkon_Automation/
├── desktop/           # Kode sumber C# WinForms (UI & Controller)
├── engine/            # Skrip pemrosesan Node.js & Python (Video/Audio/AI)
├── config/            # Template konfigurasi aplikasi
├── assets/            # Aset antarmuka dan branding
├── docs/              # Panduan pengguna dan dokumentasi teknis
└── projects/          # Direktori kerja output render video
```

---
&copy; 2026 FokusKonten Digital — All Rights Reserved.
