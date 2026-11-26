# Guide Utilisateur & Déploiement Client — IIT FINCORE v2.0

---

## 1. Guide des 3 espaces utilisateurs

### 1.1 Espace Administrateur (`admin@iit.ci`)
- **Tableau de bord Gouvernance** : solde consolidé toutes structures, recettes, dépenses, compteur des demandes en attente, courbe mensuelle, donut analytique, liste des demandes à approuver et dernières activités d'audit.
- **Validations** : approuver ou rejeter les dépenses soumises par les trésoriers (la validation débite automatiquement le compte via trigger MySQL).
- **Prévisions & Clôture** : consulter la projection Cash-Flow à 6 mois, gérer les opérations récurrentes et **clôturer un exercice annuel** (verrouillage définitif + ouverture de l'exercice N+1).
- **Utilisateurs, Rôles & Permissions** : créer/désactiver des comptes et cocher/décocher les autorisations dans la matrice RBAC.
- **Paramètres généraux** : modifier le nom de l'organisation, la devise, le seuil de validation, **sauvegarder la base MySQL en 1 clic (`.sql`)** et **changer le logo client**.

### 1.2 Espace Trésorier (`tresorier@iit.ci`)
- Cloisonné automatiquement sur sa structure d'affectation (*ex. Bureau des Étudiants — BDE*).
- **Opérations & Transferts** : enregistrer des Recettes, des Dépenses avec **multi-ventilation analytique** (plusieurs centres jusqu'à 100 %), effectuer des transferts entre comptes, imprimer des **Reçus PDF** et **pointer le relevé bancaire / Mobile Money**.
- **Budgets & Alertes** : créer des enveloppes budgétaires et recevoir des alertes visuelles à ≥ 80 % et ≥ 100 % (ainsi qu'une alerte préventive lors de la saisie d'une dépense).
- **Cotisations** : créer des campagnes, **importer une liste de membres en CSV**, enregistrer les règlements (créditant automatiquement le compte) et relancer les impayés (**Avis PDF** ou **message WhatsApp/E-mail**).

### 1.3 Espace Consultant (`consultant@iit.ci`)
- **Mode Lecture Seule** affiché en permanence dans l'en-tête.
- Accès au Tableau de bord analytique (KPI *Résultat net global* + Synthèse par centre analytique), consultation des comptes/transactions, prévisions Cash-Flow et **exportation des Rapports Officiels PDF & CSV**.

---

## 2. Format d'import CSV des membres (Cotisations)

Le bouton **`Importer CSV Membres`** accepte un fichier `.csv` encodé en UTF-8 (séparateur `;` ou `,`) :

```csv
Matricule;Nom;Email;Telephone
IIT-2026-101;Kouamé Franck;f.kouame@iit.ci;+225 07 12 34 56 01
IIT-2026-102;Soro Aminata;a.soro@iit.ci;+225 07 12 34 56 02
```
*(Ou simplement `Nom;Telephone` : un matricule unique est généré automatiquement.)*

---

## 3. Déploiement chez un client

### Option A — Livraison rapide (Dossier Release prêt à l'emploi)
1. Copier le dossier `dist\publish\` sur le PC du client.
2. Installer MySQL 8 (ou WampServer) et exécuter `database\iit_fincore.sql` puis `database\upgrade_v2.sql`.
3. Adapter si besoin l'adresse/mot de passe MySQL dans `dist\publish\appsettings.json`.
4. Lancer `IIT.FinCore.WinForms.exe`, se connecter en Administrateur et personnaliser le **Nom d'organisation** et le **Logo client** dans *Profil & Paramètres*.

### Option B — Installeur Windows professionnel (`.exe`)
1. Installer **Inno Setup 6** (gratuit).
2. Exécuter dans PowerShell :
   ```powershell
   .\installer\build_release.ps1
   ```
3. Récupérer l'installeur prêt à distribuer dans `dist\installer\IIT_FINCORE_Setup_v2.0.exe`.
