 USE library_lsp;
INSERT INTO akun (id_anggota, username, password_hash, role)
VALUES (NULL, 'adminjess', SHA2('aiueo12234', 256), 'ADMIN');
 
INSERT INTO kategori (nama_kategori) VALUES
('Ilmu Komputer'),
('Bisnis dan Manajemen'),
('Sains dan Teknologi'),
('Pendidikan'),
('Hukum dan Sosial'),
('Kesehatan'),
('Multimedia dan Seni'),
('Komik'),
('Novel'),
('Agama');

INSERT INTO anggota 
(nama_lengkap, nik_or_nis, jenis_kelamin, alamat, no_telp) 
VALUES
('Jessica Theijer', 'NIS2022001', 'Perempuan', 'Jl Waterplace Residence Tower B Surabaya', '081234567001'),
('Harto Budianto', 'NIS2022002', 'Laki-laki', 'Jl Randu Asri I, Sono, Sidoarjo', '081234567002'),
('Frederick Prasetyo', '3572849103689027', 'Laki-laki', 'Jl Ahmad Yani 318 Surabaya', '081234567003'),
('Lumba Lumba', '3574383042329802', 'Perempuan', 'Perumahan the taman dhika, blok bromo c1, Sidoarjo', '081234567004'),
('Kimberly Hartono', 'NIS2022005', 'Perempuan', 'Jl Mangga III Blok J No 1 Sidoarjo', '081234567005');

-- INSERT LIST BUKU
-- Buku 1
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Clean Code', 2, 'BUKU', 150000, 'TERSEDIA');

INSERT INTO buku (id_koleksi, isbn, penulis, penerbit, tahun_terbit, stok)
VALUES (LAST_INSERT_ID(), '9780132350884', 'Robert C. Martin', 'Prentice Hall', 2008, 3);

-- Buku 2
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Design Patterns', 2, 'BUKU', 180000, 'TERSEDIA');

INSERT INTO buku (id_koleksi, isbn, penulis, penerbit, tahun_terbit, stok)
VALUES (LAST_INSERT_ID(), '9780201633610', 'Gamma et al.', 'Addison-Wesley', 1994, 2);

-- Buku 3
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Database System Concepts', 3, 'BUKU', 200000, 'TERSEDIA');

INSERT INTO buku (id_koleksi, isbn, penulis, penerbit, tahun_terbit, stok)
VALUES (LAST_INSERT_ID(), '9780073523323', 'Silberschatz', 'McGraw-Hill', 2010, 2);


-- INSERT LIST CD
-- CD 1
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Greatest Hits Collection', 10, 'CD', 80000, 'TERSEDIA');

INSERT INTO cd (id_koleksi, judul_album, artis, durasi_menit, format)
VALUES (LAST_INSERT_ID(), 'Greatest Hits', 'Queen', 74, 'AUDIO');

-- CD 2
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Movie Soundtrack Vol.1', 10, 'CD', 60000, 'TERSEDIA');

INSERT INTO cd (id_koleksi, judul_album, artis, durasi_menit, format)
VALUES (LAST_INSERT_ID(), 'Soundtrack Vol.1', 'Various Artists', 58, 'AUDIO');


-- INSERT LIST JURNAL
-- Jurnal 1
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('Journal of Computer Science', 2, 'JURNAL', 250000, 'TERSEDIA');

INSERT INTO jurnal (id_koleksi, issn, volume, nomor, tahun_terbit, penerbit)
VALUES (LAST_INSERT_ID(), '1234-5678', '15', '2', 2023, 'Elsevier');

-- Jurnal 2
INSERT INTO koleksi (judul, id_kategori, tipe_koleksi, harga, status)
VALUES ('International Journal of Information Systems', 3, 'JURNAL', 230000, 'TERSEDIA');

INSERT INTO jurnal (id_koleksi, issn, volume, nomor, tahun_terbit, penerbit)
VALUES (LAST_INSERT_ID(), '9876-5432', '8', '1', 2022, 'Springer');



INSERT INTO peminjaman (id_anggota, tgl_pinjam, tgl_jatuh_tempo)
VALUES (1, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY));
SET @last_pinjam := LAST_INSERT_ID();


INSERT INTO peminjaman_detail (id_pinjam, id_koleksi)
VALUES
(@last_pinjam, 1),
(@last_pinjam, 4);

UPDATE koleksi
SET status = 'DIPINJAM'
WHERE id_koleksi IN (1,4);

UPDATE peminjaman_detail d
JOIN koleksi k ON k.id_koleksi = d.id_koleksi
SET
  d.status_item = 'HILANG',
  d.tgl_kembali = NOW(),
  d.denda = k.harga + 50000
WHERE d.id_koleksi = 1
  AND d.id_pinjam = @last_pinjam;

UPDATE koleksi
SET status = 'HILANG'
WHERE id_koleksi = 1;

-- Lihat semua koleksi
SELECT k.id_koleksi, k.judul, k.tipe_koleksi, kt.nama_kategori, k.harga, k.status
FROM koleksi k
JOIN kategori kt ON kt.id_kategori = k.id_kategori;

-- Lihat transaksi peminjaman
SELECT p.id_pinjam, a.nama_lengkap, p.tgl_pinjam, p.tgl_jatuh_tempo,
       d.id_koleksi, d.status_item, d.denda
FROM peminjaman p
JOIN anggota a ON a.id_anggota = p.id_anggota
JOIN peminjaman_detail d ON d.id_pinjam = p.id_pinjam;
