# Dossier d'Analyse et de Conception — IIT FINCORE
**Solution de Gestion d'une Mini-Trésorerie d'Association (C# .NET WinForms & MySQL)**

---

## 1. Synthèse de l'analyse (Cahier des charges + 45 Maquettes HD)

L'application **IIT FINCORE** permet la gestion centralisée et multi-structures de la trésorerie associative (ex. : *IIT*, *BDE*, *IT CLUB*, *Club Sportif*, etc.) en **FCFA**, avec :
- **Gestion des comptes de trésorerie** (Banque, Caisse, Mobile Money) rattachés aux structures.
- **Gestion des mouvements de trésorerie** : Recettes, Dépenses (avec pièces justificatives) et Transferts entre comptes.
- **Gestion des centres d'imputation analytiques** avec **multi-ventilation** (répartition d'un même mouvement sur plusieurs centres analytiques).
- **Modules complémentaires présents dans les maquettes** :
  - Tableaux de bord (général et par structure),
  - Budgets & suivi budgétaire par centre analytique,
  - Circuit de validation / approbation des demandes de dépenses,
  - Campagnes de cotisations, paiements et suivi des impayés,
  - Rapports & exports (synthèse financière, recettes, dépenses),
  - Authentification et contrôle d'accès basé sur **3 rôles** (**Administrateur**, **Trésorier**, **Consultant**) avec matrice de permissions,
  - Journal d'audit des activités et paramètres généraux.

### Cartographie des 45 écrans par module

| Module | Écrans (Maquettes) | Rôles autorisés |
| :--- | :--- | :--- |
| **Authentification** | 01 Splash, 02 Connexion, 03 Mot de passe oublié | Tous |
| **Tableaux de bord** | 04 Général, 05 Par structure | Admin, Trésorier, Consultant |
| **Structures** | 06 Liste, 07 Créer, 08 Modifier, 09 Détails | Admin (CRUD), Trésorier/Consultant (Lecture) |
| **Comptes** | 10 Liste, 11 Créer, 12 Modifier, 13 Détails | Admin, Trésorier (sa structure), Consultant (Lecture) |
| **Transactions** | 14 Mouvements, 15-16 Recettes, 17-18 Dépenses, 19 Détails, 20 Transfert, 21 Historique | Admin, Trésorier (Saisie), Consultant (Lecture) |
| **Budgets** | 22 Liste, 23 Créer, 24 Suivi budgétaire | Admin, Trésorier, Consultant (Lecture) |
| **Validations** | 25 En attente, 26 Détails, 27 Approbation, 28 Historique | Admin (Valider/Rejeter), Trésorier (Soumettre/Voir) |
| **Cotisations** | 29 Liste, 30 Créer, 31 Paiements, 32 Impayés | Admin, Trésorier, Consultant (Lecture) |
| **Rapports** | 33 Synthèse, 34 Recettes, 35 Dépenses, 36 Exportation | Admin, Trésorier, Consultant |
| **Utilisateurs & RBAC** | 37 Liste, 38 Créer, 39 Détails, 40 Rôles, 41 Permissions | Admin uniquement |
| **Audit & Paramètres** | 42-43 Journal activités, 44 Profil, 45 Paramètres généraux | Admin (Audit/Paramètres), Tous (Profil) |

---

## 2. Règles de gestion (RG)

- **RG1 — Unicité et traçabilité** : Chaque transaction validée possède une référence unique générée automatiquement (ex. `TRX-2026-0001`).
- **RG2 — Calcul des soldes** : `solde_actuel = solde_initial + SUM(recettes validées + transferts entrants) - SUM(dépenses validées + transferts sortants)`.
- **RG3 — Multi-ventilation analytique** : Une recette ou dépense peut être ventilée sur $1$ à $N$ centres analytiques via la table `imputations_analytiques`. La somme des `montant_impute` d'une transaction doit être strictement égale au `montant` de la transaction (et la somme des `pourcentage` égale à `100.00 %`).
- **RG4 — Transfert entre comptes** : Un transfert entre deux comptes (source ≠ destination) génère atomiquement deux mouvements liés (`TRANSFERT_SORTANT` et `TRANSFERT_ENTRANT` partageant le même `transfert_groupe_uuid`) sans impacter le bilan global Recettes/Dépenses de l'association.
- **RG5 — Circuit de validation** : Une dépense soumise (ou dépassant un seuil paramétrable) passe au statut `EN_ATTENTE`, crée une entrée dans `demandes_validation`, et n'impacte le solde du compte qu'une fois `APPROUVEE` (`VALIDEE`).
- **RG6 — Sécurité et 3 rôles** :
  - **Administrateur** : accès complet multi-structures, validation des dépenses, gestion utilisateurs/rôles/permissions, audit.
  - **Trésorier** : gestion des comptes, recettes, dépenses, transferts, budgets et cotisations de sa structure.
  - **Consultant** : lecture seule sur les tableaux de bord, listes et exports de rapports.
