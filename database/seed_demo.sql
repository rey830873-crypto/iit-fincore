-- ============================================================================
-- IIT FINCORE — Jeu de données de démonstration commercial complet (6 mois)
-- Compatible avec la base existante (préserve les utilisateurs et mots de passe)
-- ============================================================================

USE iit_fincore;

-- 1. Structures complémentaires
INSERT IGNORE INTO structures (code, nom, type_structure, parent_id, email_contact, telephone) VALUES
('ROBO',    'Club Robotique & IA',       'CLUB',        2, 'robotique@iit.ci', '+225 07 11 22 33 44'),
('CULTURE', 'Association Culturelle',    'ASSOCIATION', 2, 'culture@iit.ci',   '+225 07 55 66 77 88');

-- 2. Centres analytiques complémentaires
INSERT IGNORE INTO centres_analytiques (code, libelle, description) VALUES
('CA-FORM', 'Formation & Ateliers', 'Formations techniques, hackathons et ateliers étudiants'),
('CA-COMM', 'Communication',        'Affiches, supports digitaux, réseaux sociaux et goodies');

-- 3. Comptes de trésorerie complémentaires
INSERT INTO comptes_tresorerie (nom_compte, structure_id, type_compte, etablissement, numero_reference, solde_initial, devise)
SELECT 'Compte Événements IIT', 1, 'BANQUE', 'Société Générale CI', 'CI008-99887766', 4500000.00, 'FCFA'
WHERE NOT EXISTS (SELECT 1 FROM comptes_tresorerie WHERE nom_compte = 'Compte Événements IIT');

INSERT INTO comptes_tresorerie (nom_compte, structure_id, type_compte, etablissement, numero_reference, solde_initial, devise)
SELECT 'Wave Club Sportif', 4, 'MOBILE_MONEY', 'Wave CI', '+225 07 09 10 11', 620000.00, 'FCFA'
WHERE NOT EXISTS (SELECT 1 FROM comptes_tresorerie WHERE nom_compte = 'Wave Club Sportif');

INSERT INTO comptes_tresorerie (nom_compte, structure_id, type_compte, etablissement, numero_reference, solde_initial, devise)
SELECT 'Caisse Robotique', (SELECT id FROM structures WHERE code = 'ROBO' LIMIT 1), 'CAISSE', 'Caisse interne', 'CAISSE-ROBO-01', 280000.00, 'FCFA'
WHERE NOT EXISTS (SELECT 1 FROM comptes_tresorerie WHERE nom_compte = 'Caisse Robotique');

-- 4. Mouvements réalistes sur 6 mois (Juin -> Novembre 2026) pour alimenter les graphiques
INSERT IGNORE INTO transactions (
    reference, date_operation, libelle, type_transaction, montant, mode_paiement,
    statut, compte_id, categorie_id, structure_id, cree_par, valide_par, date_validation, observations
) VALUES
-- JUIN 2026
('TRX-202606-001', '2026-06-05', 'Subvention annuelle Direction IIT',     'RECETTE', 3500000.00, 'VIREMENT',     'VALIDEE', 1, 3, 2, 1, 1, '2026-06-05 10:00:00', 'Dotation annuelle BDE'),
('TRX-202606-002', '2026-06-18', 'Fournitures de bureau et reprographie', 'DEPENSE',  420000.00, 'ESPECES',      'VALIDEE', 2, 7, 2, 2, 1, '2026-06-18 14:30:00', 'Fonctionnement mensuel'),

-- JUILLET 2026
('TRX-202607-001', '2026-07-04', 'Sponsoring Orange Digital Center',      'RECETTE', 1800000.00, 'VIREMENT',     'VALIDEE', 3, 2, 3, 1, 1, '2026-07-04 09:15:00', 'Partenariat Hackathon IT CLUB'),
('TRX-202607-002', '2026-07-19', 'Organisation Hackathon IIT 2026',       'DEPENSE', 1150000.00, 'VIREMENT',     'VALIDEE', 3, 6, 3, 2, 1, '2026-07-19 16:45:00', 'Logistique, restauration et prix'),

