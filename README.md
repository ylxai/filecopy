# FileCopy Utility v2.0 - WPF Edition

## Deskripsi Proyek

FileCopy Utility adalah aplikasi desktop berbasis WPF yang dirancang untuk menyalin file foto secara efisien dengan kecepatan tinggi dan fitur-fitur canggih. Aplikasi ini dirancang khusus untuk memproses file RAW dan JPG dengan pemisahan otomatis ke folder yang berbeda, menyediakan pengalaman pengguna yang modern dan antarmuka yang intuitif.

## Fitur Utama

- **Pemrosesan File Cepat**: Engine copy berkecepatan tinggi yang dapat mencapai kecepatan hingga 500 MB/s
- **Pemisahan File Otomatis**: File RAW dan JPG secara otomatis disimpan di folder terpisah
- **Validasi File**: Validasi file sebelum proses copy untuk memastikan keberhasilan
- **Monitoring Real-time**: Dashboard real-time untuk memantau proses copy
- **Multi-threading**: Dukungan multi-threading untuk pemrosesan paralel
- **Drag & Drop**: Dukungan drag & drop untuk kemudahan penggunaan
- **Laporan Kinerja**: Laporan detail dengan berbagai format ekspor
- **Pause/Resume**: Dukungan untuk jeda dan lanjutkan proses copy
- **Smart Copy**: Fitur cerdas yang melewati file yang sudah ada dengan ukuran yang sama untuk mempercepat proses resume
- **Post-Copy Verification**: Verifikasi otomatis setelah proses copy untuk memastikan integritas semua file
- **Glassmorphism UI**: Antarmuka modern dengan efek transparan dan blur yang elegan
- **Keamanan File**: File asli tidak dihapus atau dipindahkan, hanya disalin

## Teknologi dan Framework

- **.NET 8**: Platform runtime terbaru
- **WPF (Windows Presentation Foundation)**: Framework UI untuk aplikasi desktop Windows
- **C# 12**: Bahasa pemrograman utama
- **Material Design**: Desain UI yang modern dan konsisten
- **MVVM Pattern**: Arsitektur Model-View-ViewModel untuk pemisahan tanggung jawab

## Struktur Proyek

```
FileCopyUtility.WPF/
├── App.xaml                 # Konfigurasi aplikasi dan resource dictionary
├── App.xaml.cs              # Entry point aplikasi dan handler exception global
├── MainWindow.xaml          # UI utama aplikasi
├── MainWindow.xaml.cs       # Logika interaksi untuk MainWindow
├── MainWindow.Events.cs     # Handler event untuk UI
├── MainWindow.FileOperations.cs # Operasi file dan copy
├── MainWindow.Helpers.cs    # Fungsi-fungsi bantuan
├── MainWindow.Initialization.cs # Inisialisasi komponen dan layanan
├── MainWindow.Loading.cs    # Indikator loading dan UI enhancements
├── MainWindow.Performance.cs # Fungsi pelacakan kinerja
├── MainWindow.UIHandlers.cs # Fungsi-fungsi UI
├── FileCopyUtility.WPF.csproj # Konfigurasi proyek dan dependensi
├── .gitignore               # File-file yang diabaikan oleh Git
├── Configuration/           # File konfigurasi aplikasi
├── Controls/                # Custom controls
├── Helpers/                 # Fungsi-fungsi bantuan
│   └── AnimationHelper.cs   # Fungsi animasi dan efek UI
├── Models/                  # Model data
├── Services/                # Layanan bisnis
│   ├── AdvancedProgressService.cs    # Layanan progress lanjutan
│   ├── AdvancedReportingService.cs   # Layanan pelaporan
│   ├── FileOperationService.cs       # Layanan operasi file
│   ├── HighPerformanceFileService.cs # Layanan copy berkecepatan tinggi
│   ├── RealTimeDashboardService.cs   # Layanan dashboard real-time
│   └── ...                 # Layanan lainnya
├── Styles/                  # File-file style dan tema
│   ├── CssToXamlHelper.cs   # Konversi CSS ke XAML
│   └── ...                 # File-file tema
├── Windows/                 # Window tambahan
├── TestPhotos/              # Folder contoh foto untuk testing
├── package.json             # Dependensi Node.js (jika ada)
├── package-lock.json        # Lock file untuk dependensi Node.js
├── Glass_UI_Approach.md     # Dokumentasi pendekatan UI Glass
├── UI_Refactor_Plan.md      # Rencana refactor UI
└── refactor_plan.md         # Rencana refactor keseluruhan
```

## Dependensi Proyek

- **MaterialDesignThemes** (v4.9.0): Komponen UI dengan gaya Material Design
- **MaterialDesignColors** (v2.1.4): Palet warna Material Design
- **CommunityToolkit.Mvvm** (v8.2.2): Toolkit untuk pola MVVM
- **Hardcodet.NotifyIcon.Wpf** (v1.1.0): Integrasi system tray

## Instalasi dan Setup

### Prasyarat

- .NET 8 SDK
- Visual Studio 2022 atau VS Code dengan extension C#
- Windows 10/11 (karena menggunakan WPF)

