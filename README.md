**1. Informasi Umum Aplikasi**
Nama Aplikasi : Aplikasi Perpustakaan SiJess
Jenis Aplikasi : Desktop Application
Platform : Windows
Bahasa Pemrograman : C# (.NET Framework 4.8)
DBMS : MySQL
IDE : Microsoft Visual Studio
Library Utama : MySql.Data

## Fitur
- Login Admin & Anggota
- Katalog Koleksi (Buku / CD / Jurnal)
- Peminjaman & Pengembalian
- Perhitungan denda otomatis
- Upload foto koleksi
- CRUD Anggota, Kategori, Koleksi

Aplikasi ini dibuat untuk mendukung kegiatan perpustakaan, khususnya pengelolaan katalog koleksi, pendaftaran anggota, dan pencatatan peminjaman koleksi sesuai dengan kebutuhan sistem yang ditentukan.


**2. Struktur Modul Program**
Aplikasi dikembangkan dengan konsep Object Oriented Programming (OOP) dan pemisahan tanggung jawab (separation of concerns).

**2.1 Modul Data (Data Layer)**
Folder: Data/Db.cs
Mengatur koneksi ke database MySQL menggunakan connection string.
Data/Sql.cs
Menyimpan helper query SQL (jika diperlukan).

Fungsi utama: Menghubungkan aplikasi dengan basis data MySQL secara aman dan terstruktur.

**2.2 Modul Model (Model Layer)**
Folder: Models/
Anggota.cs → Representasi data anggota
Kategori.cs → Representasi data kategori koleksi
KoleksiRow.cs → Data koleksi lengkap untuk admin
KoleksiCard.cs → Data koleksi ringkas untuk katalog anggota
PeminjamanInfo.cs → Data transaksi peminjaman
Session.cs → Menyimpan data user yang sedang login

Fungsi utama: Merepresentasikan struktur data dan properti yang digunakan oleh aplikasi.


**2.3 Modul Repository (Repository Layer)**
Folder: Repositories/
AuthRepository.cs → Login \& Sign Up
AnggotaRepository.cs → CRUD anggota
KategoriRepository.cs → CRUD kategori
KoleksiRepository.cs → CRUD buku, CD, jurnal, katalog, upload foto
PeminjamanRepository.cs → Transaksi peminjaman \& pengembalian

Fungsi utama: Mengelola seluruh proses akses basis data (CRUD) dengan query SQL dan parameterized query untuk mencegah SQL Injection.



**2.4 Modul Service (Business Logic)**
Folder: Services/
DendaService.cs
Menghitung denda keterlambatan (Rp10.000/hari), denda kehilangan (harga koleksi + Rp50.000)

DateService.cs
Mengatur logika tanggal peminjaman dan jatuh tempo (7 hari).

Fungsi utama: Memisahkan logika bisnis dari tampilan (UI).



**2.5 Modul UI (Presentation Layer)**
Folder: UI/
Theme.cs → Pengaturan warna, font, dan tampilan modern
Controls/KoleksiCardControl.cs → Komponen kartu katalog

Forms/
LoginForm.cs
SignupForm.cs
MemberCatalogForm.cs
MemberLoansForm.cs
AdminDashboardForm.cs
AdminMasterDataForm.cs
AdminTransaksiForm.cs
AdminLaporanForm.cs
IdCardForm.cs

Fungsi utama: Menampilkan antarmuka pengguna dan menangani interaksi user.



**3. Cara Menjalankan Aplikasi**

**3.1 Persiapan**

- Install MySQL Server
- Install Visual Studio
- Pastikan .NET Framework 4.8 sudah terpasang


**3.2 Setup Database**
Buka MySQL Workbench
Jalankan seluruh file script SQL:
pembuatan database dan table (List\_Table\_Library\_LSP.sql)
trigger (List\_Trigger\_Library\_LSP.sql)
dummy data (List\_input\_Library\_LSP.sql)

Pastikan database bernama: library\_lsp



**3.3 Konfigurasi Koneksi Database**
Buka file App.config, sesuaikan server, user, password: 
<connectionStrings>
&nbsp; <add name="db" connectionString="server=localhost;database=library\_lsp;uid=root;pwd=;"
&nbsp;      providerName="MySql.Data.MySqlClient"/>
</connectionStrings>



**3.4 Menjalankan Aplikasi**
- Buka project di Visual Studio
- Set Startup Project
- Tekan F5 / Run
- Login:
- Admin:  Username: adminjess, Password: aiueo1234
- Anggota: Gunakan fitur Sign Up



**4. Dependency dan Library**
**4.1 Library yang Digunakan**
MySql.Data
Fungsi: koneksi dan query ke MySQL
Sumber: NuGet Package Manager
Lisensi: Open source (legal)

**4.2 Komponen Reuse**
Windows Forms Controls (Button, DataGridView, Panel, dll)
MySql.Data library
Keuntungan reuse:
* Menghemat waktu pengembangan
* Stabil dan teruji
* Tidak melanggar lisensi



**5. Catatan Teknis**
Aplikasi menerapkan:
* OOP
* Layered Architecture
* Error handling (try–catch)
* Parameterized query
* File foto koleksi disimpan secara lokal dan path-nya disimpan di database.
* Data diuji menggunakan dummy data dan berbagai skenario transaksi.


**6. Penutup**
Aplikasi Perpustakaan SiJess telah memenuhi kebutuhan sistem perpustakaan dengan fitur katalog, peminjaman, pengelolaan data, serta koneksi basis data MySQL sesuai standar pemrograman desktop.