-- AOUT 2026
('TRX-202608-001', '2026-08-10', 'Cotisations estivales Club Sportif',    'RECETTE',  750000.00, 'MOBILE_MONEY', 'VALIDEE', 5, 1, 4, 2, 1, '2026-08-10 11:20:00', 'Vague 1 cotisations sport'),
('TRX-202608-002', '2026-08-22', 'Achat équipements sportifs & maillots', 'DEPENSE',  580000.00, 'MOBILE_MONEY', 'VALIDEE', 5, 5, 4, 2, 1, '2026-08-22 15:10:00', 'Tournoi inter-écoles'),

-- SEPTEMBRE 2026
('TRX-202609-001', '2026-09-08', 'Inscriptions & cotisations rentrée BDE','RECETTE', 2450000.00, 'ESPECES',      'VALIDEE', 1, 1, 2, 2, 1, '2026-09-08 17:00:00', 'Campagne rentrée 2026-2027'),
('TRX-202609-002', '2026-09-21', 'Journée d''intégration des nouveaux',   'DEPENSE', 1380000.00, 'VIREMENT',     'VALIDEE', 1, 6, 2, 2, 1, '2026-09-21 12:30:00', 'Sonorisation, traiteur, communication'),

-- OCTOBRE 2026
('TRX-202610-001', '2026-10-06', 'Billetterie Conférence Tech & IA',      'RECETTE', 1250000.00, 'MOBILE_MONEY', 'VALIDEE', 4, 4, 1, 1, 1, '2026-10-06 10:40:00', '250 billets vendus'),
('TRX-202610-002', '2026-10-15', 'Prestation audiovisuelle & streaming',  'DEPENSE',  690000.00, 'CHEQUE',       'VALIDEE', 4, 8, 1, 1, 1, '2026-10-15 16:00:00', 'Captation HD conférence'),
('TRX-202610-003', '2026-10-28', 'Achat kits Arduino & capteurs IA',      'DEPENSE',  460000.00, 'VIREMENT',     'VALIDEE', 1, 5, 2, 2, 1, '2026-10-28 11:00:00', 'Matériel ateliers pratiques'),

-- NOVEMBRE 2026
('TRX-202611-001', '2026-11-05', 'Sponsoring Gala de Fin d''Année',       'RECETTE', 2200000.00, 'VIREMENT',     'VALIDEE', 1, 2, 2, 2, 1, '2026-11-05 09:30:00', 'Partenaire Gold NSIA'),
('TRX-202611-002', '2026-11-12', 'Acompte salle & décoration Gala BDE',   'DEPENSE',  950000.00, 'VIREMENT',     'VALIDEE', 1, 6, 2, 2, 1, '2026-11-12 15:20:00', 'Réservation espace prestige'),

-- 2 DÉPENSES EN ATTENTE DE VALIDATION (pour le Dashboard Administrateur)
('TRX-202611-003', '2026-11-24', 'Achat vidéoprojecteur 4K amphithéâtre', 'DEPENSE',  480000.00, 'VIREMENT',     'EN_ATTENTE', 1, 5, 2, 2, NULL, NULL, 'Renouvellement équipement projections BDE'),
('TRX-202611-004', '2026-11-25', 'Impression affiches & badges Gala',     'DEPENSE',  195000.00, 'ESPECES',      'EN_ATTENTE', 2, 6, 2, 2, NULL, NULL, 'Supports communication événement annuel');

-- 5. Multi-ventilation analytique sur les transactions insérées
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 1, 3500000.00, 100.00 FROM transactions WHERE reference = 'TRX-202606-001';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 1, 420000.00, 100.00 FROM transactions WHERE reference = 'TRX-202606-002';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 1800000.00, 100.00 FROM transactions WHERE reference = 'TRX-202607-001';

-- Multi-ventilation 70% Événements / 30% Formation
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 805000.00, 70.00 FROM transactions WHERE reference = 'TRX-202607-002';
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT t.id, ca.id, 345000.00, 30.00 FROM transactions t JOIN centres_analytiques ca ON ca.code = 'CA-FORM' WHERE t.reference = 'TRX-202607-002';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 750000.00, 100.00 FROM transactions WHERE reference = 'TRX-202608-001';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 3, 580000.00, 100.00 FROM transactions WHERE reference = 'TRX-202608-002';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 1, 2450000.00, 100.00 FROM transactions WHERE reference = 'TRX-202609-001';

