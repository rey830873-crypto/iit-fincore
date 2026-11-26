using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public class UcAudit : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly TextBox _txtSearch = new();
    private List<JournalActiviteItem> _allLogs = new();
    private List<JournalActiviteItem> _filtered = new();

    public UcAudit()
    {
        BackColor = Theme.PageBg;
        var card = Theme.CreateCardPanel();
        card.Dock = DockStyle.Fill;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent };
        _txtSearch.SetBounds(0, 4, 280, 28);
        _txtSearch.PlaceholderText = "Rechercher utilisateur, action, module...";
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        var lblHint = new Label
        {
            Text = "Double-cliquez sur une ligne pour consulter le détail complet de l'événement",
            Font = Theme.FontSubtitle,
            ForeColor = Theme.TextMuted,
            AutoSize = true,
            Location = new Point(296, 10)
        };
        topBar.Controls.AddRange(new Control[] { _txtSearch, lblHint });

        _grid.CellDoubleClick += (_, _) => ShowDetails();
        card.Controls.Add(_grid);
        card.Controls.Add(topBar);
        Controls.Add(card);

        _allLogs = _repo.GetJournalActivites();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _txtSearch.Text.Trim();
        _filtered = string.IsNullOrEmpty(q)
            ? _allLogs
            : _allLogs.Where(j =>
                j.UtilisateurNom.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                j.Action.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                j.Module.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (j.Details?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

        _grid.DataSource = _filtered.Select(j => new
        {
            Date = j.DateAction.ToString("dd/MM/yyyy HH:mm:ss"),
            Utilisateur = j.UtilisateurNom,
            Action = j.Action,
            Module = j.Module,
            Détails = j.Details,
            Adresse_IP = j.AdresseIp
        }).ToList();
    }

    private void ShowDetails()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var j = _filtered[idx];
        using var dlg = new FrmDetailsDialog("Détails de l'activité d'audit", new Dictionary<string, string>
        {
            ["ID Événement"] = $"#{j.Id}",
            ["Date & Heure"] = j.DateAction.ToString("dd/MM/yyyy HH:mm:ss"),
            ["Utilisateur"] = j.UtilisateurNom,
            ["Module"] = j.Module,
            ["Action"] = j.Action,
            ["Adresse IP"] = j.AdresseIp ?? "—",
            ["Détails"] = j.Details ?? "—"
        });
        dlg.ShowDialog(this);
    }
}
