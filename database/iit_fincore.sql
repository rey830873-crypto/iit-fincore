-- ============================================================================
-- IIT FINCORE — Script de création de la base de données MySQL (8.0+)
-- Projet : Solution de Gestion d'une Mini-Trésorerie d'Association
-- Stack  : C# .NET WinForms + MySQL
-- Devise : FCFA (modifiable dans parametres_generaux)
-- ============================================================================

DROP DATABASE IF EXISTS iit_fincore;
CREATE DATABASE iit_fincore
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE iit_fincore;

-- ============================================================================
-- 1. SECURITE : ROLES, PERMISSIONS, UTILISATEURS
-- ============================================================================

CREATE TABLE roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(30) NOT NULL UNIQUE,           -- ADMIN, TRESORIER, CONSULTANT
    nom VARCHAR(80) NOT NULL,
    description VARCHAR(255) NULL,
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE permissions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(60) NOT NULL UNIQUE,           -- ex: COMPTE_VIEW, RECETTE_CREATE, DEPENSE_VALIDATE
    libelle VARCHAR(120) NOT NULL,
    module VARCHAR(50) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE role_permissions (
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    PRIMARY KEY (role_id, permission_id),
    CONSTRAINT fk_rp_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT fk_rp_perm FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ============================================================================
-- 2. STRUCTURES ASSOCIATIVES (IIT, BDE, IT CLUB, CLUB SPORTIF...)
-- ============================================================================

CREATE TABLE structures (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(30) NOT NULL UNIQUE,
    nom VARCHAR(120) NOT NULL,
    type_structure ENUM('INSTITUTION', 'ASSOCIATION', 'CLUB', 'DEPARTEMENT', 'AUTRE') NOT NULL DEFAULT 'ASSOCIATION',
    parent_id INT NULL,
    responsable_id INT NULL,                    -- FK ajoutée après création de utilisateurs
    email_contact VARCHAR(120) NULL,
    telephone VARCHAR(30) NULL,
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_struct_parent FOREIGN KEY (parent_id) REFERENCES structures(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE utilisateurs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nom_complet VARCHAR(120) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    mot_de_passe_hash VARCHAR(255) NOT NULL,    -- Stockage BCrypt / PBKDF2
    telephone VARCHAR(30) NULL,
    role_id INT NOT NULL,
    structure_id INT NULL,                      -- NULL pour un Admin global
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    dernier_acces DATETIME NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_role FOREIGN KEY (role_id) REFERENCES roles(id),
    CONSTRAINT fk_user_structure FOREIGN KEY (structure_id) REFERENCES structures(id) ON DELETE SET NULL
) ENGINE=InnoDB;

ALTER TABLE structures
    ADD CONSTRAINT fk_struct_responsable
    FOREIGN KEY (responsable_id) REFERENCES utilisateurs(id) ON DELETE SET NULL;

-- ============================================================================
-- 3. COMPTES DE TRESORERIE
-- ============================================================================

CREATE TABLE comptes_tresorerie (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nom_compte VARCHAR(120) NOT NULL,
    structure_id INT NOT NULL,
    type_compte ENUM('BANQUE', 'CAISSE', 'MOBILE_MONEY', 'AUTRE') NOT NULL DEFAULT 'CAISSE',
    etablissement VARCHAR(120) NULL,            -- Banque / Opérateur (ex: NSIA, Orange Money, Caisse interne)
    numero_reference VARCHAR(80) NULL,          -- N° de compte / RIB / N° téléphone
    solde_initial DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    solde_actuel DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    devise VARCHAR(10) NOT NULL DEFAULT 'FCFA',
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_compte_structure FOREIGN KEY (structure_id) REFERENCES structures(id),
    INDEX idx_compte_structure (structure_id)
) ENGINE=InnoDB;

-- ============================================================================
-- 4. CATEGORIES & CENTRES D'IMPUTATION ANALYTIQUES
-- ============================================================================

CREATE TABLE categories_mouvement (
    id INT AUTO_INCREMENT PRIMARY KEY,
    libelle VARCHAR(100) NOT NULL,
    nature ENUM('RECETTE', 'DEPENSE') NOT NULL,
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    UNIQUE KEY uq_cat_libelle_nature (libelle, nature)
) ENGINE=InnoDB;

CREATE TABLE centres_analytiques (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(30) NOT NULL UNIQUE,           -- ex: CA-FONCT, CA-EVENT, CA-MAT
    libelle VARCHAR(120) NOT NULL,
    description VARCHAR(255) NULL,
    parent_id INT NULL,
    structure_id INT NULL,                      -- NULL = centre transversal à toutes les structures
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_centre_parent FOREIGN KEY (parent_id) REFERENCES centres_analytiques(id) ON DELETE SET NULL,
    CONSTRAINT fk_centre_structure FOREIGN KEY (structure_id) REFERENCES structures(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ============================================================================
-- 5. MOUVEMENTS DE TRESORERIE & MULTI-VENTILATION ANALYTIQUE
-- ============================================================================

CREATE TABLE transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    reference VARCHAR(40) NOT NULL UNIQUE,      -- ex: TRX-2026-0001
    date_operation DATE NOT NULL,
    libelle VARCHAR(200) NOT NULL,
    type_transaction ENUM('RECETTE', 'DEPENSE', 'TRANSFERT_SORTANT', 'TRANSFERT_ENTRANT') NOT NULL,
    montant DECIMAL(15, 2) NOT NULL,
    mode_paiement ENUM('ESPECES', 'VIREMENT', 'CHEQUE', 'MOBILE_MONEY', 'CARTE') NOT NULL DEFAULT 'ESPECES',
    justificatif_path VARCHAR(255) NULL,
    statut ENUM('BROUILLON', 'EN_ATTENTE', 'VALIDEE', 'REJETEE', 'ANNULEE') NOT NULL DEFAULT 'VALIDEE',
    compte_id INT NOT NULL,
    compte_contrepartie_id INT NULL,            -- Renseigné en cas de transfert entre comptes
    categorie_id INT NULL,
    structure_id INT NOT NULL,
    cree_par INT NOT NULL,
    valide_par INT NULL,
    date_validation DATETIME NULL,
    observations TEXT NULL,
    transfert_groupe_uuid CHAR(36) NULL,        -- Lie les 2 écritures d'un transfert
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_trx_montant CHECK (montant > 0),
    CONSTRAINT fk_trx_compte FOREIGN KEY (compte_id) REFERENCES comptes_tresorerie(id),
    CONSTRAINT fk_trx_contrepartie FOREIGN KEY (compte_contrepartie_id) REFERENCES comptes_tresorerie(id),
    CONSTRAINT fk_trx_categorie FOREIGN KEY (categorie_id) REFERENCES categories_mouvement(id),
    CONSTRAINT fk_trx_structure FOREIGN KEY (structure_id) REFERENCES structures(id),
    CONSTRAINT fk_trx_cree_par FOREIGN KEY (cree_par) REFERENCES utilisateurs(id),
    CONSTRAINT fk_trx_valide_par FOREIGN KEY (valide_par) REFERENCES utilisateurs(id),
    INDEX idx_trx_date (date_operation),
    INDEX idx_trx_structure_statut (structure_id, statut),
    INDEX idx_trx_compte_statut (compte_id, statut)
) ENGINE=InnoDB;

CREATE TABLE imputations_analytiques (
    id INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id INT NOT NULL,
    centre_analytique_id INT NOT NULL,
    montant_impute DECIMAL(15, 2) NOT NULL,
    pourcentage DECIMAL(5, 2) NOT NULL,         -- 0.01 à 100.00
    CONSTRAINT chk_imp_montant CHECK (montant_impute > 0),
    CONSTRAINT chk_imp_pct CHECK (pourcentage > 0 AND pourcentage <= 100.00),
    CONSTRAINT uq_imp_trx_centre UNIQUE (transaction_id, centre_analytique_id),
    CONSTRAINT fk_imp_trx FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
    CONSTRAINT fk_imp_centre FOREIGN KEY (centre_analytique_id) REFERENCES centres_analytiques(id)
) ENGINE=InnoDB;

-- ============================================================================
-- 6. CIRCUIT DE VALIDATION DES DEMANDES
-- ============================================================================

CREATE TABLE demandes_validation (
    id INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id INT NOT NULL UNIQUE,
    demandeur_id INT NOT NULL,
    validateur_id INT NULL,
    motif_demande TEXT NOT NULL,
    commentaire_decision TEXT NULL,
    statut ENUM('EN_ATTENTE', 'APPROUVEE', 'REJETEE') NOT NULL DEFAULT 'EN_ATTENTE',
    date_demande DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_decision DATETIME NULL,
    CONSTRAINT fk_val_trx FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
    CONSTRAINT fk_val_demandeur FOREIGN KEY (demandeur_id) REFERENCES utilisateurs(id),
    CONSTRAINT fk_val_validateur FOREIGN KEY (validateur_id) REFERENCES utilisateurs(id)
) ENGINE=InnoDB;

-- ============================================================================
-- 7. BUDGETS & SUIVI BUDGETAIRE
-- ============================================================================

CREATE TABLE budgets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    intitule VARCHAR(150) NOT NULL,
    structure_id INT NOT NULL,
    centre_analytique_id INT NOT NULL,
    montant_prevu DECIMAL(15, 2) NOT NULL,
    date_debut DATE NOT NULL,
    date_fin DATE NOT NULL,
    statut ENUM('ACTIF', 'CLOTURE', 'ARCHIVE') NOT NULL DEFAULT 'ACTIF',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_budget_montant CHECK (montant_prevu > 0),
    CONSTRAINT chk_budget_dates CHECK (date_fin >= date_debut),
    CONSTRAINT fk_budget_structure FOREIGN KEY (structure_id) REFERENCES structures(id),
    CONSTRAINT fk_budget_centre FOREIGN KEY (centre_analytique_id) REFERENCES centres_analytiques(id)
) ENGINE=InnoDB;

-- ============================================================================
-- 8. COTISATIONS & MEMBRES
-- ============================================================================

CREATE TABLE membres (
    id INT AUTO_INCREMENT PRIMARY KEY,
    matricule VARCHAR(30) NULL UNIQUE,
    nom_complet VARCHAR(120) NOT NULL,
    email VARCHAR(120) NULL,
    telephone VARCHAR(30) NULL,
    structure_id INT NOT NULL,
    est_actif TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_membre_structure FOREIGN KEY (structure_id) REFERENCES structures(id)
) ENGINE=InnoDB;

CREATE TABLE cotisations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nom VARCHAR(120) NOT NULL,
    structure_id INT NOT NULL,
    montant_par_membre DECIMAL(15, 2) NOT NULL,
    date_limite DATE NOT NULL,
    population_concernee VARCHAR(150) NULL DEFAULT 'Tous les membres',
    statut ENUM('ACTIVE', 'CLOTUREE', 'ARCHIVEE') NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_cotis_montant CHECK (montant_par_membre > 0),
    CONSTRAINT fk_cotis_structure FOREIGN KEY (structure_id) REFERENCES structures(id)
) ENGINE=InnoDB;

CREATE TABLE paiements_cotisations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cotisation_id INT NOT NULL,
    membre_id INT NOT NULL,
    montant_du DECIMAL(15, 2) NOT NULL,
    montant_paye DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    date_echeance DATE NOT NULL,
    date_paiement DATE NULL,
    statut ENUM('PAYE', 'PARTIEL', 'EN_ATTENTE', 'IMPAYE') NOT NULL DEFAULT 'EN_ATTENTE',
    transaction_id INT NULL,                    -- Lien vers la recette générée
    CONSTRAINT uq_paiement_cotis_membre UNIQUE (cotisation_id, membre_id),
    CONSTRAINT fk_pc_cotisation FOREIGN KEY (cotisation_id) REFERENCES cotisations(id) ON DELETE CASCADE,
    CONSTRAINT fk_pc_membre FOREIGN KEY (membre_id) REFERENCES membres(id) ON DELETE CASCADE,
    CONSTRAINT fk_pc_transaction FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ============================================================================
-- 9. AUDIT & PARAMETRES GENERAUX
-- ============================================================================

CREATE TABLE journal_activites (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    date_action DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    utilisateur_id INT NULL,
    action VARCHAR(120) NOT NULL,
    module VARCHAR(60) NOT NULL,
    entite_cible VARCHAR(60) NULL,
    entite_id INT NULL,
    details TEXT NULL,
    adresse_ip VARCHAR(45) NULL,
    CONSTRAINT fk_audit_user FOREIGN KEY (utilisateur_id) REFERENCES utilisateurs(id) ON DELETE SET NULL,
    INDEX idx_audit_date (date_action),
    INDEX idx_audit_module (module)
) ENGINE=InnoDB;

CREATE TABLE parametres_generaux (
    cle_param VARCHAR(80) PRIMARY KEY,
    valeur_param VARCHAR(255) NOT NULL,
    description VARCHAR(255) NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ============================================================================
-- 10. TRIGGERS : MISE A JOUR AUTOMATIQUE DES SOLDES & CONTROLE VENTILATION
-- ============================================================================

DELIMITER $$

-- Initialise solde_actuel = solde_initial à la création d'un compte
CREATE TRIGGER trg_compte_before_insert
BEFORE INSERT ON comptes_tresorerie
FOR EACH ROW
BEGIN
    SET NEW.solde_actuel = NEW.solde_initial;
END$$

-- Met à jour solde_actuel après insertion d'une transaction déjà VALIDEE
CREATE TRIGGER trg_trx_after_insert
AFTER INSERT ON transactions
FOR EACH ROW
BEGIN
    IF NEW.statut = 'VALIDEE' THEN
        IF NEW.type_transaction IN ('RECETTE', 'TRANSFERT_ENTRANT') THEN
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel + NEW.montant WHERE id = NEW.compte_id;
        ELSE
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel - NEW.montant WHERE id = NEW.compte_id;
        END IF;
    END IF;
END$$

-- Met à jour solde_actuel lors d'un changement de statut (ex: EN_ATTENTE -> VALIDEE, ou VALIDEE -> ANNULEE)
CREATE TRIGGER trg_trx_after_update
AFTER UPDATE ON transactions
FOR EACH ROW
BEGIN
    -- Annulation de l'ancien impact si elle était VALIDEE
    IF OLD.statut = 'VALIDEE' THEN
        IF OLD.type_transaction IN ('RECETTE', 'TRANSFERT_ENTRANT') THEN
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel - OLD.montant WHERE id = OLD.compte_id;
        ELSE
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel + OLD.montant WHERE id = OLD.compte_id;
        END IF;
    END IF;

    -- Application du nouvel impact si elle devient/reste VALIDEE
    IF NEW.statut = 'VALIDEE' THEN
        IF NEW.type_transaction IN ('RECETTE', 'TRANSFERT_ENTRANT') THEN
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel + NEW.montant WHERE id = NEW.compte_id;
        ELSE
            UPDATE comptes_tresorerie SET solde_actuel = solde_actuel - NEW.montant WHERE id = NEW.compte_id;
        END IF;
    END IF;
END$$

-- Contrôle qu'une imputation ne dépasse pas le montant total de la transaction
CREATE TRIGGER trg_imputation_before_insert
BEFORE INSERT ON imputations_analytiques
FOR EACH ROW
BEGIN
    DECLARE v_montant_trx DECIMAL(15,2);
    DECLARE v_deja_impute DECIMAL(15,2);

    SELECT montant INTO v_montant_trx FROM transactions WHERE id = NEW.transaction_id;
    SELECT COALESCE(SUM(montant_impute), 0) INTO v_deja_impute
      FROM imputations_analytiques
     WHERE transaction_id = NEW.transaction_id;

    IF (v_deja_impute + NEW.montant_impute) > v_montant_trx THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La somme des imputations analytiques dépasse le montant de la transaction.';
    END IF;
END$$

-- Procédure stockée pour effectuer un transfert atomique entre deux comptes
CREATE PROCEDURE sp_effectuer_transfert_comptes(
    IN p_compte_source_id INT,
    IN p_compte_dest_id INT,
    IN p_montant DECIMAL(15,2),
    IN p_date_op DATE,
    IN p_motif VARCHAR(200),
    IN p_utilisateur_id INT
)
BEGIN
    DECLARE v_uuid CHAR(36);
    DECLARE v_struct_src INT;
    DECLARE v_struct_dst INT;
    DECLARE v_solde_src DECIMAL(15,2);
    DECLARE v_ref_base VARCHAR(32);

    IF p_compte_source_id = p_compte_dest_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Le compte source et le compte destinataire doivent être différents.';
    END IF;

    IF p_montant <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Le montant du transfert doit être strictement positif.';
    END IF;

    SELECT structure_id, solde_actuel INTO v_struct_src, v_solde_src
      FROM comptes_tresorerie WHERE id = p_compte_source_id FOR UPDATE;
    SELECT structure_id INTO v_struct_dst
      FROM comptes_tresorerie WHERE id = p_compte_dest_id FOR UPDATE;

    IF v_solde_src < p_montant THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Solde insuffisant sur le compte source.';
    END IF;

    SET v_uuid = UUID();
    SET v_ref_base = CONCAT('TRF-', DATE_FORMAT(p_date_op, '%Y%m%d'), '-', LPAD(FLOOR(RAND() * 9999), 4, '0'));

    START TRANSACTION;
        INSERT INTO transactions (
            reference, date_operation, libelle, type_transaction, montant,
            mode_paiement, statut, compte_id, compte_contrepartie_id,
            structure_id, cree_par, valide_par, date_validation, transfert_groupe_uuid
        ) VALUES (
            CONCAT(v_ref_base, '-OUT'), p_date_op, p_motif, 'TRANSFERT_SORTANT', p_montant,
            'VIREMENT', 'VALIDEE', p_compte_source_id, p_compte_dest_id,
            v_struct_src, p_utilisateur_id, p_utilisateur_id, NOW(), v_uuid
        );

        INSERT INTO transactions (
            reference, date_operation, libelle, type_transaction, montant,
            mode_paiement, statut, compte_id, compte_contrepartie_id,
            structure_id, cree_par, valide_par, date_validation, transfert_groupe_uuid
        ) VALUES (
            CONCAT(v_ref_base, '-IN'), p_date_op, p_motif, 'TRANSFERT_ENTRANT', p_montant,
            'VIREMENT', 'VALIDEE', p_compte_dest_id, p_compte_source_id,
            v_struct_dst, p_utilisateur_id, p_utilisateur_id, NOW(), v_uuid
        );

        INSERT INTO journal_activites (utilisateur_id, action, module, entite_cible, details)
        VALUES (p_utilisateur_id, 'Transfert entre comptes', 'Transactions', 'transactions',
                CONCAT('Transfert de ', p_montant, ' FCFA (', v_ref_base, ')'));
    COMMIT;
END$$

DELIMITER ;

-- ============================================================================
-- 11. VUES DE REPORTING (TABLEAUX DE BORD, ANALYTIQUE, BUDGETS)
-- ============================================================================

CREATE VIEW v_synthese_analytique AS
SELECT
    s.id AS structure_id,
    s.nom AS structure_nom,
    ca.id AS centre_id,
    ca.code AS centre_code,
    ca.libelle AS centre_libelle,
    COALESCE(SUM(CASE WHEN t.type_transaction = 'RECETTE' AND t.statut = 'VALIDEE' THEN ia.montant_impute ELSE 0 END), 0) AS total_recettes,
    COALESCE(SUM(CASE WHEN t.type_transaction = 'DEPENSE' AND t.statut = 'VALIDEE' THEN ia.montant_impute ELSE 0 END), 0) AS total_depenses,
    COALESCE(SUM(CASE WHEN t.type_transaction = 'RECETTE' AND t.statut = 'VALIDEE' THEN ia.montant_impute
                      WHEN t.type_transaction = 'DEPENSE' AND t.statut = 'VALIDEE' THEN -ia.montant_impute
                      ELSE 0 END), 0) AS resultat_net
FROM centres_analytiques ca
LEFT JOIN imputations_analytiques ia ON ia.centre_analytique_id = ca.id
LEFT JOIN transactions t ON t.id = ia.transaction_id
LEFT JOIN structures s ON s.id = COALESCE(t.structure_id, ca.structure_id)
GROUP BY s.id, s.nom, ca.id, ca.code, ca.libelle;

CREATE VIEW v_suivi_budgetaire AS
SELECT
    b.id AS budget_id,
    b.intitule,
    s.nom AS structure_nom,
    ca.libelle AS centre_analytique,
    b.date_debut,
    b.date_fin,
    b.montant_prevu,
    COALESCE(SUM(CASE WHEN t.id IS NOT NULL THEN ia.montant_impute ELSE 0 END), 0) AS montant_realise,
    (b.montant_prevu - COALESCE(SUM(CASE WHEN t.id IS NOT NULL THEN ia.montant_impute ELSE 0 END), 0)) AS reste_a_consommer,
    ROUND((COALESCE(SUM(CASE WHEN t.id IS NOT NULL THEN ia.montant_impute ELSE 0 END), 0) / b.montant_prevu) * 100, 2) AS taux_execution_pct,
    b.statut
FROM budgets b
JOIN structures s ON s.id = b.structure_id
JOIN centres_analytiques ca ON ca.id = b.centre_analytique_id
LEFT JOIN imputations_analytiques ia ON ia.centre_analytique_id = b.centre_analytique_id
LEFT JOIN transactions t ON t.id = ia.transaction_id
    AND t.structure_id = b.structure_id
    AND t.type_transaction = 'DEPENSE'
    AND t.statut = 'VALIDEE'
    AND t.date_operation BETWEEN b.date_debut AND b.date_fin
GROUP BY b.id, b.intitule, s.nom, ca.libelle, b.date_debut, b.date_fin, b.montant_prevu, b.statut;

-- ============================================================================
-- 12. DONNEES INITIALES (3 ROLES, PERMISSIONS, STRUCTURES, EXEMPLES)
-- ============================================================================

INSERT INTO roles (code, nom, description) VALUES
('ADMIN',      'Administrateur', 'Accès complet : gestion globale, validations, utilisateurs et audit'),
('TRESORIER',  'Trésorier',      'Gestion financière de sa structure : comptes, opérations, budgets et cotisations'),
('CONSULTANT', 'Consultant',     'Consultation uniquement : tableaux de bord, listes et export de rapports');

INSERT INTO permissions (code, libelle, module) VALUES
('DASHBOARD_VIEW',   'Consulter les tableaux de bord', 'Tableaux de bord'),
('STRUCTURE_MANAGE', 'Gérer les structures',           'Structures'),
('COMPTE_VIEW',      'Consulter les comptes',          'Comptes'),
('COMPTE_MANAGE',    'Créer / modifier un compte',     'Comptes'),
('RECETTE_CREATE',   'Créer une recette',              'Transactions'),
('DEPENSE_CREATE',   'Créer une dépense',              'Transactions'),
('TRANSFERT_CREATE', 'Effectuer un transfert',         'Transactions'),
('DEPENSE_VALIDATE', 'Valider / rejeter une dépense',  'Validations'),
('CENTRE_MANAGE',    'Gérer les centres analytiques',  'Analytique'),
('BUDGET_MANAGE',    'Gérer les budgets',              'Budgets'),
('COTISATION_MANAGE','Gérer les cotisations',          'Cotisations'),
('RAPPORT_EXPORT',   'Exporter les rapports',          'Rapports'),
('USER_MANAGE',      'Gérer les utilisateurs & rôles', 'Utilisateurs'),
('AUDIT_VIEW',       'Consulter le journal d''audit',  'Audit');

-- Admin : toutes les permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r CROSS JOIN permissions p WHERE r.code = 'ADMIN';

-- Trésorier : gestion financière (hors gestion utilisateurs, audit global et validation finale)
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON p.code IN (
    'DASHBOARD_VIEW', 'COMPTE_VIEW', 'COMPTE_MANAGE', 'RECETTE_CREATE',
    'DEPENSE_CREATE', 'TRANSFERT_CREATE', 'CENTRE_MANAGE', 'BUDGET_MANAGE',
    'COTISATION_MANAGE', 'RAPPORT_EXPORT'
) WHERE r.code = 'TRESORIER';

-- Consultant : lecture & export uniquement
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON p.code IN (
    'DASHBOARD_VIEW', 'COMPTE_VIEW', 'RAPPORT_EXPORT'
) WHERE r.code = 'CONSULTANT';

INSERT INTO structures (code, nom, type_structure, parent_id, email_contact, telephone) VALUES
('IIT',     'Institut Ivoirien de Technologie', 'INSTITUTION', NULL, 'contact@iit.ci', '+225 07 00 00 00 00'),
('BDE',     'Bureau des Étudiants (BDE)',       'ASSOCIATION', 1,    'bde@iit.ci',     '+225 07 01 02 03 04'),
('ITCLUB',  'IT CLUB',                          'CLUB',        2,    'itclub@iit.ci',  '+225 07 05 06 07 08'),
('SPORT',   'Club Sportif',                     'CLUB',        2,    'sport@iit.ci',   '+225 07 09 10 11 12');

-- Utilisateurs de démo (mots de passe à hasher en BCrypt depuis l'application C#)
INSERT INTO utilisateurs (nom_complet, email, mot_de_passe_hash, telephone, role_id, structure_id) VALUES
('Admin IIT',       'admin@iit.ci',      '$2a$11$PLACEHOLDER_HASH_ADMIN',      '+225 07 00 00 00 01', 1, 1),
('Kouassi Jean',    'tresorier@iit.ci',  '$2a$11$PLACEHOLDER_HASH_TRESORIER',  '+225 07 00 00 00 02', 2, 2),
('Yao Mariam',      'consultant@iit.ci', '$2a$11$PLACEHOLDER_HASH_CONSULTANT', '+225 07 00 00 00 03', 3, 1);

INSERT INTO categories_mouvement (libelle, nature) VALUES
('Cotisation',      'RECETTE'),
('Don / Sponsoring','RECETTE'),
('Subvention',      'RECETTE'),
('Billetterie',     'RECETTE'),
('Matériel',        'DEPENSE'),
('Événements',      'DEPENSE'),
('Fonctionnement',  'DEPENSE'),
('Prestation',      'DEPENSE');

INSERT INTO centres_analytiques (code, libelle, description) VALUES
('CA-FONCT', 'Fonctionnement', 'Frais généraux et fonctionnement courant'),
('CA-EVENT', 'Événements',     'Activités, journées culturelles et conférences'),
('CA-MAT',   'Matériel',       'Achat et renouvellement d''équipements'),
('CA-AUTRE', 'Autres',         'Imputations diverses');

INSERT INTO comptes_tresorerie (nom_compte, structure_id, type_compte, etablissement, numero_reference, solde_initial) VALUES
('Compte BDE',       2, 'BANQUE',       'NSIA Banque',   'CI001-000123456', 2500000.00),
('Caisse BDE',       2, 'CAISSE',       'Caisse interne','CAISSE-BDE-01',    350000.00),
('Compte IT CLUB',   3, 'MOBILE_MONEY', 'Orange Money',  '+225 07 05 06 07', 450000.00);

INSERT INTO parametres_generaux (cle_param, valeur_param, description) VALUES
('DEVISE_DEFAUT',         'FCFA',   'Devise principale de la trésorerie'),
('SEUIL_VALIDATION_DEP',  '100000', 'Montant (FCFA) au-delà duquel une dépense requiert validation'),
('NOM_ORGANISATION',      'IIT FINCORE', 'Nom affiché dans les rapports et l''interface');

-- Exemple de dépense ventilée sur 2 centres analytiques (60% Matériel / 40% Fonctionnement)
INSERT INTO transactions (
    reference, date_operation, libelle, type_transaction, montant, mode_paiement,
    statut, compte_id, categorie_id, structure_id, cree_par, valide_par, date_validation
) VALUES (
    'TRX-2026-0001', '2026-11-20', 'Achat matériel informatique', 'DEPENSE', 320000.00, 'VIREMENT',
    'VALIDEE', 1, 5, 2, 2, 1, NOW()
);

INSERT INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage) VALUES
(1, 3, 192000.00, 60.00),
(1, 1, 128000.00, 40.00);