-- Multi-ventilation 65% Événements / 35% Communication
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 897000.00, 65.00 FROM transactions WHERE reference = 'TRX-202609-002';
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT t.id, ca.id, 483000.00, 35.00 FROM transactions t JOIN centres_analytiques ca ON ca.code = 'CA-COMM' WHERE t.reference = 'TRX-202609-002';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 1250000.00, 100.00 FROM transactions WHERE reference = 'TRX-202610-001';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT t.id, ca.id, 690000.00, 100.00 FROM transactions t JOIN centres_analytiques ca ON ca.code = 'CA-COMM' WHERE t.reference = 'TRX-202610-002';

-- Multi-ventilation 50% Matériel / 50% Formation
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 3, 230000.00, 50.00 FROM transactions WHERE reference = 'TRX-202610-003';
INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT t.id, ca.id, 230000.00, 50.00 FROM transactions t JOIN centres_analytiques ca ON ca.code = 'CA-FORM' WHERE t.reference = 'TRX-202610-003';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 2200000.00, 100.00 FROM transactions WHERE reference = 'TRX-202611-001';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 2, 950000.00, 100.00 FROM transactions WHERE reference = 'TRX-202611-002';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT id, 3, 480000.00, 100.00 FROM transactions WHERE reference = 'TRX-202611-003';

INSERT IGNORE INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
SELECT t.id, ca.id, 195000.00, 100.00 FROM transactions t JOIN centres_analytiques ca ON ca.code = 'CA-COMM' WHERE t.reference = 'TRX-202611-004';

-- 6. Demandes de validation en attente
INSERT IGNORE INTO demandes_validation (transaction_id, demandeur_id, motif_demande, statut, date_demande)
SELECT id, 2, 'Renouvellement du vidéoprojecteur principal pour les assemblées et conférences BDE.', 'EN_ATTENTE', '2026-11-24 10:15:00'
FROM transactions WHERE reference = 'TRX-202611-003';

INSERT IGNORE INTO demandes_validation (transaction_id, demandeur_id, motif_demande, statut, date_demande)
SELECT id, 2, 'Impression des badges VIP et affiches grand format pour le Gala annuel.', 'EN_ATTENTE', '2026-11-25 14:40:00'
FROM transactions WHERE reference = 'TRX-202611-004';

-- 7. Budgets 2026 (démontrant Maîtrisé, Alerte >= 80%, et Dépassé >= 100%)
INSERT INTO budgets (intitule, structure_id, centre_analytique_id, montant_prevu, date_debut, date_fin, statut)
SELECT 'Budget Fonctionnement BDE 2026', 2, 1, 1200000.00, '2026-01-01', '2026-12-31', 'ACTIF'
WHERE NOT EXISTS (SELECT 1 FROM budgets WHERE intitule = 'Budget Fonctionnement BDE 2026');

INSERT INTO budgets (intitule, structure_id, centre_analytique_id, montant_prevu, date_debut, date_fin, statut)
SELECT 'Budget Événements BDE 2026', 2, 2, 1750000.00, '2026-01-01', '2026-12-31', 'ACTIF'
WHERE NOT EXISTS (SELECT 1 FROM budgets WHERE intitule = 'Budget Événements BDE 2026');

INSERT INTO budgets (intitule, structure_id, centre_analytique_id, montant_prevu, date_debut, date_fin, statut)
SELECT 'Budget Matériel BDE 2026', 2, 3, 500000.00, '2026-01-01', '2026-12-31', 'ACTIF'
WHERE NOT EXISTS (SELECT 1 FROM budgets WHERE intitule = 'Budget Matériel BDE 2026');

INSERT INTO budgets (intitule, structure_id, centre_analytique_id, montant_prevu, date_debut, date_fin, statut)
SELECT 'Budget Hackathon IT CLUB 2026', 3, 2, 1000000.00, '2026-01-01', '2026-12-31', 'ACTIF'
WHERE NOT EXISTS (SELECT 1 FROM budgets WHERE intitule = 'Budget Hackathon IT CLUB 2026');