- **RG7 — Intégrité historique** : Aucun compte, structure ou centre analytique référencé par des mouvements ne peut être supprimé physiquement (désactivation logique `est_actif = 0`).
- **RG8 — Journalisation (Audit)** : Toute action sensible (connexion, création/validation/rejet d'opération, modification de budget ou de rôle) est enregistrée dans `journal_activites`.

---

## 3. Modèle Conceptuel & Logique de Données (MCD / MLD — MySQL)

```mermaid
erDiagram
    ROLES ||--o{ UTILISATEURS : "attribue"
    ROLES ||--o{ ROLE_PERMISSIONS : "possede"
    PERMISSIONS ||--o{ ROLE_PERMISSIONS : "definit"

    STRUCTURES ||--o{ STRUCTURES : "parent"
    STRUCTURES ||--o{ UTILISATEURS : "rattache"
    STRUCTURES ||--o{ COMPTES_TRESORERIE : "detient"
    STRUCTURES ||--o{ CENTRES_ANALYTIQUES : "organise"
    STRUCTURES ||--o{ BUDGETS : "alloue"
    STRUCTURES ||--o{ COTISATIONS : "lance"
    STRUCTURES ||--o{ MEMBRES : "regroupe"

    COMPTES_TRESORERIE ||--o{ TRANSACTIONS : "enregistre"
    CATEGORIES_MOUVEMENT ||--o{ TRANSACTIONS : "classe"
    UTILISATEURS ||--o{ TRANSACTIONS : "cree"

    TRANSACTIONS ||--o{ IMPUTATIONS_ANALYTIQUES : "ventile_sur"
    CENTRES_ANALYTIQUES ||--o{ IMPUTATIONS_ANALYTIQUES : "recoit"
    CENTRES_ANALYTIQUES ||--o{ CENTRES_ANALYTIQUES : "sous_centre"
    CENTRES_ANALYTIQUES ||--o{ BUDGETS : "suivi_par"

    TRANSACTIONS ||--o| DEMANDES_VALIDATION : "soumet"
    UTILISATEURS ||--o{ DEMANDES_VALIDATION : "traite"

    COTISATIONS ||--o{ PAIEMENTS_COTISATIONS : "genere"
    MEMBRES ||--o{ PAIEMENTS_COTISATIONS : "regle"
    TRANSACTIONS ||--o| PAIEMENTS_COTISATIONS : "encaisse"

    UTILISATEURS ||--o{ JOURNAL_ACTIVITES : "trace"

    ROLES {
        int id PK
        varchar code UK
        varchar nom
        varchar description
        boolean est_actif
    }

    PERMISSIONS {
        int id PK
        varchar code UK
        varchar libelle
        varchar module
    }

    UTILISATEURS {
        int id PK
        varchar nom_complet
        varchar email UK
        varchar mot_de_passe_hash
        varchar telephone
        int role_id FK
        int structure_id FK
        boolean est_actif
    }

    STRUCTURES {
        int id PK
        varchar code UK
        varchar nom
        enum type_structure
        int parent_id FK
        int responsable_id FK
        varchar email_contact
        varchar telephone
        boolean est_actif
    }

    COMPTES_TRESORERIE {
        int id PK
        varchar nom_compte
        int structure_id FK
        enum type_compte
        varchar etablissement
        varchar numero_reference
        decimal solde_initial
        decimal solde_actuel
        varchar devise
        boolean est_actif
    }

    CENTRES_ANALYTIQUES {
        int id PK
        varchar code UK
        varchar libelle
        int parent_id FK
        int structure_id FK
        boolean est_actif
    }

    TRANSACTIONS {
        int id PK
        varchar reference UK
        date date_operation
        varchar libelle
        enum type_transaction
        decimal montant
        enum mode_paiement
        varchar justificatif_path
        enum statut
        int compte_id FK
        int categorie_id FK
        int structure_id FK
        int cree_par FK
        int valide_par FK
        char transfert_groupe_uuid
    }

    IMPUTATIONS_ANALYTIQUES {
        int id PK
        int transaction_id FK
        int centre_analytique_id FK
        decimal montant_impute
        decimal pourcentage
    }

    DEMANDES_VALIDATION {
        int id PK
        int transaction_id FK
        int demandeur_id FK
        int validateur_id FK
        text motif_demande
        text commentaire_decision
        enum statut
        datetime date_demande
        datetime date_decision
    }

    BUDGETS {
        int id PK
        varchar intitule
        int structure_id FK
        int centre_analytique_id FK
        decimal montant_prevu
        date date_debut
        date date_fin
        enum statut
    }

    COTISATIONS {
        int id PK
        varchar nom
        int structure_id FK
        decimal montant_par_membre
        date date_limite
        varchar population_concernee
        enum statut
    }

    PAIEMENTS_COTISATIONS {
        int id PK
        int cotisation_id FK
        int membre_id FK
        decimal montant_du
        decimal montant_paye
        date date_echeance
        date date_paiement
        enum statut
        int transaction_id FK
    }

    JOURNAL_ACTIVITES {
        bigint id PK
        datetime date_action
        int utilisateur_id FK
        varchar action
        varchar module
        varchar entite_cible
        int entite_id
        text details
        varchar adresse_ip
    }
```

---

## 4. Diagrammes UML

### 4.1 Diagramme de Cas d'Utilisation (Use Case)

```mermaid
flowchart LR
    Admin([Administrateur])
    Tresorier([Trésorier])
    Consultant([Consultant])

    subgraph FINCORE["Système IIT FINCORE"]
        UC_Auth["S'authentifier / Gérer son profil"]
        UC_Dash["Consulter Tableaux de bord & Rapports"]
        UC_Export["Exporter rapports (PDF / Excel / CSV)"]
        UC_Struct["Gérer les Structures"]
        UC_Comptes["Gérer les Comptes de trésorerie"]
        UC_Mvt["Saisir Recettes, Dépenses & Transferts"]
        UC_Ventil["Ventiler sur plusieurs Centres Analytiques"]
        UC_Centres["Gérer les Centres Analytiques"]
        UC_Budget["Créer & Suivre les Budgets"]
        UC_Cotis["Gérer les Cotisations & Impayés"]
        UC_Valid["Approuver / Rejeter les demandes de dépenses"]
        UC_Users["Gérer Utilisateurs, Rôles & Permissions"]
        UC_Audit["Consulter le Journal d'Audit & Paramètres"]
    end

    Consultant --> UC_Auth
    Consultant --> UC_Dash
    Consultant --> UC_Export

    Tresorier --> UC_Auth
    Tresorier --> UC_Dash
    Tresorier --> UC_Export
    Tresorier --> UC_Comptes
    Tresorier --> UC_Mvt
    UC_Mvt -.->|include| UC_Ventil
    Tresorier --> UC_Centres
    Tresorier --> UC_Budget
    Tresorier --> UC_Cotis

    Admin --> UC_Auth
    Admin --> UC_Dash
    Admin --> UC_Export
    Admin --> UC_Struct
    Admin --> UC_Comptes
    Admin --> UC_Mvt
    Admin --> UC_Centres
    Admin --> UC_Budget
    Admin --> UC_Cotis
    Admin --> UC_Valid
    Admin --> UC_Users
    Admin --> UC_Audit
```

### 4.2 Diagramme de Classes (Couche Domaine C#)

```mermaid
classDiagram
    class Utilisateur {
        +int Id
        +string NomComplet
        +string Email
        +string Telephone
        +Role Role
        +Structure? Structure
        +bool EstActif
        +bool APermission(string codePermission)
    }

    class Role {
        +int Id
        +string Code
        +string Nom
        +List~Permission~ Permissions
    }

    class Structure {
        +int Id
        +string Code
        +string Nom
        +TypeStructure Type
        +Structure? Parent
        +List~CompteTresorerie~ Comptes
    }

    class CompteTresorerie {
        +int Id
        +string NomCompte
        +TypeCompte TypeCompte
        +string Etablissement
        +string NumeroReference
        +decimal SoldeInitial
        +decimal SoldeActuel
        +string Devise
        +bool EstActif
        +decimal CalculerSolde()
    }

    class Transaction {
        +int Id
        +string Reference
        +DateTime DateOperation
        +string Libelle
        +TypeTransaction Type
        +decimal Montant
        +ModePaiement ModePaiement
        +StatutTransaction Statut
        +string? JustificatifPath
        +List~ImputationAnalytique~ Imputations
        +bool VerifierEquilibreVentilation()
    }

    class CentreAnalytique {
        +int Id
        +string Code
        +string Libelle
        +CentreAnalytique? Parent
        +bool EstActif
    }

    class ImputationAnalytique {
        +int Id
        +int TransactionId
        +int CentreAnalytiqueId
        +decimal MontantImpute
        +decimal Pourcentage
    }

    class DemandeValidation {
        +int Id
        +int TransactionId
        +StatutValidation Statut
        +string MotifDemande
        +string? CommentaireDecision
        +void Approuver(int validateurId, string commentaire)
        +void Rejeter(int validateurId, string commentaire)
    }

    class Budget {
        +int Id
        +string Intitule
        +decimal MontantPrevu
        +DateTime DateDebut
        +DateTime DateFin
        +decimal CalculerTauxExecution(decimal realise)
    }

    Utilisateur "*" --> "1" Role
    Structure "1" --> "*" CompteTresorerie
    CompteTresorerie "1" --> "*" Transaction
    Transaction "1" *-- "1..*" ImputationAnalytique
    ImputationAnalytique "*" --> "1" CentreAnalytique
    Transaction "1" --> "0..1" DemandeValidation
    Budget "*" --> "1" CentreAnalytique
```

### 4.3 Diagramme de Séquence — Saisie d'une dépense avec multi-ventilation et validation

```mermaid
sequenceDiagram
    autonumber
    actor T as Trésorier (WinForms)
    participant UI as FrmAjouterDepense
    participant Svc as TransactionService
    participant Repo as MySQL Repository
    actor A as Administrateur

    T->>UI: Saisit dépense (320 000 FCFA) + 2 centres analytiques (60% / 40%) + justificatif
    UI->>Svc: CreerDepenseAvecVentilation(dto, imputations)
    Svc->>Svc: Vérifie SUM(imputations) == 320 000 FCFA
    alt Somme incohérente
        Svc-->>UI: Erreur métier (Ventilation déséquilibrée)
    else Somme valide
        Svc->>Repo: BEGIN TRANSACTION
        Svc->>Repo: INSERT INTO transactions (statut='EN_ATTENTE')
        Svc->>Repo: INSERT INTO imputations_analytiques (2 lignes)
        Svc->>Repo: INSERT INTO demandes_validation (statut='EN_ATTENTE')
        Svc->>Repo: INSERT INTO journal_activites ('Création dépense')
        Svc->>Repo: COMMIT
        Svc-->>UI: Succès (En attente de validation)
    end

    A->>UI: Ouvre FrmApprobationDemande et clique "Approuver"
    UI->>Svc: ApprouverDemande(demandeId, adminId, commentaire)
    Svc->>Repo: BEGIN TRANSACTION
    Svc->>Repo: UPDATE demandes_validation SET statut='APPROUVEE'
    Svc->>Repo: UPDATE transactions SET statut='VALIDEE'
    Note over Repo: Trigger MySQL met à jour comptes_tresorerie.solde_actuel
    Svc->>Repo: INSERT INTO journal_activites ('Validation dépense')
    Svc->>Repo: COMMIT
    Svc-->>UI: Dépense validée et solde mis à jour
```

---

## 5. Architecture recommandée pour le projet C# WinForms (.NET 8)

```text
IIT.FinCore.sln
├── IIT.FinCore.Domain/          # Entités métier, Enums, Règles de validation
├── IIT.FinCore.Data/            # Accès MySQL (MySqlConnector + Dapper ou EF Core Pomelo)
├── IIT.FinCore.Services/        # Logique métier (Auth, Transactions, Ventilation, Budgets, Audit, Exports)
└── IIT.FinCore.WinForms/        # Interface utilisateur (Charte IIT FINCORE : Bleu marine #1E344E + Or #C5A45C)
    ├── Forms/
    │   ├── Auth/                # FrmSplash, FrmLogin
    │   ├── Shell/               # FrmMain (Sidebar + Topbar + Navigation UserControls)
    │   └── Dialogs/             # Modales d'ajout/édition/validation
    └── Controls/                # UserControls par module (Dashboard, Structures, Comptes, Transactions, ...)
```
