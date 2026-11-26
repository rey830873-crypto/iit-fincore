using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public class UcUtilisateurs : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _gridUsers = Theme.CreateStyledGrid();
    private readonly DataGridView _gridRoles = Theme.CreateStyledGrid();
    private readonly DataGridView _gridPerms = Theme.CreateStyledGrid();

    private readonly TextBox _txtNom = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPwd = new();
    private readonly TextBox _txtTel = new();
    private readonly ComboBox _cboRole = new();
    private readonly ComboBox _cboStructure = new();

    private List<Utilisateur> _users = new();
    private List<Role> _roles = new();
    private List<PermissionItem> _perms = new();
    private bool _loadingMatrix;

    public UcUtilisateurs()
    {
        BackColor = Theme.PageBg;
        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBodyBold };

        // ==================== ONGLET 1 : UTILISATEURS ====================
        var tabUsers = new TabPage("Utilisateurs") { BackColor = Theme.PageBg, Padding = new Padding(8) };

        var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
        formCard.Dock = DockStyle.Top;
        formCard.AutoSize = true;

        var lbl = new Label { Text = "Créer un utilisateur", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };
        _txtPwd.UseSystemPasswordChar = true;
        _cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboStructure.DropDownStyle = ComboBoxStyle.DropDownList;

        var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
        flow.Controls.Add(Theme.CreateFieldGroup("Nom complet", _txtNom, 165));
        flow.Controls.Add(Theme.CreateFieldGroup("Adresse e-mail", _txtEmail, 175));
        flow.Controls.Add(Theme.CreateFieldGroup("Mot de passe", _txtPwd, 125));
        flow.Controls.Add(Theme.CreateFieldGroup("Rôle", _cboRole, 135));
        flow.Controls.Add(Theme.CreateFieldGroup("Structure", _cboStructure, 155));
        flow.Controls.Add(Theme.CreateFieldGroup("Téléphone", _txtTel, 125));

        var btnAdd = Theme.CreatePrimaryButton("+ Créer", 100, 34);
        btnAdd.Margin = new Padding(4, 18, 8, 4);
        btnAdd.Click += (_, _) => AddUser();

        var btnToggle = Theme.CreateSecondaryButton("Activer / Désactiver", 150, 34);
        btnToggle.Margin = new Padding(0, 18, 0, 4);
        btnToggle.Click += (_, _) => ToggleSelectedUser();

        flow.Controls.Add(btnAdd);
        flow.Controls.Add(btnToggle);
        formCard.Controls.Add(flow);
        formCard.Controls.Add(lbl);

        var listUsers = Theme.CreateCardPanel();
        listUsers.Dock = DockStyle.Fill;
        _gridUsers.CellDoubleClick += (_, _) => ShowUserDetails();
        listUsers.Controls.Add(_gridUsers);
        listUsers.Controls.Add(new Label { Text = "Double-cliquez sur un utilisateur pour consulter sa fiche complète", Font = Theme.FontSubtitle, ForeColor = Theme.TextMuted, Dock = DockStyle.Top, Height = 26 });

        tabUsers.Controls.Add(listUsers);
        tabUsers.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
        tabUsers.Controls.Add(formCard);

        // ==================== ONGLET 2 : RÔLES ====================
        var tabRoles = new TabPage("Gestion des rôles") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        var cardRoles = Theme.CreateCardPanel(); cardRoles.Dock = DockStyle.Fill;
        cardRoles.Controls.Add(_gridRoles);
        tabRoles.Controls.Add(cardRoles);

        // ==================== ONGLET 3 : MATRICE DES PERMISSIONS ====================
        var tabPerms = new TabPage("Matrice des permissions") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        var cardPerms = Theme.CreateCardPanel(); cardPerms.Dock = DockStyle.Fill;
        _gridPerms.ReadOnly = false;
        _gridPerms.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_gridPerms.IsCurrentCellDirty)
                _gridPerms.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _gridPerms.CellValueChanged += OnPermissionToggled;

        cardPerms.Controls.Add(_gridPerms);
        cardPerms.Controls.Add(new Label { Text = "Cochez ou décochez une autorisation pour la mettre à jour instantanément.", Font = Theme.FontSubtitle, ForeColor = Theme.TextMuted, Dock = DockStyle.Top, Height = 28 });
        tabPerms.Controls.Add(cardPerms);

        tabs.TabPages.AddRange(new[] { tabUsers, tabRoles, tabPerms });
        Controls.Add(tabs);
    }

    private void LoadData()
    {
        _roles = _repo.GetRoles();
        _cboRole.DataSource = _roles.ToList();
        _cboRole.DisplayMember = "Nom"; _cboRole.ValueMember = "Id";

        _cboStructure.DataSource = _repo.GetStructures();
        _cboStructure.DisplayMember = "Nom"; _cboStructure.ValueMember = "Id";

        _users = _repo.GetUtilisateurs();
        _gridUsers.DataSource = _users.Select(u => new
        {
            Nom = u.NomComplet,
            Email = u.Email,
            Rôle = u.RoleNom,
            Structure = u.StructureNom ?? "Global",
            Téléphone = u.Telephone,
            Dernier_Accès = u.DernierAcces?.ToString("dd/MM/yyyy HH:mm") ?? "Jamais",
            Statut = u.EstActif ? "Actif" : "Désactivé"
        }).ToList();

        _gridRoles.DataSource = _roles.Select(r => new
        {
            r.Code,
            Rôle = r.Nom,
            Description = r.Description,
            Utilisateurs = r.NbUtilisateurs,
            Statut = r.EstActif ? "Actif" : "Inactif"
        }).ToList();

        LoadPermissionMatrix();
    }

    private void LoadPermissionMatrix()
    {
        _loadingMatrix = true;
        _gridPerms.Columns.Clear();
        _gridPerms.Rows.Clear();

        _perms = _repo.GetPermissions();
        var pairs = _repo.GetRolePermissionPairs();

        _gridPerms.Columns.Add(new DataGridViewTextBoxColumn { Name = "Module", HeaderText = "Module", ReadOnly = true });
        _gridPerms.Columns.Add(new DataGridViewTextBoxColumn { Name = "Permission", HeaderText = "Permission", ReadOnly = true });

        foreach (var r in _roles)
        {
            _gridPerms.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = $"ROLE_{r.Id}",
                HeaderText = r.Nom,
                ReadOnly = r.Code == "ADMIN"
            });
        }

        foreach (var p in _perms)
        {
            var rowValues = new object[2 + _roles.Count];
            rowValues[0] = p.Module;
            rowValues[1] = p.Libelle;
            for (int i = 0; i < _roles.Count; i++)
                rowValues[2 + i] = pairs.Contains((_roles[i].Id, p.Id));
            _gridPerms.Rows.Add(rowValues);
        }
        _loadingMatrix = false;
    }

    private void OnPermissionToggled(object? sender, DataGridViewCellEventArgs e)
    {
        if (_loadingMatrix || e.RowIndex < 0 || e.ColumnIndex < 2) return;
        int roleIdx = e.ColumnIndex - 2;
        if (roleIdx < 0 || roleIdx >= _roles.Count || e.RowIndex >= _perms.Count) return;

        var role = _roles[roleIdx];
        var perm = _perms[e.RowIndex];
        bool granted = Convert.ToBoolean(_gridPerms.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
        _repo.SetRolePermission(role.Id, perm.Id, granted, UserSession.CurrentUser!.Id);
    }

    private void ShowUserDetails()
    {
        if (_gridUsers.CurrentRow == null) return;
        int idx = _gridUsers.CurrentRow.Index;
        if (idx < 0 || idx >= _users.Count) return;

        var u = _users[idx];
        using var dlg = new FrmDetailsDialog($"Utilisateur — {u.NomComplet}", new Dictionary<string, string>
        {
            ["Nom complet"] = u.NomComplet,
            ["Adresse e-mail"] = u.Email,
            ["Téléphone"] = u.Telephone ?? "—",
            ["Rôle"] = $"{u.RoleNom} ({u.RoleCode})",
            ["Structure"] = u.StructureNom ?? "Global",
            ["Statut"] = u.EstActif ? "Actif" : "Désactivé",
            ["Dernier accès"] = u.DernierAcces?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Jamais connecté"
        });
        dlg.ShowDialog(this);
    }

    private void ToggleSelectedUser()
    {
        if (_gridUsers.CurrentRow == null) return;
        int idx = _gridUsers.CurrentRow.Index;
        if (idx < 0 || idx >= _users.Count) return;

        var u = _users[idx];
        if (u.Id == UserSession.CurrentUser!.Id)
        {
            MessageBox.Show("Vous ne pouvez pas désactiver votre propre compte.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _repo.ToggleUtilisateurActif(u.Id, !u.EstActif, UserSession.CurrentUser.Id);
        LoadData();
    }

    private void AddUser()
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text) || string.IsNullOrWhiteSpace(_txtEmail.Text) || string.IsNullOrWhiteSpace(_txtPwd.Text))
        {
            MessageBox.Show("Nom, e-mail et mot de passe sont obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var (okPwd, pwdMsg) = SecurityService.ValidatePasswordStrength(_txtPwd.Text);
        if (!okPwd)
        {
            MessageBox.Show(pwdMsg, "Politique de sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            _repo.CreateUtilisateur(
                _txtNom.Text.Trim(), _txtEmail.Text.Trim(), _txtPwd.Text, _txtTel.Text.Trim(),
                (int)_cboRole.SelectedValue!, _cboStructure.SelectedValue as int?, UserSession.CurrentUser!.Id);

            _txtNom.Clear(); _txtEmail.Clear(); _txtPwd.Clear(); _txtTel.Clear();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
