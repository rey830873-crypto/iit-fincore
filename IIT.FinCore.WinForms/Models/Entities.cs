namespace IIT.FinCore.WinForms.Models;

public class Utilisateur
{
    public int Id { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleNom { get; set; } = string.Empty;
    public int? StructureId { get; set; }
    public string? StructureNom { get; set; }
    public bool EstActif { get; set; }
    public DateTime? DernierAcces { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; }
    public int NbUtilisateurs { get; set; }
}

public class Structure
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string TypeStructure { get; set; } = "ASSOCIATION";
    public int? ParentId { get; set; }
    public string? ParentNom { get; set; }
    public string? EmailContact { get; set; }
    public string? Telephone { get; set; }
    public bool EstActif { get; set; } = true;
}

public class CompteTresorerie
{
    public int Id { get; set; }
    public string NomCompte { get; set; } = string.Empty;
    public int StructureId { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public string TypeCompte { get; set; } = "CAISSE";
    public string? Etablissement { get; set; }
    public string? NumeroReference { get; set; }
    public decimal SoldeInitial { get; set; }
    public decimal SoldeActuel { get; set; }
    public string Devise { get; set; } = "FCFA";
    public bool EstActif { get; set; } = true;

    public override string ToString() => $"{NomCompte} ({StructureNom}) — {SoldeActuel:N0} {Devise}";
}

public class CategorieMouvement
{
    public int Id { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string Nature { get; set; } = "RECETTE";
}

public class CentreAnalytique
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? StructureId { get; set; }
    public string? StructureNom { get; set; }
    public bool EstActif { get; set; } = true;

    public override string ToString() => $"{Code} - {Libelle}";
}

public class TransactionItem
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime DateOperation { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string TypeTransaction { get; set; } = "RECETTE";
    public decimal Montant { get; set; }
    public string ModePaiement { get; set; } = "ESPECES";
    public string? JustificatifPath { get; set; }
    public string Statut { get; set; } = "VALIDEE";
    public int CompteId { get; set; }
    public string CompteNom { get; set; } = string.Empty;
    public int? CategorieId { get; set; }
    public string? CategorieLibelle { get; set; }
    public int StructureId { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public int CreePar { get; set; }
    public string? CreeParNom { get; set; }
    public string? CentresResume { get; set; }
    public string? Observations { get; set; }
    public bool EstRapprochee { get; set; }
    public DateTime? DateRapprochement { get; set; }
}

public class ImputationAnalytique
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int CentreAnalytiqueId { get; set; }
    public string CentreLibelle { get; set; } = string.Empty;
    public decimal MontantImpute { get; set; }
    public decimal Pourcentage { get; set; }
}

public class SyntheseAnalytiqueRow
{
    public string StructureNom { get; set; } = string.Empty;
    public string CentreCode { get; set; } = string.Empty;
    public string CentreLibelle { get; set; } = string.Empty;
    public decimal TotalRecettes { get; set; }
    public decimal TotalDepenses { get; set; }
    public decimal ResultatNet { get; set; }
}

public class DemandeValidationItem
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public string CompteNom { get; set; } = string.Empty;
    public string DemandeurNom { get; set; } = string.Empty;
    public string MotifDemande { get; set; } = string.Empty;
    public string? CommentaireDecision { get; set; }
    public string Statut { get; set; } = "EN_ATTENTE";
    public DateTime DateDemande { get; set; }
    public DateTime? DateDecision { get; set; }
}

public class SuiviBudgetaireRow
{
    public int BudgetId { get; set; }
    public string Intitule { get; set; } = string.Empty;
    public string StructureNom { get; set; } = string.Empty;
    public string CentreAnalytique { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal MontantPrevu { get; set; }
    public decimal MontantRealise { get; set; }
    public decimal ResteAConsommer { get; set; }
    public decimal TauxExecutionPct { get; set; }
    public string Statut { get; set; } = "ACTIF";
}

public class CotisationItem
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int StructureId { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public decimal MontantParMembre { get; set; }
    public DateTime DateLimite { get; set; }
    public string PopulationConcernee { get; set; } = "Tous les membres";
    public string Statut { get; set; } = "ACTIVE";
}

public class JournalActiviteItem
{
    public long Id { get; set; }
    public DateTime DateAction { get; set; }
    public string UtilisateurNom { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? AdresseIp { get; set; }
}

public class DashboardKpi
{
    public decimal SoldeGlobal { get; set; }
    public decimal TotalRecettes { get; set; }
    public decimal TotalDepenses { get; set; }
    public int StructuresActives { get; set; }
}

public class MonthlyEvolutionPoint
{
    public string Mois { get; set; } = string.Empty;
    public decimal Recettes { get; set; }
    public decimal Depenses { get; set; }
}

public class MembreItem
{
    public int Id { get; set; }
    public string? Matricule { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public int StructureId { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public bool EstActif { get; set; } = true;
    public override string ToString() => $"{NomComplet} ({StructureNom})";
}

public class PaiementCotisationItem
{
    public int Id { get; set; }
    public int CotisationId { get; set; }
    public string CotisationNom { get; set; } = string.Empty;
    public int MembreId { get; set; }
    public string MembreNom { get; set; } = string.Empty;
    public string StructureNom { get; set; } = string.Empty;
    public decimal MontantDu { get; set; }
    public decimal MontantPaye { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DatePaiement { get; set; }
    public string Statut { get; set; } = "EN_ATTENTE";
    public int? TransactionId { get; set; }
}

public class PermissionItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class ParametreGeneral
{
    public string CleParam { get; set; } = string.Empty;
    public string ValeurParam { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ExerciceComptable
{
    public int Id { get; set; }
    public int Annee { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string Statut { get; set; } = "OUVERT";
    public decimal? SoldeCloture { get; set; }
    public DateTime? DateCloture { get; set; }
}

public class OperationRecurrente
{
    public int Id { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string TypeOperation { get; set; } = "DEPENSE";
    public decimal Montant { get; set; }
    public string Frequence { get; set; } = "MENSUEL";
    public DateTime ProchaineEcheance { get; set; }
    public int CompteId { get; set; }
    public string CompteNom { get; set; } = string.Empty;
    public int CentreAnalytiqueId { get; set; }
    public string CentreLibelle { get; set; } = string.Empty;
    public int StructureId { get; set; }
    public string StructureNom { get; set; } = string.Empty;
    public bool EstActif { get; set; } = true;
}

public class CashFlowPoint
{
    public string MoisLabel { get; set; } = string.Empty;
    public decimal EntreesPrevues { get; set; }
    public decimal SortiesPrevues { get; set; }
    public decimal SoldeProjete { get; set; }
}

