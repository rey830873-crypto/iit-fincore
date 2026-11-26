-- ============================================================================
-- IIT FINCORE v2.0 — Mise à niveau Base de Données MySQL
-- Ajoute : Rapprochement bancaire, Exercices comptables (clôture), Opérations récurrentes
-- ============================================================================

USE iit_fincore;

-- 1. Rapprochement bancaire & Mobile Money sur transactions
SET @col_exists := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = 'iit_fincore' AND TABLE_NAME = 'transactions' AND COLUMN_NAME = 'est_rapprochee'
);
SET @sql_add := IF(@col_exists = 0,
    'ALTER TABLE transactions ADD COLUMN est_rapprochee TINYINT(1) NOT NULL DEFAULT 0, ADD COLUMN date_rapprochement DATETIME NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql_add;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2. Exercices comptables (Clôture annuelle)
CREATE TABLE IF NOT EXISTS exercices_comptables (
    id INT AUTO_INCREMENT PRIMARY KEY,
    annee INT NOT NULL UNIQUE,
    libelle VARCHAR(80) NOT NULL,
    date_debut DATE NOT NULL,
    date_fin DATE NOT NULL,
    statut ENUM('OUVERT', 'CLOTURE') NOT NULL DEFAULT 'OUVERT',
    solde_cloture DECIMAL(15, 2) NULL,
    cloture_par INT NULL,
    date_cloture DATETIME NULL,
    CONSTRAINT fk_ex_user FOREIGN KEY (cloture_par) REFERENCES utilisateurs(id) ON DELETE SET NULL
) ENGINE=InnoDB;

INSERT IGNORE INTO exercices_comptables (annee, libelle, date_debut, date_fin, statut) VALUES
(2025, 'Exercice 2025', '2025-01-01', '2025-12-31', 'CLOTURE'),
(2026, 'Exercice 2026', '2026-01-01', '2026-12-31', 'OUVERT');

-- 3. Opérations récurrentes (Charges fixes & prévision Cash-Flow)
CREATE TABLE IF NOT EXISTS operations_recurrentes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    libelle VARCHAR(180) NOT NULL,
    type_operation ENUM('RECETTE', 'DEPENSE') NOT NULL DEFAULT 'DEPENSE',
    montant DECIMAL(15, 2) NOT NULL,
    frequence ENUM('MENSUEL', 'TRIMESTRIEL', 'ANNUEL') NOT NULL DEFAULT 'MENSUEL',
    prochaine_echeance DATE NOT NULL,
    compte_id INT NOT NULL,
    centre_analytique_id INT NOT NULL,
    structure_id INT NOT NULL,
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_rec_montant CHECK (montant > 0),
    CONSTRAINT fk_rec_compte FOREIGN KEY (compte_id) REFERENCES comptes_tresorerie(id),
    CONSTRAINT fk_rec_centre FOREIGN KEY (centre_analytique_id) REFERENCES centres_analytiques(id),
    CONSTRAINT fk_rec_struct FOREIGN KEY (structure_id) REFERENCES structures(id)
) ENGINE=InnoDB;

-- Jeu de données réaliste pour les opérations récurrentes
INSERT INTO operations_recurrentes (libelle, type_operation, montant, frequence, prochaine_echeance, compte_id, centre_analytique_id, structure_id)
SELECT 'Abonnement Internet & Cloud BDE', 'DEPENSE', 85000.00, 'MENSUEL', '2026-12-05', 1, 1, 2
WHERE NOT EXISTS (SELECT 1 FROM operations_recurrentes WHERE libelle = 'Abonnement Internet & Cloud BDE');

INSERT INTO operations_recurrentes (libelle, type_operation, montant, frequence, prochaine_echeance, compte_id, centre_analytique_id, structure_id)
SELECT 'Frais bancaires & tenue de compte', 'DEPENSE', 25000.00, 'MENSUEL', '2026-12-28', 1, 1, 2
WHERE NOT EXISTS (SELECT 1 FROM operations_recurrentes WHERE libelle = 'Frais bancaires & tenue de compte');

INSERT INTO operations_recurrentes (libelle, type_operation, montant, frequence, prochaine_echeance, compte_id, centre_analytique_id, structure_id)
SELECT 'Hébergement serveurs IT CLUB', 'DEPENSE', 45000.00, 'MENSUEL', '2026-12-10', 3, 1, 3
WHERE NOT EXISTS (SELECT 1 FROM operations_recurrentes WHERE libelle = 'Hébergement serveurs IT CLUB');

INSERT INTO operations_recurrentes (libelle, type_operation, montant, frequence, prochaine_echeance, compte_id, centre_analytique_id, structure_id)
SELECT 'Dotation trimestrielle Direction IIT', 'RECETTE', 900000.00, 'TRIMESTRIEL', '2027-01-15', 1, 1, 2
WHERE NOT EXISTS (SELECT 1 FROM operations_recurrentes WHERE libelle = 'Dotation trimestrielle Direction IIT');

-- Trigger : empêche l'insertion d'une transaction dans un exercice déjà CLOTURE
DROP TRIGGER IF EXISTS trg_trx_check_exercice_cloture;
DELIMITER $$
CREATE TRIGGER trg_trx_check_exercice_cloture
BEFORE INSERT ON transactions
FOR EACH ROW
BEGIN
    DECLARE v_cloture INT DEFAULT 0;
    SELECT COUNT(*) INTO v_cloture
      FROM exercices_comptables
     WHERE statut = 'CLOTURE'
       AND NEW.date_operation BETWEEN date_debut AND date_fin;
    IF v_cloture > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Impossible d''enregistrer une opération sur un exercice comptable clôturé.';
    END IF;
END$$
DELIMITER ;