### Langkah-langkah Instalasi

1. Clone repository:

   ```bash
   git clone <repository-url>
   cd FileCopyUtility.WPF
   ```

2. Restore dependensi:

   ```bash
   dotnet restore
   ```

3. Build proyek:

   ```bash
   dotnet build
   ```

4. Jalankan aplikasi:
   ```bash
   dotnet run
   ```

## Penggunaan

### Alur Kerja Dasar

1. **Pilih Folder Sumber**: Klik tombol "📁 Browse Source" untuk memilih folder yang berisi file foto
2. **Tambahkan Daftar File**: Masukkan nama file (tanpa ekstensi) ke dalam text box, satu per baris
3. **Impor File**: Gunakan tombol "📄 Import list.txt" atau "📋 Paste from Clipboard"
4. **Validasi File**: Klik "✅ Validate Files" untuk mencari file yang sesuai di folder sumber
5. **Mulai Copy**: Klik "🚀 START COPY" untuk memulai proses copy
6. **Pantau Proses**: Gunakan dashboard untuk memantau progress dan kinerja
7. **Hasil**: File RAW akan disimpan di folder "RAW", file JPG di folder "JPG"

### Fitur Lanjutan

- **Filter File**: Gunakan kotak filter untuk mencari file tertentu
- **Pause/Resume**: Gunakan tombol pause untuk menghentikan sementara proses
- **Cancel**: Batalkan proses kapan saja dengan tombol cancel
- **Laporan**: Hasilkan laporan kinerja setelah selesai

## Arsitektur dan Pola Desain

### MVVM Pattern

Aplikasi ini menggunakan pola Model-View-ViewModel untuk memisahkan logika bisnis dari tampilan UI:

- **Model**: FileItem, CopyResult, dll.
- **View**: MainWindow.xaml dan file-file XAML lainnya
- **ViewModel**: Terdistribusi di berbagai partial class

### Partial Class Pattern

Logika MainWindow dipisah ke beberapa file partial class:

- `MainWindow.Initialization.cs`: Inisialisasi komponen dan layanan
- `MainWindow.Events.cs`: Handler event UI
- `MainWindow.FileOperations.cs`: Logika operasi file
- `MainWindow.UIHandlers.cs`: Fungsi-fungsi UI
- `MainWindow.Helpers.cs`: Fungsi-fungsi bantuan
- `MainWindow.Performance.cs`: Fungsi pelacakan kinerja
- `MainWindow.Loading.cs`: Indikator loading

## Layanan Inti

### HighPerformanceFileService

Layanan utama untuk copy file berkecepatan tinggi dengan fitur:

- Multi-threading untuk pemrosesan paralel
- Optimasi I/O untuk kecepatan maksimum
- Pelaporan kinerja real-time

### FileOperationService

Layanan untuk operasi file dasar dengan:

- Validasi file
- Copy file standar
- Pelacakan progress

### RealTimeDashboardService

Layanan untuk dashboard real-time dengan:

- Pembaruan statistik secara real-time
- Visualisasi kinerja
- Monitoring proses

## Testing

### Unit Testing

Unit test belum diimplementasikan dalam proyek ini, tetapi struktur layanan yang modular memungkinkan implementasi unit testing di masa depan.

### Integration Testing

Proyek ini lebih fokus pada integration testing karena sifatnya sebagai aplikasi desktop dengan operasi file.

## Kontribusi

### Panduan Kontribusi

1. Fork repository
2. Buat branch fitur (`git checkout -b feature/NamaFitur`)
3. Commit perubahan (`git commit -m 'Tambahkan fitur NamaFitur'`)
4. Push ke branch (`git push origin feature/NamaFitur`)
5. Buat pull request

### Panduan Gaya Kode

- Gunakan gaya Microsoft C# dengan penekanan pada readability
- Gunakan nullable reference types
- Gunakan implicit usings
- Gunakan partial class untuk memisahkan tanggung jawab
- Gunakan async/await untuk operasi I/O

## Deployment

### Build Release

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### Installer

Aplikasi dapat dipaketkan sebagai installer Windows menggunakan tools seperti:

- WiX Toolset
- Advanced Installer
- Inno Setup

## Lisensi

Proyek ini adalah proyek internal/enterprise. Detail lisensi dapat ditentukan sesuai kebijakan organisasi.

## Dukungan dan Kontak

Untuk dukungan teknis atau pertanyaan lebih lanjut, silakan hubungi tim pengembangan internal.

## Riwayat Versi

- **v2.0**: Rilis utama dengan UI Glassmorphism, Smart Copy, Post-Copy Verification, dan Icon baru
- **v1.0**: Rilis awal dengan fitur dasar copy file

## TODO dan Rencana Pengembangan

- [ ] Implementasi fitur scheduler lanjutan
- [ ] Penambahan fitur backup sebelum copy
- [ ] Integrasi dengan cloud storage
- [ ] Implementasi unit test
- [ ] Dukungan multi-platform (jika bermigrasi ke .NET MAUI)
- [ ] Penambahan fitur batch processing
- [ ] Penyempurnaan UI/UX berdasarkan feedback pengguna
