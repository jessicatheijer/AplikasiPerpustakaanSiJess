USE library_lsp;
DELIMITER $$
CREATE TRIGGER trg_buku_validate
BEFORE INSERT ON buku
FOR EACH ROW
BEGIN
  IF (SELECT tipe_koleksi FROM koleksi WHERE id_koleksi = NEW.id_koleksi) <> 'BUKU' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'tipe_koleksi harus BUKU untuk insert ke tabel buku';
  END IF;

  IF EXISTS (SELECT 1 FROM cd WHERE id_koleksi = NEW.id_koleksi)
     OR EXISTS (SELECT 1 FROM jurnal WHERE id_koleksi = NEW.id_koleksi) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'id_koleksi sudah terdaftar pada tabel turunan lain';
  END IF;
END$$

CREATE TRIGGER trg_cd_validate
BEFORE INSERT ON cd
FOR EACH ROW
BEGIN
  IF (SELECT tipe_koleksi FROM koleksi WHERE id_koleksi = NEW.id_koleksi) <> 'CD' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'tipe_koleksi harus CD untuk insert ke tabel cd';
  END IF;

  IF EXISTS (SELECT 1 FROM buku WHERE id_koleksi = NEW.id_koleksi)
     OR EXISTS (SELECT 1 FROM jurnal WHERE id_koleksi = NEW.id_koleksi) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'id_koleksi sudah terdaftar pada tabel turunan lain';
  END IF;
END$$

CREATE TRIGGER trg_jurnal_validate
BEFORE INSERT ON jurnal
FOR EACH ROW
BEGIN
  IF (SELECT tipe_koleksi FROM koleksi WHERE id_koleksi = NEW.id_koleksi) <> 'JURNAL' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'tipe_koleksi harus JURNAL untuk insert ke tabel jurnal';
  END IF;

  IF EXISTS (SELECT 1 FROM buku WHERE id_koleksi = NEW.id_koleksi)
     OR EXISTS (SELECT 1 FROM cd WHERE id_koleksi = NEW.id_koleksi) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'id_koleksi sudah terdaftar pada tabel turunan lain';
  END IF;
END$$

DELIMITER ;