-- 8. Membres & Cotisations (Payés, Partiels, Impayés)
INSERT IGNORE INTO membres (matricule, nom_complet, email, telephone, structure_id) VALUES
('IIT-2026-001', 'Kouassi Ange',      'a.kouassi@iit.ci',  '+225 07 10 20 30 01', 2),
('IIT-2026-002', 'Yao Merveille',     'm.yao@iit.ci',      '+225 07 10 20 30 02', 3),
('IIT-2026-003', 'Konan Serge',       's.konan@iit.ci',    '+225 07 10 20 30 03', 2),
('IIT-2026-004', 'Traoré Kader',      'k.traore@iit.ci',   '+225 07 10 20 30 04', 4),
('IIT-2026-005', 'Diallo Fatoumata',  'f.diallo@iit.ci',   '+225 07 10 20 30 05', 2),
('IIT-2026-006', 'N''Guessan Brice',  'b.nguessan@iit.ci', '+225 07 10 20 30 06', 2),
('IIT-2026-007', 'Bamba Ibrahim',     'i.bamba@iit.ci',    '+225 07 10 20 30 07', 3),
('IIT-2026-008', 'Touré Aminata',     'a.toure@iit.ci',    '+225 07 10 20 30 08', 4);

INSERT INTO cotisations (nom, structure_id, montant_par_membre, date_limite, population_concernee, statut)
SELECT 'Cotisation Annuelle BDE 2026', 2, 10000.00, '2026-11-30', 'Tous les étudiants IIT', 'ACTIVE'
WHERE NOT EXISTS (SELECT 1 FROM cotisations WHERE nom = 'Cotisation Annuelle BDE 2026');

INSERT INTO cotisations (nom, structure_id, montant_par_membre, date_limite, population_concernee, statut)
SELECT 'Adhésion IT CLUB 2026', 3, 5000.00, '2026-11-30', 'Membres IT CLUB', 'ACTIVE'
WHERE NOT EXISTS (SELECT 1 FROM cotisations WHERE nom = 'Adhésion IT CLUB 2026');

INSERT INTO cotisations (nom, structure_id, montant_par_membre, date_limite, population_concernee, statut)
SELECT 'Licence Club Sportif 2026', 4, 8000.00, '2026-11-25', 'Athlètes & adhérents', 'ACTIVE'
WHERE NOT EXISTS (SELECT 1 FROM cotisations WHERE nom = 'Licence Club Sportif 2026');

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Cotisation Annuelle BDE 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-001' LIMIT 1), 10000.00, 10000.00, '2026-11-30', '2026-11-05', 'PAYE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Cotisation Annuelle BDE 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-003' LIMIT 1), 10000.00, 10000.00, '2026-11-30', '2026-11-10', 'PAYE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Cotisation Annuelle BDE 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-005' LIMIT 1), 10000.00, 5000.00, '2026-11-30', '2026-11-14', 'PARTIEL';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Cotisation Annuelle BDE 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-006' LIMIT 1), 10000.00, 0.00, '2026-11-30', NULL, 'IMPAYE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Adhésion IT CLUB 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-002' LIMIT 1), 5000.00, 5000.00, '2026-11-30', '2026-11-12', 'PAYE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Adhésion IT CLUB 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-007' LIMIT 1), 5000.00, 0.00, '2026-11-30', NULL, 'EN_ATTENTE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Licence Club Sportif 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-004' LIMIT 1), 8000.00, 0.00, '2026-11-25', NULL, 'IMPAYE';

INSERT IGNORE INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut)
SELECT (SELECT id FROM cotisations WHERE nom = 'Licence Club Sportif 2026' LIMIT 1),
       (SELECT id FROM membres WHERE matricule = 'IIT-2026-008' LIMIT 1), 8000.00, 8000.00, '2026-11-25', '2026-11-18', 'PAYE';

-- 9. Journal d'audit réaliste
INSERT INTO journal_activites (date_action, utilisateur_id, action, module, entite_cible, details, adresse_ip) VALUES
('2026-11-20 09:15:00', 1, 'Validation dépense',  'Validations',  'transactions', 'Approbation TRX-202611-002 (950 000 FCFA)', '192.168.1.10'),
('2026-11-21 10:42:00', 2, 'Création recette',    'Transactions', 'transactions', 'Sponsoring Gala de Fin d''Année (2 200 000 FCFA)', '192.168.1.12'),
('2026-11-22 16:20:00', 1, 'Création budget',     'Budgets',      'budgets',      'Budget Événements BDE 2026 (1 750 000 FCFA)', '192.168.1.10'),
('2026-11-24 10:15:00', 2, 'Soumission dépense',  'Validations',  'demandes_validation', 'Demande vidéoprojecteur 4K (480 000 FCFA)', '192.168.1.12'),
('2026-11-25 14:40:00', 2, 'Soumission dépense',  'Validations',  'demandes_validation', 'Demande badges & affiches Gala (195 000 FCFA)', '192.168.1.12');
