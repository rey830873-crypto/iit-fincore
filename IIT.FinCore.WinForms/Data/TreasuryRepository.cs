using System.Data;
using Dapper;
using IIT.FinCore.WinForms.Models;

namespace IIT.FinCore.WinForms.Data;

public class TreasuryRepository
{
    private readonly AuthRepository _audit = new();

    // ==================== TABLEAU DE BORD ====================

    public DashboardKpi GetDashboardKpis(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();

        var soldeGlobal = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(solde_actuel), 0)
            FROM comptes_tresorerie
            WHERE est_actif = 1 AND (@Sid IS NULL OR structure_id = @Sid);",
            new { Sid = structureId });

        var recettes = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(montant), 0)
            FROM transactions
            WHERE type_transaction = 'RECETTE' AND statut = 'VALIDEE'
              AND (@Sid IS NULL OR structure_id = @Sid);",
            new { Sid = structureId });

        var depenses = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(montant), 0)
            FROM transactions
            WHERE type_transaction = 'DEPENSE' AND statut = 'VALIDEE'
              AND (@Sid IS NULL OR structure_id = @Sid);",
            new { Sid = structureId });

        var structuresActives = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM structures WHERE est_actif = 1;");

        return new DashboardKpi
        {
            SoldeGlobal = soldeGlobal,
            TotalRecettes = recettes,
            TotalDepenses = depenses,
            StructuresActives = structuresActives
        };
    }

    // ==================== STRUCTURES ====================

    public List<Structure> GetStructures()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<Structure>(@"
            SELECT s.id AS Id, s.code AS Code, s.nom AS Nom,
                   s.type_structure AS TypeStructure,
                   s.parent_id AS ParentId, p.nom AS ParentNom,
                   s.email_contact AS EmailContact, s.telephone AS Telephone,
                   s.est_actif AS EstActif
            FROM structures s
            LEFT JOIN structures p ON p.id = s.parent_id
            ORDER BY s.id;").ToList();
    }

    public void CreateStructure(Structure s, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO structures (code, nom, type_structure, parent_id, email_contact, telephone)
            VALUES (@Code, @Nom, @TypeStructure, @ParentId, @EmailContact, @Telephone);
            SELECT LAST_INSERT_ID();", s);
        _audit.LogActivity(userId, "Création structure", "Structures", "structures", id, $"{s.Code} - {s.Nom}");
    }

    // ==================== COMPTES DE TRESORERIE ====================

    public List<CompteTresorerie> GetComptes(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<CompteTresorerie>(@"
            SELECT c.id AS Id, c.nom_compte AS NomCompte, c.structure_id AS StructureId,
                   s.nom AS StructureNom, c.type_compte AS TypeCompte,
                   c.etablissement AS Etablissement, c.numero_reference AS NumeroReference,
                   c.solde_initial AS SoldeInitial, c.solde_actuel AS SoldeActuel,
                   c.devise AS Devise, c.est_actif AS EstActif
            FROM comptes_tresorerie c
            JOIN structures s ON s.id = c.structure_id
            WHERE (@Sid IS NULL OR c.structure_id = @Sid)
            ORDER BY c.id;", new { Sid = structureId }).ToList();
    }

    public void CreateCompte(CompteTresorerie c, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO comptes_tresorerie
                (nom_compte, structure_id, type_compte, etablissement, numero_reference, solde_initial, devise)
            VALUES
                (@NomCompte, @StructureId, @TypeCompte, @Etablissement, @NumeroReference, @SoldeInitial, @Devise);
            SELECT LAST_INSERT_ID();", c);
        _audit.LogActivity(userId, "Création compte", "Comptes", "comptes_tresorerie", id,
            $"{c.NomCompte} ({c.TypeCompte}) - Initial: {c.SoldeInitial:N0} {c.Devise}");
    }

    // ==================== CATEGORIES & CENTRES ANALYTIQUES ====================

    public List<CategorieMouvement> GetCategories(string? nature = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<CategorieMouvement>(@"
            SELECT id AS Id, libelle AS Libelle, nature AS Nature
            FROM categories_mouvement
            WHERE est_actif = 1 AND (@Nature IS NULL OR nature = @Nature)
            ORDER BY libelle;", new { Nature = nature }).ToList();
    }

    public List<CentreAnalytique> GetCentresAnalytiques()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<CentreAnalytique>(@"
            SELECT ca.id AS Id, ca.code AS Code, ca.libelle AS Libelle,
                   ca.description AS Description, ca.structure_id AS StructureId,
                   s.nom AS StructureNom, ca.est_actif AS EstActif
            FROM centres_analytiques ca
            LEFT JOIN structures s ON s.id = ca.structure_id
            ORDER BY ca.code;").ToList();
    }

    public void CreateCentreAnalytique(CentreAnalytique ca, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO centres_analytiques (code, libelle, description, structure_id)
            VALUES (@Code, @Libelle, @Description, @StructureId);
            SELECT LAST_INSERT_ID();", ca);
        _audit.LogActivity(userId, "Création centre analytique", "Analytique", "centres_analytiques", id, $"{ca.Code} - {ca.Libelle}");
    }

    public List<SyntheseAnalytiqueRow> GetSyntheseAnalytique()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<SyntheseAnalytiqueRow>(@"
            SELECT COALESCE(structure_nom, 'Transversal') AS StructureNom,
                   centre_code AS CentreCode,
                   centre_libelle AS CentreLibelle,
                   total_recettes AS TotalRecettes,
                   total_depenses AS TotalDepenses,
                   resultat_net AS ResultatNet
            FROM v_synthese_analytique
            ORDER BY centre_code;").ToList();
    }

    // ==================== TRANSACTIONS & MULTI-VENTILATION ====================

    public List<TransactionItem> GetTransactions(string? typeFilter = null, int? structureId = null, int limit = 200)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<TransactionItem>(@"
            SELECT t.id AS Id,
                   t.reference AS Reference,
                   t.date_operation AS DateOperation,
                   t.libelle AS Libelle,
                   t.type_transaction AS TypeTransaction,
                   t.montant AS Montant,
                   t.mode_paiement AS ModePaiement,
                   t.justificatif_path AS JustificatifPath,
                   t.statut AS Statut,
                   t.compte_id AS CompteId,
                   c.nom_compte AS CompteNom,
                   t.categorie_id AS CategorieId,
                   cat.libelle AS CategorieLibelle,
                   t.structure_id AS StructureId,
                   s.nom AS StructureNom,
                   t.cree_par AS CreePar,
                   u.nom_complet AS CreeParNom,
                   t.observations AS Observations,
                   t.est_rapprochee AS EstRapprochee,
                   t.date_rapprochement AS DateRapprochement,
                   (SELECT GROUP_CONCAT(CONCAT(ca.libelle, ' (', ROUND(ia.pourcentage, 0), '%)') SEPARATOR ', ')
                    FROM imputations_analytiques ia
                    JOIN centres_analytiques ca ON ca.id = ia.centre_analytique_id
                    WHERE ia.transaction_id = t.id) AS CentresResume
            FROM transactions t
            JOIN comptes_tresorerie c ON c.id = t.compte_id
            JOIN structures s ON s.id = t.structure_id
            JOIN utilisateurs u ON u.id = t.cree_par
            LEFT JOIN categories_mouvement cat ON cat.id = t.categorie_id
            WHERE (@TypeFilter IS NULL OR t.type_transaction = @TypeFilter)
              AND (@Sid IS NULL OR t.structure_id = @Sid)
            ORDER BY t.date_operation DESC, t.id DESC
            LIMIT @Limit;",
            new { TypeFilter = typeFilter, Sid = structureId, Limit = limit }).ToList();
    }

    public decimal GetSeuilValidationDepense()
    {
        using var conn = DatabaseConfig.CreateConnection();
        var val = conn.ExecuteScalar<string?>(
            "SELECT valeur_param FROM parametres_generaux WHERE cle_param = 'SEUIL_VALIDATION_DEP';");
        return decimal.TryParse(val, out var seuil) ? seuil : 100000m;
    }

    /// <summary>
    /// Crée une Recette ou une Dépense avec multi-ventilation analytique (somme = 100 % du montant).
    /// Si Dépense >= seuil et non-Admin, crée automatiquement une demande de validation (statut EN_ATTENTE).
    /// </summary>
    public int CreateTransactionWithVentilation(
        TransactionItem trx,
        List<ImputationAnalytique> imputations,
        bool isAdmin,
        string? motifDemande = null)
    {
        if (imputations.Count == 0)
            throw new InvalidOperationException("Veuillez affecter au moins un centre analytique.");

        var totalImpute = imputations.Sum(i => i.MontantImpute);
        if (Math.Abs(totalImpute - trx.Montant) > 0.01m)
            throw new InvalidOperationException(
                $"Ventilation déséquilibrée : total ventilé ({totalImpute:N0} FCFA) ≠ montant ({trx.Montant:N0} FCFA).");

        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        using var dbTrx = conn.BeginTransaction();

        try
        {
            var seuil = GetSeuilValidationDepense();
            bool requiresValidation = trx.TypeTransaction == "DEPENSE" && !isAdmin && trx.Montant >= seuil;
            trx.Statut = requiresValidation ? "EN_ATTENTE" : "VALIDEE";
            trx.Reference = $"TRX-{trx.DateOperation:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

            var trxId = conn.ExecuteScalar<int>(@"
                INSERT INTO transactions (
                    reference, date_operation, libelle, type_transaction, montant, mode_paiement,
                    justificatif_path, statut, compte_id, categorie_id, structure_id,
                    cree_par, valide_par, date_validation, observations
                ) VALUES (
                    @Reference, @DateOperation, @Libelle, @TypeTransaction, @Montant, @ModePaiement,
                    @JustificatifPath, @Statut, @CompteId, @CategorieId, @StructureId,
                    @CreePar, @ValidePar, @DateValidation, @Observations
                );
                SELECT LAST_INSERT_ID();",
                new
                {
                    trx.Reference,
                    trx.DateOperation,
                    trx.Libelle,
                    trx.TypeTransaction,
                    trx.Montant,
                    trx.ModePaiement,
                    trx.JustificatifPath,
                    trx.Statut,
                    trx.CompteId,
                    trx.CategorieId,
                    trx.StructureId,
                    trx.CreePar,
                    ValidePar = requiresValidation ? (int?)null : trx.CreePar,
                    DateValidation = requiresValidation ? (DateTime?)null : DateTime.Now,
                    trx.Observations
                }, dbTrx);

            foreach (var imp in imputations)
            {
                var pct = Math.Round((imp.MontantImpute / trx.Montant) * 100m, 2);
                conn.Execute(@"
                    INSERT INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
                    VALUES (@TrxId, @CentreId, @Montant, @Pct);",
                    new { TrxId = trxId, CentreId = imp.CentreAnalytiqueId, Montant = imp.MontantImpute, Pct = pct },
                    dbTrx);
            }

            if (requiresValidation)
            {
                conn.Execute(@"
                    INSERT INTO demandes_validation (transaction_id, demandeur_id, motif_demande, statut)
                    VALUES (@TrxId, @DemandeurId, @Motif, 'EN_ATTENTE');",
                    new
                    {
                        TrxId = trxId,
                        DemandeurId = trx.CreePar,
                        Motif = string.IsNullOrWhiteSpace(motifDemande) ? trx.Libelle : motifDemande
                    }, dbTrx);
            }

            dbTrx.Commit();

            _audit.LogActivity(trx.CreePar,
                trx.TypeTransaction == "RECETTE" ? "Création recette" : "Création dépense",
                "Transactions", "transactions", trxId,
                $"{trx.Reference} | {trx.Libelle} | {trx.Montant:N0} FCFA | {imputations.Count} centre(s) | Statut: {trx.Statut}");

            return trxId;
        }
        catch
        {
            dbTrx.Rollback();
            throw;
        }
    }

    public void ExecuteTransfert(int compteSourceId, int compteDestId, decimal montant, DateTime dateOp, string motif, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        conn.Execute("sp_effectuer_transfert_comptes",
            new
            {
                p_compte_source_id = compteSourceId,
                p_compte_dest_id = compteDestId,
                p_montant = montant,
                p_date_op = dateOp.Date,
                p_motif = motif,
                p_utilisateur_id = userId
            },
            commandType: CommandType.StoredProcedure);
    }

    // ==================== VALIDATIONS ====================

    public List<DemandeValidationItem> GetDemandesValidation(bool onlyPending = false)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<DemandeValidationItem>(@"
            SELECT dv.id AS Id, dv.transaction_id AS TransactionId,
                   t.reference AS Reference, t.libelle AS Libelle, t.montant AS Montant,
                   s.nom AS StructureNom, c.nom_compte AS CompteNom,
                   u.nom_complet AS DemandeurNom,
                   dv.motif_demande AS MotifDemande,
                   dv.commentaire_decision AS CommentaireDecision,
                   dv.statut AS Statut,
                   dv.date_demande AS DateDemande,
                   dv.date_decision AS DateDecision
            FROM demandes_validation dv
            JOIN transactions t ON t.id = dv.transaction_id
            JOIN structures s ON s.id = t.structure_id
            JOIN comptes_tresorerie c ON c.id = t.compte_id
            JOIN utilisateurs u ON u.id = dv.demandeur_id
            WHERE (@OnlyPending = 0 OR dv.statut = 'EN_ATTENTE')
            ORDER BY dv.date_demande DESC;", new { OnlyPending = onlyPending ? 1 : 0 }).ToList();
    }

    public void DecideValidation(int demandeId, int transactionId, bool approve, int validateurId, string commentaire)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        using var dbTrx = conn.BeginTransaction();
        try
        {
            conn.Execute(@"
                UPDATE demandes_validation
                SET statut = @Statut, validateur_id = @ValId, commentaire_decision = @Comm, date_decision = NOW()
                WHERE id = @Id;",
                new
                {
                    Statut = approve ? "APPROUVEE" : "REJETEE",
                    ValId = validateurId,
                    Comm = commentaire,
                    Id = demandeId
                }, dbTrx);

            conn.Execute(@"
                UPDATE transactions
                SET statut = @TrxStatut, valide_par = @ValId, date_validation = NOW()
                WHERE id = @TrxId;",
                new
                {
                    TrxStatut = approve ? "VALIDEE" : "REJETEE",
                    ValId = validateurId,
                    TrxId = transactionId
                }, dbTrx);

            dbTrx.Commit();
            _audit.LogActivity(validateurId, approve ? "Validation dépense" : "Rejet dépense",
                "Validations", "demandes_validation", demandeId, commentaire);
        }
        catch
        {
            dbTrx.Rollback();
            throw;
        }
    }

    // ==================== BUDGETS ====================

    public List<SuiviBudgetaireRow> GetSuiviBudgetaire()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<SuiviBudgetaireRow>(@"
            SELECT budget_id AS BudgetId, intitule AS Intitule, structure_nom AS StructureNom,
                   centre_analytique AS CentreAnalytique, date_debut AS DateDebut, date_fin AS DateFin,
                   montant_prevu AS MontantPrevu, montant_realise AS MontantRealise,
                   reste_a_consommer AS ResteAConsommer, taux_execution_pct AS TauxExecutionPct,
                   statut AS Statut
            FROM v_suivi_budgetaire
            ORDER BY date_debut DESC;").ToList();
    }

    public void CreateBudget(string intitule, int structureId, int centreId, decimal montantPrevu, DateTime debut, DateTime fin, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO budgets (intitule, structure_id, centre_analytique_id, montant_prevu, date_debut, date_fin)
            VALUES (@Intitule, @Sid, @Cid, @Montant, @Debut, @Fin);
            SELECT LAST_INSERT_ID();",
            new { Intitule = intitule, Sid = structureId, Cid = centreId, Montant = montantPrevu, Debut = debut.Date, Fin = fin.Date });
        _audit.LogActivity(userId, "Création budget", "Budgets", "budgets", id, $"{intitule} ({montantPrevu:N0} FCFA)");
    }

    // ==================== COTISATIONS ====================

    public List<CotisationItem> GetCotisations()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<CotisationItem>(@"
            SELECT co.id AS Id, co.nom AS Nom, co.structure_id AS StructureId,
                   s.nom AS StructureNom, co.montant_par_membre AS MontantParMembre,
                   co.date_limite AS DateLimite, co.population_concernee AS PopulationConcernee,
                   co.statut AS Statut
            FROM cotisations co
            JOIN structures s ON s.id = co.structure_id
            ORDER BY co.date_limite DESC;").ToList();
    }

    public void CreateCotisation(CotisationItem c, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO cotisations (nom, structure_id, montant_par_membre, date_limite, population_concernee, statut)
            VALUES (@Nom, @StructureId, @MontantParMembre, @DateLimite, @PopulationConcernee, @Statut);
            SELECT LAST_INSERT_ID();", c);
        _audit.LogActivity(userId, "Création cotisation", "Cotisations", "cotisations", id, $"{c.Nom} ({c.MontantParMembre:N0} FCFA)");
    }

    // ==================== UTILISATEURS, ROLES & AUDIT ====================

    public List<Utilisateur> GetUtilisateurs()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<Utilisateur>(@"
            SELECT u.id AS Id, u.nom_complet AS NomComplet, u.email AS Email,
                   u.telephone AS Telephone, u.role_id AS RoleId,
                   r.code AS RoleCode, r.nom AS RoleNom,
                   u.structure_id AS StructureId, s.nom AS StructureNom,
                   u.est_actif AS EstActif, u.dernier_acces AS DernierAcces
            FROM utilisateurs u
            JOIN roles r ON r.id = u.role_id
            LEFT JOIN structures s ON s.id = u.structure_id
            ORDER BY u.id;").ToList();
    }

    public List<Role> GetRoles()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<Role>(@"
            SELECT r.id AS Id, r.code AS Code, r.nom AS Nom, r.description AS Description,
                   r.est_actif AS EstActif,
                   (SELECT COUNT(*) FROM utilisateurs u WHERE u.role_id = r.id) AS NbUtilisateurs
            FROM roles r ORDER BY r.id;").ToList();
    }

    public void CreateUtilisateur(string nom, string email, string passwordPlain, string? telephone, int roleId, int? structureId, int adminId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var hash = BCrypt.Net.BCrypt.HashPassword(passwordPlain);
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO utilisateurs (nom_complet, email, mot_de_passe_hash, telephone, role_id, structure_id)
            VALUES (@Nom, @Email, @Hash, @Tel, @RoleId, @Sid);
            SELECT LAST_INSERT_ID();",
            new { Nom = nom, Email = email.Trim(), Hash = hash, Tel = telephone, RoleId = roleId, Sid = structureId });
        _audit.LogActivity(adminId, "Création utilisateur", "Utilisateurs", "utilisateurs", id, $"{nom} ({email})");
    }

    public List<JournalActiviteItem> GetJournalActivites(int limit = 150)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<JournalActiviteItem>(@"
            SELECT j.id AS Id, j.date_action AS DateAction,
                   COALESCE(u.nom_complet, 'Système') AS UtilisateurNom,
                   j.action AS Action, j.module AS Module,
                   j.details AS Details, j.adresse_ip AS AdresseIp
            FROM journal_activites j
            LEFT JOIN utilisateurs u ON u.id = j.utilisateur_id
            ORDER BY j.date_action DESC
            LIMIT @Limit;", new { Limit = limit }).ToList();
    }

    // ==================== EVOLUTION MENSUELLE (DASHBOARD) ====================

    public List<MonthlyEvolutionPoint> GetMonthlyEvolution(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<MonthlyEvolutionPoint>(@"
            SELECT DATE_FORMAT(date_operation, '%Y-%m') AS Mois,
                   COALESCE(SUM(CASE WHEN type_transaction = 'RECETTE' THEN montant ELSE 0 END), 0) AS Recettes,
                   COALESCE(SUM(CASE WHEN type_transaction = 'DEPENSE' THEN montant ELSE 0 END), 0) AS Depenses
            FROM transactions
            WHERE statut = 'VALIDEE' AND (@Sid IS NULL OR structure_id = @Sid)
            GROUP BY DATE_FORMAT(date_operation, '%Y-%m')
            ORDER BY Mois ASC
            LIMIT 12;", new { Sid = structureId }).ToList();
    }

    // ==================== EDITION / ARCHIVAGE STRUCTURES & COMPTES ====================

    public void UpdateStructure(Structure s, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute(@"
            UPDATE structures
            SET nom = @Nom, type_structure = @TypeStructure, email_contact = @EmailContact,
                telephone = @Telephone, est_actif = @EstActif
            WHERE id = @Id;", s);
        _audit.LogActivity(userId, "Modification structure", "Structures", "structures", s.Id, $"{s.Code} - {s.Nom}");
    }

    public void UpdateCompte(CompteTresorerie c, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute(@"
            UPDATE comptes_tresorerie
            SET nom_compte = @NomCompte, type_compte = @TypeCompte, etablissement = @Etablissement,
                numero_reference = @NumeroReference, est_actif = @EstActif
            WHERE id = @Id;", c);
        _audit.LogActivity(userId, "Modification compte", "Comptes", "comptes_tresorerie", c.Id, c.NomCompte);
    }

    public List<ImputationAnalytique> GetImputationsByTransaction(int transactionId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<ImputationAnalytique>(@"
            SELECT ia.id AS Id, ia.transaction_id AS TransactionId,
                   ia.centre_analytique_id AS CentreAnalytiqueId,
                   CONCAT(ca.code, ' - ', ca.libelle) AS CentreLibelle,
                   ia.montant_impute AS MontantImpute,
                   ia.pourcentage AS Pourcentage
            FROM imputations_analytiques ia
            JOIN centres_analytiques ca ON ca.id = ia.centre_analytique_id
            WHERE ia.transaction_id = @TrxId;", new { TrxId = transactionId }).ToList();
    }

    // ==================== MEMBRES, PAIEMENTS & IMPAYES COTISATIONS ====================

    public List<MembreItem> GetMembres(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<MembreItem>(@"
            SELECT m.id AS Id, m.matricule AS Matricule, m.nom_complet AS NomComplet,
                   m.email AS Email, m.telephone AS Telephone, m.structure_id AS StructureId,
                   s.nom AS StructureNom, m.est_actif AS EstActif
            FROM membres m
            JOIN structures s ON s.id = m.structure_id
            WHERE (@Sid IS NULL OR m.structure_id = @Sid)
            ORDER BY m.nom_complet;", new { Sid = structureId }).ToList();
    }

    public void CreateMembre(MembreItem m, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO membres (matricule, nom_complet, email, telephone, structure_id)
            VALUES (@Matricule, @NomComplet, @Email, @Telephone, @StructureId);
            SELECT LAST_INSERT_ID();", m);
        _audit.LogActivity(userId, "Ajout membre", "Cotisations", "membres", id, m.NomComplet);
    }

    public List<PaiementCotisationItem> GetPaiementsCotisations(bool onlyUnpaid = false)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<PaiementCotisationItem>(@"
            SELECT pc.id AS Id, pc.cotisation_id AS CotisationId, co.nom AS CotisationNom,
                   pc.membre_id AS MembreId, m.nom_complet AS MembreNom,
                   s.nom AS StructureNom, pc.montant_du AS MontantDu, pc.montant_paye AS MontantPaye,
                   pc.date_echeance AS DateEcheance, pc.date_paiement AS DatePaiement,
                   pc.statut AS Statut, pc.transaction_id AS TransactionId
            FROM paiements_cotisations pc
            JOIN cotisations co ON co.id = pc.cotisation_id
            JOIN membres m ON m.id = pc.membre_id
            JOIN structures s ON s.id = co.structure_id
            WHERE (@OnlyUnpaid = 0 OR pc.statut IN ('IMPAYE', 'EN_ATTENTE', 'PARTIEL'))
            ORDER BY pc.date_echeance DESC, pc.id DESC;", new { OnlyUnpaid = onlyUnpaid ? 1 : 0 }).ToList();
    }

    public void EnregistrerPaiementCotisation(int cotisationId, int membreId, int compteId, decimal montantPaye, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        using var dbTrx = conn.BeginTransaction();
        try
        {
            var cotis = conn.QueryFirst<CotisationItem>(
                "SELECT id AS Id, nom AS Nom, structure_id AS StructureId, montant_par_membre AS MontantParMembre, date_limite AS DateLimite FROM cotisations WHERE id = @Id;",
                new { Id = cotisationId }, dbTrx);
            var membreNom = conn.ExecuteScalar<string>(
                "SELECT nom_complet FROM membres WHERE id = @Id;", new { Id = membreId }, dbTrx);
            var catId = conn.ExecuteScalar<int?>(
                "SELECT id FROM categories_mouvement WHERE nature = 'RECETTE' AND libelle LIKE 'Cotisation%' LIMIT 1;", transaction: dbTrx);
            var centreId = conn.ExecuteScalar<int?>(
                "SELECT id FROM centres_analytiques ORDER BY id LIMIT 1;", transaction: dbTrx);

            var statutPaiement = montantPaye >= cotis.MontantParMembre ? "PAYE" : (montantPaye > 0 ? "PARTIEL" : "IMPAYE");
            int? trxId = null;

            if (montantPaye > 0)
            {
                var reference = $"COT-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                trxId = conn.ExecuteScalar<int>(@"
                    INSERT INTO transactions (
                        reference, date_operation, libelle, type_transaction, montant, mode_paiement,
                        statut, compte_id, categorie_id, structure_id, cree_par, valide_par, date_validation
                    ) VALUES (
                        @Ref, CURDATE(), @Lib, 'RECETTE', @Montant, 'ESPECES',
                        'VALIDEE', @CompteId, @CatId, @Sid, @Uid, @Uid, NOW()
                    );
                    SELECT LAST_INSERT_ID();",
                    new { Ref = reference, Lib = $"Cotisation {cotis.Nom} - {membreNom}", Montant = montantPaye, CompteId = compteId, CatId = catId, Sid = cotis.StructureId, Uid = userId },
                    dbTrx);

                if (centreId.HasValue)
                {
                    conn.Execute(@"
                        INSERT INTO imputations_analytiques (transaction_id, centre_analytique_id, montant_impute, pourcentage)
                        VALUES (@TrxId, @Cid, @Montant, 100.00);",
                        new { TrxId = trxId.Value, Cid = centreId.Value, Montant = montantPaye }, dbTrx);
                }
            }

            conn.Execute(@"
                INSERT INTO paiements_cotisations (cotisation_id, membre_id, montant_du, montant_paye, date_echeance, date_paiement, statut, transaction_id)
                VALUES (@CotId, @MemId, @Du, @Paye, @Ech, @DateP, @Statut, @TrxId)
                ON DUPLICATE KEY UPDATE
                    montant_paye = VALUES(montant_paye),
                    date_paiement = VALUES(date_paiement),
                    statut = VALUES(statut),
                    transaction_id = COALESCE(VALUES(transaction_id), transaction_id);",
                new
                {
                    CotId = cotisationId,
                    MemId = membreId,
                    Du = cotis.MontantParMembre,
                    Paye = montantPaye,
                    Ech = cotis.DateLimite,
                    DateP = montantPaye > 0 ? (DateTime?)DateTime.Today : null,
                    Statut = statutPaiement,
                    TrxId = trxId
                }, dbTrx);

            dbTrx.Commit();
            _audit.LogActivity(userId, "Paiement cotisation", "Cotisations", "paiements_cotisations", cotisationId,
                $"{membreNom} : {montantPaye:N0} / {cotis.MontantParMembre:N0} FCFA ({statutPaiement})");
        }
        catch
        {
            dbTrx.Rollback();
            throw;
        }
    }

    // ==================== MATRICE PERMISSIONS & PARAMETRES ====================

    public List<PermissionItem> GetPermissions()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<PermissionItem>(
            "SELECT id AS Id, code AS Code, libelle AS Libelle, module AS Module FROM permissions ORDER BY module, id;").ToList();
    }

    public HashSet<(int RoleId, int PermId)> GetRolePermissionPairs()
    {
        using var conn = DatabaseConfig.CreateConnection();
        var pairs = conn.Query<(int RoleId, int PermId)>(
            "SELECT role_id AS RoleId, permission_id AS PermId FROM role_permissions;");
        return new HashSet<(int, int)>(pairs);
    }

    public void SetRolePermission(int roleId, int permissionId, bool granted, int adminId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        if (granted)
        {
            conn.Execute("INSERT IGNORE INTO role_permissions (role_id, permission_id) VALUES (@R, @P);",
                new { R = roleId, P = permissionId });
        }
        else
        {
            conn.Execute("DELETE FROM role_permissions WHERE role_id = @R AND permission_id = @P;",
                new { R = roleId, P = permissionId });
        }
        _audit.LogActivity(adminId, "Modification permission RBAC", "Utilisateurs", "role_permissions", roleId,
            $"Role #{roleId} | Perm #{permissionId} => {(granted ? "Accordée" : "Retirée")}");
    }

    public void ToggleUtilisateurActif(int userId, bool estActif, int adminId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("UPDATE utilisateurs SET est_actif = @Actif WHERE id = @Id;", new { Actif = estActif ? 1 : 0, Id = userId });
        _audit.LogActivity(adminId, estActif ? "Activation utilisateur" : "Désactivation utilisateur", "Utilisateurs", "utilisateurs", userId, null);
    }

    public List<ParametreGeneral> GetParametres()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<ParametreGeneral>(
            "SELECT cle_param AS CleParam, valeur_param AS ValeurParam, description AS Description FROM parametres_generaux ORDER BY cle_param;").ToList();
    }

    public void SaveParametre(string cle, string valeur, int adminId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("UPDATE parametres_generaux SET valeur_param = @Val WHERE cle_param = @Cle;", new { Val = valeur, Cle = cle });
        _audit.LogActivity(adminId, "Modification paramètre", "Paramètres", "parametres_generaux", null, $"{cle} = {valeur}");
    }

    // ==================== #1 IMPORT CSV MEMBRES ====================

    public int ImportMembresFromCsv(string csvPath, int structureId, int userId)
    {
        var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0) return 0;

        int imported = 0;
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        using var trx = conn.BeginTransaction();
        try
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var sep = lines[i].Contains(';') ? ';' : ',';
                var cols = lines[i].Split(sep).Select(c => c.Trim().Trim('"')).ToArray();
                if (cols.Length < 1) continue;

                // Ignore la ligne d'en-tête si présente
                if (i == 0 && (cols[0].Contains("nom", StringComparison.OrdinalIgnoreCase) || cols[0].Contains("matricule", StringComparison.OrdinalIgnoreCase)))
                    continue;

                string nom = cols.Length >= 2 && cols[0].StartsWith("IIT", StringComparison.OrdinalIgnoreCase) ? cols[1] : cols[0];
                string matricule = cols.Length >= 2 && cols[0].StartsWith("IIT", StringComparison.OrdinalIgnoreCase)
                    ? cols[0]
                    : $"IMP-{DateTime.Now:yyMM}-{Random.Shared.Next(1000, 9999)}";
                string? email = cols.Length >= 3 ? cols[2] : null;
                string? tel = cols.Length >= 4 ? cols[3] : (cols.Length == 2 ? cols[1] : null);

                if (string.IsNullOrWhiteSpace(nom)) continue;

                conn.Execute(@"
                    INSERT IGNORE INTO membres (matricule, nom_complet, email, telephone, structure_id)
                    VALUES (@Mat, @Nom, @Email, @Tel, @Sid);",
                    new { Mat = matricule, Nom = nom, Email = email, Tel = tel, Sid = structureId }, trx);
                imported++;
            }
            trx.Commit();
            _audit.LogActivity(userId, "Import CSV membres", "Cotisations", "membres", null, $"{imported} membre(s) importé(s) depuis {Path.GetFileName(csvPath)}");
            return imported;
        }
        catch
        {
            trx.Rollback();
            throw;
        }
    }

    // ==================== #3 RAPPROCHEMENT BANCAIRE & MOBILE MONEY ====================

    public void ToggleRapprochement(int transactionId, bool rapprochee, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute(@"
            UPDATE transactions
            SET est_rapprochee = @Rap,
                date_rapprochement = IF(@Rap = 1, NOW(), NULL)
            WHERE id = @Id;",
            new { Rap = rapprochee ? 1 : 0, Id = transactionId });

        _audit.LogActivity(userId, rapprochee ? "Rapprochement bancaire" : "Annulation rapprochement",
            "Transactions", "transactions", transactionId, $"Transaction #{transactionId} => {(rapprochee ? "Pointée" : "Non pointée")}");
    }

    public (decimal SoldeComptable, decimal SoldeRapproche, decimal Ecart) GetRapprochementSummary(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var soldeInitial = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(solde_initial), 0)
            FROM comptes_tresorerie
            WHERE est_actif = 1 AND (@Sid IS NULL OR structure_id = @Sid);", new { Sid = structureId });

        var soldeComptable = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(solde_actuel), 0)
            FROM comptes_tresorerie
            WHERE est_actif = 1 AND (@Sid IS NULL OR structure_id = @Sid);", new { Sid = structureId });

        var mouvementsRapproches = conn.ExecuteScalar<decimal>(@"
            SELECT COALESCE(SUM(CASE WHEN type_transaction IN ('RECETTE', 'TRANSFERT_ENTRANT') THEN montant ELSE -montant END), 0)
            FROM transactions
            WHERE statut = 'VALIDEE' AND est_rapprochee = 1 AND (@Sid IS NULL OR structure_id = @Sid);", new { Sid = structureId });

        var soldeRapproche = soldeInitial + mouvementsRapproches;
        return (soldeComptable, soldeRapproche, soldeComptable - soldeRapproche);
    }

    // ==================== #4 CLOTURE D'EXERCICE COMPTABLE ====================

    public List<ExerciceComptable> GetExercices()
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<ExerciceComptable>(@"
            SELECT id AS Id, annee AS Annee, libelle AS Libelle, date_debut AS DateDebut,
                   date_fin AS DateFin, statut AS Statut, solde_cloture AS SoldeCloture,
                   date_cloture AS DateCloture
            FROM exercices_comptables
            ORDER BY annee DESC;").ToList();
    }

    public void CloturerExercice(int exerciceId, int adminId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Open();
        using var trx = conn.BeginTransaction();
        try
        {
            var ex = conn.QueryFirst<ExerciceComptable>(
                "SELECT id AS Id, annee AS Annee, libelle AS Libelle, statut AS Statut FROM exercices_comptables WHERE id = @Id;",
                new { Id = exerciceId }, trx);

            if (ex.Statut == "CLOTURE")
                throw new InvalidOperationException("Cet exercice est déjà clôturé.");

            var soldeGlobal = conn.ExecuteScalar<decimal>(
                "SELECT COALESCE(SUM(solde_actuel), 0) FROM comptes_tresorerie WHERE est_actif = 1;", transaction: trx);

            conn.Execute(@"
                UPDATE exercices_comptables
                SET statut = 'CLOTURE', solde_cloture = @Solde, cloture_par = @Uid, date_cloture = NOW()
                WHERE id = @Id;",
                new { Solde = soldeGlobal, Uid = adminId, Id = exerciceId }, trx);

            // Ouvre automatiquement l'exercice suivant (N+1)
            int nextYear = ex.Annee + 1;
            conn.Execute(@"
                INSERT IGNORE INTO exercices_comptables (annee, libelle, date_debut, date_fin, statut)
                VALUES (@Annee, @Lib, @Debut, @Fin, 'OUVERT');",
                new { Annee = nextYear, Lib = $"Exercice {nextYear}", Debut = new DateTime(nextYear, 1, 1), Fin = new DateTime(nextYear, 12, 31) }, trx);

            trx.Commit();
            _audit.LogActivity(adminId, "Clôture d'exercice", "Exercices", "exercices_comptables", exerciceId,
                $"Clôture {ex.Libelle} (Solde: {soldeGlobal:N0} FCFA) + Ouverture Exercice {nextYear}");
        }
        catch
        {
            trx.Rollback();
            throw;
        }
    }

    // ==================== #5 OPERATIONS RECURRENTES & PREVISION CASH-FLOW ====================

    public List<OperationRecurrente> GetOperationsRecurrentes(int? structureId = null)
    {
        using var conn = DatabaseConfig.CreateConnection();
        return conn.Query<OperationRecurrente>(@"
            SELECT r.id AS Id, r.libelle AS Libelle, r.type_operation AS TypeOperation,
                   r.montant AS Montant, r.frequence AS Frequence, r.prochaine_echeance AS ProchaineEcheance,
                   r.compte_id AS CompteId, c.nom_compte AS CompteNom,
                   r.centre_analytique_id AS CentreAnalytiqueId, ca.libelle AS CentreLibelle,
                   r.structure_id AS StructureId, s.nom AS StructureNom, r.est_actif AS EstActif
            FROM operations_recurrentes r
            JOIN comptes_tresorerie c ON c.id = r.compte_id
            JOIN centres_analytiques ca ON ca.id = r.centre_analytique_id
            JOIN structures s ON s.id = r.structure_id
            WHERE (@Sid IS NULL OR r.structure_id = @Sid)
            ORDER BY r.prochaine_echeance ASC;", new { Sid = structureId }).ToList();
    }

    public void CreateOperationRecurrente(OperationRecurrente op, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var id = conn.ExecuteScalar<int>(@"
            INSERT INTO operations_recurrentes
                (libelle, type_operation, montant, frequence, prochaine_echeance, compte_id, centre_analytique_id, structure_id)
            VALUES
                (@Libelle, @TypeOperation, @Montant, @Frequence, @ProchaineEcheance, @CompteId, @CentreAnalytiqueId, @StructureId);
            SELECT LAST_INSERT_ID();", op);
        _audit.LogActivity(userId, "Ajout opération récurrente", "Prévisions", "operations_recurrentes", id,
            $"{op.Libelle} ({op.Montant:N0} FCFA / {op.Frequence})");
    }

    public List<CashFlowPoint> GetCashFlowForecast(int months = 6, int? structureId = null)
    {
        var kpi = GetDashboardKpis(structureId);
        var recurrents = GetOperationsRecurrentes(structureId).Where(r => r.EstActif).ToList();
        var impayes = GetPaiementsCotisations(onlyUnpaid: true);
        if (structureId.HasValue)
        {
            var structNom = GetStructures().FirstOrDefault(s => s.Id == structureId.Value)?.Nom;
            if (!string.IsNullOrEmpty(structNom))
                impayes = impayes.Where(i => i.StructureNom == structNom).ToList();
        }
        decimal expectedImpayes = impayes.Sum(i => Math.Max(0, i.MontantDu - i.MontantPaye));

        var result = new List<CashFlowPoint>();
        decimal running = kpi.SoldeGlobal;
        var baseDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        for (int m = 1; m <= months; m++)
        {
            var target = baseDate.AddMonths(m);
            decimal entrees = m == 1 ? expectedImpayes : 0m; // Cotisations en retard attendues sur M+1
            decimal sorties = 0m;

            foreach (var r in recurrents)
            {
                bool applies = r.Frequence == "MENSUEL"
                    || (r.Frequence == "TRIMESTRIEL" && m % 3 == 0)
                    || (r.Frequence == "ANNUEL" && target.Month == r.ProchaineEcheance.Month);

                if (applies)
                {
                    if (r.TypeOperation == "RECETTE") entrees += r.Montant;
                    else sorties += r.Montant;
                }
            }

            running += entrees - sorties;
            result.Add(new CashFlowPoint
            {
                MoisLabel = target.ToString("MMM yyyy"),
                EntreesPrevues = entrees,
                SortiesPrevues = sorties,
                SoldeProjete = running
            });
        }
        return result;
    }

    // ==================== CRUD COMPLET : MODIFIER & SUPPRIMER / ARCHIVER ====================

    public string DeleteOrArchiveStructure(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        try
        {
            conn.Execute("DELETE FROM structures WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Suppression structure", "Structures", "structures", id, null);
            return "Structure supprimée définitivement.";
        }
        catch (MySqlConnector.MySqlException)
        {
            conn.Execute("UPDATE structures SET est_actif = 0 WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Archivage structure", "Structures", "structures", id, "Archivée car liée à des comptes/opérations");
            return "Structure archivée (désactivée) car elle possède un historique comptable.";
        }
    }

    public string DeleteOrArchiveCompte(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        try
        {
            conn.Execute("DELETE FROM comptes_tresorerie WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Suppression compte", "Comptes", "comptes_tresorerie", id, null);
            return "Compte supprimé définitivement.";
        }
        catch (MySqlConnector.MySqlException)
        {
            conn.Execute("UPDATE comptes_tresorerie SET est_actif = 0 WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Archivage compte", "Comptes", "comptes_tresorerie", id, "Archivé car lié à des mouvements");
            return "Compte archivé car il contient des mouvements de trésorerie.";
        }
    }

    public string CancelOrDeleteTransaction(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        var statut = conn.ExecuteScalar<string?>("SELECT statut FROM transactions WHERE id = @Id;", new { Id = id });
        if (statut == null) return "Transaction introuvable.";

        if (statut == "VALIDEE")
        {
            // Le trigger trg_trx_after_update recalcule automatiquement le solde du compte !
            conn.Execute("UPDATE transactions SET statut = 'ANNULEE' WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Annulation transaction", "Transactions", "transactions", id, "Transaction annulée et solde rétabli");
            return "Opération annulée ! Le solde du compte a été automatiquement rétabli.";
        }

        conn.Execute("DELETE FROM transactions WHERE id = @Id;", new { Id = id });
        _audit.LogActivity(userId, "Suppression transaction", "Transactions", "transactions", id, $"Suppression transaction ({statut})");
        return "Opération supprimée définitivement.";
    }

    public void UpdateCentreAnalytique(CentreAnalytique ca, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("UPDATE centres_analytiques SET libelle = @Libelle, description = @Description, est_actif = @EstActif WHERE id = @Id;", ca);
        _audit.LogActivity(userId, "Modification centre analytique", "Analytique", "centres_analytiques", ca.Id, ca.Libelle);
    }

    public string DeleteOrArchiveCentre(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        try
        {
            conn.Execute("DELETE FROM centres_analytiques WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Suppression centre analytique", "Analytique", "centres_analytiques", id, null);
            return "Centre analytique supprimé définitivement.";
        }
        catch (MySqlConnector.MySqlException)
        {
            conn.Execute("UPDATE centres_analytiques SET est_actif = 0 WHERE id = @Id;", new { Id = id });
            _audit.LogActivity(userId, "Archivage centre analytique", "Analytique", "centres_analytiques", id, "Désactivé car utilisé");
            return "Centre analytique désactivé car déjà utilisé dans des imputations.";
        }
    }

    public void UpdateBudget(int id, string intitule, decimal montantPrevu, DateTime debut, DateTime fin, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute(@"
            UPDATE budgets SET intitule = @Intitule, montant_prevu = @Montant, date_debut = @Debut, date_fin = @Fin
            WHERE id = @Id;",
            new { Id = id, Intitule = intitule, Montant = montantPrevu, Debut = debut.Date, Fin = fin.Date });
        _audit.LogActivity(userId, "Modification budget", "Budgets", "budgets", id, $"{intitule} ({montantPrevu:N0} FCFA)");
    }

    public void DeleteBudget(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("DELETE FROM budgets WHERE id = @Id;", new { Id = id });
        _audit.LogActivity(userId, "Suppression budget", "Budgets", "budgets", id, null);
    }

    public void DeleteCotisation(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("DELETE FROM cotisations WHERE id = @Id;", new { Id = id });
        _audit.LogActivity(userId, "Suppression cotisation", "Cotisations", "cotisations", id, null);
    }

    public void DeleteOperationRecurrente(int id, int userId)
    {
        using var conn = DatabaseConfig.CreateConnection();
        conn.Execute("DELETE FROM operations_recurrentes WHERE id = @Id;", new { Id = id });
        _audit.LogActivity(userId, "Suppression opération récurrente", "Prévisions", "operations_recurrentes", id, null);
    }
}
