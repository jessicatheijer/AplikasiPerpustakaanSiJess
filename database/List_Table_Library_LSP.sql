CREATE DATABASE IF NOT EXISTS library_lsp;
USE library_lsp;

-- 1) TABEL ANGGOTA
CREATE TABLE anggota (
  id_anggota INT AUTO_INCREMENT PRIMARY KEY,
  nama_lengkap VARCHAR(150) NOT NULL,
  nik_or_nis VARCHAR(30),
  jenis_kelamin ENUM('Laki-laki','Perempuan') NOT NULL,
  alamat VARCHAR(255) NOT NULL,
  no_telp VARCHAR(30) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uk_anggota_no (nik_or_nis)
) ENGINE=InnoDB;

-- 2) TABEL AKUN
CREATE TABLE akun (
  id_akun INT AUTO_INCREMENT PRIMARY KEY,
  id_anggota INT NULL,
  username VARCHAR(50) NOT NULL,
  password_hash VARCHAR(128) NOT NULL,
  role ENUM('ADMIN','ANGGOTA') NOT NULL DEFAULT 'ANGGOTA',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uk_akun_username (username),
  KEY idx_akun_anggota (id_anggota),
  CONSTRAINT fk_akun_anggota
    FOREIGN KEY (id_anggota) REFERENCES anggota(id_anggota)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- 3) TABEL KATEGORI
CREATE TABLE kategori (
  id_kategori INT AUTO_INCREMENT PRIMARY KEY,
  nama_kategori VARCHAR(100) NOT NULL,
  UNIQUE KEY uk_kategori_nama (nama_kategori)
) ENGINE=InnoDB;

-- 4) TABEL KOLEKSI (BASE)
CREATE TABLE koleksi (
  id_koleksi INT AUTO_INCREMENT PRIMARY KEY,
  judul VARCHAR(200) NOT NULL,
  id_kategori INT NOT NULL,
  tipe_koleksi ENUM('BUKU','CD','JURNAL') NOT NULL,
  harga INT NOT NULL DEFAULT 0,
  status ENUM('TERSEDIA','DIPINJAM','HILANG','NONAKTIF') NOT NULL DEFAULT 'TERSEDIA',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  KEY idx_koleksi_judul (judul),
  KEY idx_koleksi_kategori (id_kategori),
  KEY idx_koleksi_tipe (tipe_koleksi),
  KEY idx_koleksi_status (status),

  CONSTRAINT fk_koleksi_kategori
    FOREIGN KEY (id_kategori) REFERENCES kategori(id_kategori)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- 5) TABEL BUKU (CHILD)
CREATE TABLE buku (
  id_koleksi INT PRIMARY KEY,
  isbn VARCHAR(40) NOT NULL,
  penulis VARCHAR(150) NOT NULL,
  penerbit VARCHAR(200) NOT NULL,
  tahun_terbit YEAR NOT NULL,
  stok INT NOT NULL DEFAULT 1,
  UNIQUE KEY uk_buku_isbn (isbn),
  CONSTRAINT fk_buku_koleksi
    FOREIGN KEY (id_koleksi) REFERENCES koleksi(id_koleksi)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT ck_buku_stok CHECK (stok >= 0)
) ENGINE=InnoDB;

-- 6) TABEL CD (CHILD)
CREATE TABLE cd (
  id_koleksi INT PRIMARY KEY,
  judul_album VARCHAR(200) NOT NULL,
  artis VARCHAR(150) NOT NULL,
  durasi_menit INT NOT NULL,
  format ENUM('AUDIO','VIDEO','DATA') NOT NULL,
  CONSTRAINT fk_cd_koleksi
    FOREIGN KEY (id_koleksi) REFERENCES koleksi(id_koleksi)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT ck_cd_durasi CHECK (durasi_menit > 0)
) ENGINE=InnoDB;

-- 7) TABEL JURNAL (CHILD)
CREATE TABLE jurnal (
  id_koleksi INT PRIMARY KEY,
  issn VARCHAR(40) NOT NULL,
  volume VARCHAR(20) NOT NULL,
  nomor VARCHAR(20) NOT NULL,
  tahun_terbit YEAR NOT NULL,
  penerbit VARCHAR(200) NOT NULL,
  UNIQUE KEY uk_jurnal_issn (issn),
  CONSTRAINT fk_jurnal_koleksi
    FOREIGN KEY (id_koleksi) REFERENCES koleksi(id_koleksi)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- 8) TABEL PEMINJAMAN (HEADER)
CREATE TABLE peminjaman (
  id_pinjam INT AUTO_INCREMENT PRIMARY KEY,
  id_anggota INT NOT NULL,
  tgl_pinjam DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  tgl_jatuh_tempo DATETIME NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  KEY idx_pinjam_anggota (id_anggota),
  KEY idx_pinjam_tgl_pinjam (tgl_pinjam),
  KEY idx_pinjam_jatuh_tempo (tgl_jatuh_tempo),
  CONSTRAINT fk_pinjam_anggota
    FOREIGN KEY (id_anggota) REFERENCES anggota(id_anggota)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- 10) TABEL PEMINJAMAN_DETAIL (DETAIL ITEM)
CREATE TABLE peminjaman_detail (
  id_detail INT AUTO_INCREMENT PRIMARY KEY,
  id_pinjam INT NOT NULL,
  id_koleksi INT NOT NULL,
  status_item ENUM('DIPINJAM','KEMBALI','HILANG') NOT NULL DEFAULT 'DIPINJAM',
  tgl_kembali DATETIME NULL,
  denda INT NOT NULL DEFAULT 0,

  KEY idx_detail_pinjam (id_pinjam),
  KEY idx_detail_koleksi (id_koleksi),
  KEY idx_detail_status (status_item),

  CONSTRAINT fk_detail_pinjam
    FOREIGN KEY (id_pinjam) REFERENCES peminjaman(id_pinjam)
    ON DELETE CASCADE ON UPDATE CASCADE,

  CONSTRAINT fk_detail_koleksi
    FOREIGN KEY (id_koleksi) REFERENCES koleksi(id_koleksi)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CONSTRAINT uk_pinjam_koleksi UNIQUE (id_pinjam, id_koleksi),
  CONSTRAINT ck_detail_denda CHECK (denda >= 0)
) ENGINE=InnoDB;
