# IIT FINCORE v2.0 — Solution de Gestion d'une Mini-Trésorerie d'Association

**Application Desktop C# .NET 9 Windows Forms & MySQL 8**

---

## 1. Présentation du projet

**IIT FINCORE** est une solution complète de gestion financière et analytique conçue pour les associations, clubs étudiants, écoles et ONG. Elle combine une interface moderne (*Royal Sapphire & Metallic Gold*), un contrôle d'accès à **3 rôles** et une base de données relationnelle **MySQL** sécurisée par triggers et procédures stockées.

### Fonctionnalités principales (v2.0)
- **Gestion multi-structures** : hiérarchie d'entités (Institution, Association, Club, Département).
- **Comptes de trésorerie** : Banque, Caisse espèces, Mobile Money (Orange Money, Wave), avec mise à jour automatique des soldes par triggers MySQL.
- **Mouvements & Multi-ventilation analytique** : saisie de Recettes, Dépenses (avec justificatif) ventilables sur **plusieurs centres analytiques** (contrôle temps réel à 100 %) et **Transferts atomiques** entre comptes (`sp_effectuer_transfert_comptes`).
- **Rapprochement bancaire & Mobile Money** : pointage des écritures face aux relevés + calcul automatique *Solde comptable / Solde rapproché / Écart*.
- **Budgets & Alertes automatiques** : suivi d'exécution budgétaire avec alertes visuelles à **≥ 80 %** et **≥ 100 %**, plus alerte préventive lors de la saisie d'une dépense.
- **Circuit de validation** : approbation / rejet des dépenses dépassant le seuil paramétré.
- **Cotisations, Import CSV & Relances** : campagnes, import en masse de membres via CSV, encaissement générant automatiquement la recette, suivi des impayés, **Avis de relance PDF** et message prêt pour WhatsApp/E-mail.
- **Prévision Cash-Flow (6 mois) & Charges récurrentes** : courbe prévisionnelle du solde futur et planification des opérations périodiques.
- **Clôture d'exercice comptable** : verrouillage SQL des exercices clos (trigger) et ouverture automatique de l'exercice N+1.
- **Rapports officiels PDF & CSV** : Rapport Financier & Analytique A4 avec logo + Reçus/Bordereaux A5 de transaction.
- **Administration & Sécurité** : 3 rôles (**Administrateur**, **Trésorier**, **Consultant**), mots de passe hachés **BCrypt**, matrice interactive des permissions, journal d'audit complet, **sauvegarde MySQL en 1 clic** et **personnalisation du logo client**.

---

## 2. Structure du dossier projet

```text
Gestion d'une mini-trésorerie/
├── IIT.FinCore.sln                        # Solution Visual Studio (.NET 9 WinForms)
├── README.md                              # Documentation générale (ce fichier)
├── database/
│   ├── iit_fincore.sql                    # Script complet de création MySQL (tables, vues, triggers, procédure)
│   ├── upgrade_v2.sql                     # Mise à niveau v2.0 (rapprochement, exercices, charges récurrentes)
│   └── seed_demo.sql                      # Jeu de données réaliste sur 6 mois (juin -> novembre 2026)
├── docs/
│   ├── 01_conception_mcd_uml.md           # Dossier d'analyse : Règles de gestion, MCD/MLD, Diagrammes UML
│   └── 02_guide_utilisateur_et_deploiement.md  # Guide des 3 rôles, installation & livraison client
├── installer/
│   ├── IIT_FINCORE_Setup.iss              # Script Inno Setup 6 (génère IIT_FINCORE_Setup_v2.0.exe)
│   └── build_release.ps1                  # Script automatique de build Release & packaging
├── dist/publish/                          # Exécutable Release prêt à livrer (IIT.FinCore.WinForms.exe)
└── IIT.FinCore.WinForms/                  # Code source C# WinForms
```

---

## 3. Démarrage rapide

1. Démarrer **WampServer / MySQL** (`127.0.0.1:3306`).
2. Vérifier que la base `iit_fincore` est importée (`database/iit_fincore.sql`, `database/upgrade_v2.sql`, `database/seed_demo.sql`).
3. Ouvrir `IIT.FinCore.sln` dans Visual Studio et appuyer sur **F5** (ou lancer `dist\publish\IIT.FinCore.WinForms.exe`).

### Comptes de démonstration (3 rôles)
| Rôle | E-mail | Mot de passe | Périmètre |
| :--- | :--- | :--- | :--- |
| **Administrateur** | `admin@iit.ci` | `admin123` | Accès global multi-structures, validations, clôture, RBAC, audit, sauvegarde |
| **Trésorier** | `tresorier@iit.ci` | `tresorier123` | Gestion opérationnelle de sa structure (*Bureau des Étudiants — BDE*) |
| **Consultant** | `consultant@iit.ci` | `consultant123` | Consultation en lecture seule & exports PDF/CSV |
