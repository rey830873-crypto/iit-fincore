using System.Diagnostics;
using IIT.FinCore.WinForms.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IIT.FinCore.WinForms.Services;

/// <summary>
/// Service de génération de documents PDF officiels IIT FINCORE (Rapports financiers & Reçus).
/// </summary>
public static class PdfReportService
{
    private const string NavyHex = "#061B3A";
    private const string GoldHex = "#D49212";
    private const string SlateHex = "#64748B";
    private const string LightBgHex = "#F4F7FB";

    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static string? GetLogoPath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "logo_fincore.jpg");
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Génère le Rapport Financier & Analytique officiel (PDF) avec logo, KPI, synthèse analytique et transactions.
    /// </summary>
    public static void GenerateFinancialReport(
        string outputPath,
        string orgName,
        DashboardKpi kpi,
        List<SyntheseAnalytiqueRow> synthese,
        List<TransactionItem> transactions,
        string generatedBy)
    {
        var logoPath = GetLogoPath();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor("#0C1F3D"));

                // En-tête officiel avec Logo
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        if (logoPath != null)
                        {
                            row.ConstantItem(58).Height(58).Image(logoPath).FitArea();
                            row.ConstantItem(14);
                        }

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(orgName.ToUpperInvariant()).FontSize(17).Bold().FontColor(NavyHex);
                            c.Item().Text("RAPPORT FINANCIER & ANALYTIQUE DE TRÉSORERIE").FontSize(10.5f).Bold().FontColor(GoldHex);
                            c.Item().Text($"Édité le {DateTime.Now:dd/MM/yyyy à HH:mm}  •  Par : {generatedBy}").FontSize(8.5f).FontColor(SlateHex);
                        });
                    });

                    col.Item().PaddingTop(10).LineHorizontal(2).LineColor(GoldHex);
                });

                // Contenu principal
                page.Content().PaddingVertical(14).Column(col =>
                {
                    col.Spacing(14);

                    // 1. Cartes KPI
                    col.Item().Row(row =>
                    {
                        row.Spacing(10);
                        KpiBox(row, "SOLDE GLOBAL", $"{kpi.SoldeGlobal:N0} FCFA", NavyHex);
                        KpiBox(row, "RECETTES VALIDÉES", $"+{kpi.TotalRecettes:N0} FCFA", "#109868");
                        KpiBox(row, "DÉPENSES VALIDÉES", $"-{kpi.TotalDepenses:N0} FCFA", "#D94452");
                        KpiBox(row, "STRUCTURES ACTIVES", $"{kpi.StructuresActives}", GoldHex);
                    });

                    // 2. Synthèse Analytique
                    col.Item().Text("1. Synthèse par centre d'imputation analytique").FontSize(12).Bold().FontColor(NavyHex);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(65);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(1.4f);
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Code");
                            HeaderCell(h, "Centre analytique");
                            HeaderCell(h, "Structure");
                            HeaderCell(h, "Recettes", true);
                            HeaderCell(h, "Dépenses", true);
                            HeaderCell(h, "Résultat net", true);
                        });

                        bool alt = false;
                        foreach (var s in synthese)
                        {
                            var bg = alt ? LightBgHex : "#FFFFFF";
                            alt = !alt;
                            BodyCell(table, s.CentreCode, bg);
                            BodyCell(table, s.CentreLibelle, bg);
                            BodyCell(table, s.StructureNom, bg);
                            BodyCell(table, $"{s.TotalRecettes:N0}", bg, true, "#109868");
                            BodyCell(table, $"{s.TotalDepenses:N0}", bg, true, "#D94452");
                            BodyCell(table, $"{s.ResultatNet:N0}", bg, true, s.ResultatNet >= 0 ? "#109868" : "#D94452");
                        }
                    });

                    // 3. Journal des opérations récentes
                    col.Item().Text("2. Journal des mouvements de trésorerie").FontSize(12).Bold().FontColor(NavyHex);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(62);
                            c.ConstantColumn(82);
                            c.RelativeColumn(2.2f);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.3f);
                            c.ConstantColumn(62);
                        });

                        table.Header(h =>
                        {
                            HeaderCell(h, "Date");
                            HeaderCell(h, "Référence");
                            HeaderCell(h, "Libellé");
                            HeaderCell(h, "Compte");
                            HeaderCell(h, "Montant (FCFA)", true);
                            HeaderCell(h, "Statut");
                        });

                        bool alt = false;
                        foreach (var t in transactions.Take(60))
                        {
                            var bg = alt ? LightBgHex : "#FFFFFF";
                            alt = !alt;
                            bool isPlus = t.TypeTransaction is "RECETTE" or "TRANSFERT_ENTRANT";
                            BodyCell(table, t.DateOperation.ToString("dd/MM/yyyy"), bg);
                            BodyCell(table, t.Reference, bg);
                            BodyCell(table, t.Libelle, bg);
                            BodyCell(table, t.CompteNom, bg);
                            BodyCell(table, (isPlus ? "+" : "-") + $"{t.Montant:N0}", bg, true, isPlus ? "#109868" : "#D94452");
                            BodyCell(table, t.Statut, bg);
                        }
                    });

                    // Zone Visa / Signatures
                    col.Item().PaddingTop(18).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Le Trésorier").Bold().FontColor(NavyHex);
                            c.Item().PaddingTop(28).LineHorizontal(1).LineColor(SlateHex);
                        });
                        row.ConstantItem(60);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Le Président / Administrateur").Bold().FontColor(NavyHex);
                            c.Item().PaddingTop(28).LineHorizontal(1).LineColor(SlateHex);
                        });
                    });
                });

                // Pied de page numéroté
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span($"{orgName} - Document officiel généré par IIT FINCORE   •   Page ").FontSize(8).FontColor(SlateHex);
                    txt.CurrentPageNumber().FontSize(8).Bold().FontColor(NavyHex);
                    txt.Span(" / ").FontSize(8).FontColor(SlateHex);
                    txt.TotalPages().FontSize(8).Bold().FontColor(NavyHex);
                });
            });
        }).GeneratePdf(outputPath);

        OpenPdf(outputPath);
    }

    /// <summary>
    /// Génère un Reçu / Bordereau officiel PDF pour une transaction (avec multi-ventilation analytique).
    /// </summary>
    public static void GenerateTransactionReceipt(
        string outputPath,
        string orgName,
        TransactionItem trx,
        List<ImputationAnalytique> imputations)
    {
        var logoPath = GetLogoPath();
        bool isRecette = trx.TypeTransaction is "RECETTE" or "TRANSFERT_ENTRANT";

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor("#0C1F3D"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        if (logoPath != null)
                        {
                            row.ConstantItem(48).Height(48).Image(logoPath).FitArea();
                            row.ConstantItem(12);
                        }
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(orgName.ToUpperInvariant()).FontSize(14).Bold().FontColor(NavyHex);
                            c.Item().Text(isRecette ? "REÇU OFFICIEL D'ENCAISSEMENT" : "BORDEREAU OFFICIEL DE DÉPENSE").FontSize(10).Bold().FontColor(GoldHex);
                        });
                        row.ConstantItem(150).AlignRight().Column(c =>
                        {
                            c.Item().Text($"N° {trx.Reference}").FontSize(10).Bold().FontColor(NavyHex);
                            c.Item().Text($"Date : {trx.DateOperation:dd/MM/yyyy}").FontSize(8.5f).FontColor(SlateHex);
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(GoldHex);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Background(LightBgHex).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Libellé : {trx.Libelle}").Bold();
                            c.Item().Text($"Structure : {trx.StructureNom}  •  Compte : {trx.CompteNom}");
                            c.Item().Text($"Mode de règlement : {trx.ModePaiement}  •  Statut : {trx.Statut}");
                        });
                        row.ConstantItem(165).AlignRight().AlignMiddle()
                            .Text($"{trx.Montant:N0} FCFA").FontSize(16).Bold().FontColor(isRecette ? "#109868" : "#D94452");
                    });

                    if (imputations.Count > 0)
                    {
                        col.Item().Text("Ventilation analytique :").FontSize(9).Bold().FontColor(NavyHex);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1.5f); c.RelativeColumn(1); });
                            t.Header(h =>
                            {
                                HeaderCell(h, "Centre analytique");
                                HeaderCell(h, "Montant (FCFA)", true);
                                HeaderCell(h, "Part", true);
                            });
                            foreach (var imp in imputations)
                            {
                                BodyCell(t, imp.CentreLibelle, "#FFFFFF");
                                BodyCell(t, $"{imp.MontantImpute:N0}", "#FFFFFF", true);
                                BodyCell(t, $"{imp.Pourcentage:N1} %", "#FFFFFF", true);
                            }
                        });
                    }

                    col.Item().PaddingTop(14).Row(row =>
                    {
                        row.RelativeItem().Text($"Établi par : {trx.CreeParNom ?? "Trésorerie"}").FontSize(8.5f).FontColor(SlateHex);
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text("Signature & Cachet").FontSize(8.5f).Bold().FontColor(NavyHex);
                            c.Item().PaddingTop(20).LineHorizontal(1).LineColor(SlateHex);
                        });
                    });
                });

                page.Footer().AlignCenter()
                    .Text($"Document certifié IIT FINCORE - Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(7.5f).FontColor(SlateHex);
            });
        }).GeneratePdf(outputPath);

        OpenPdf(outputPath);
    }

    /// <summary>
    /// Génère un Avis de Relance officiel PDF pour un membre en retard de cotisation.
    /// </summary>
    public static void GenerateUnpaidReminderPdf(string outputPath, string orgName, PaiementCotisationItem p)
    {
        var logoPath = GetLogoPath();
        var reste = Math.Max(0, p.MontantDu - p.MontantPaye);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor("#0C1F3D"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        if (logoPath != null)
                        {
                            row.ConstantItem(46).Height(46).Image(logoPath).FitArea();
                            row.ConstantItem(12);
                        }
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(orgName.ToUpperInvariant()).FontSize(14).Bold().FontColor(NavyHex);
                            c.Item().Text("AVIS D'ÉCHÉANCE & RELANCE DE COTISATION").FontSize(10).Bold().FontColor("#D94452");
                        });
                        row.ConstantItem(130).AlignRight().Text($"Date : {DateTime.Now:dd/MM/yyyy}").FontSize(8.5f).FontColor(SlateHex);
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(GoldHex);
                });

                page.Content().PaddingVertical(14).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Bonjour {p.MembreNom},").Bold();
                    col.Item().Text($"Sauf erreur ou omission de notre part, nous constatons un solde restant dû concernant votre cotisation « {p.CotisationNom} » au sein de la structure {p.StructureNom}.");

                    col.Item().Background(LightBgHex).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Montant total : {p.MontantDu:N0} FCFA");
                            c.Item().Text($"Déjà réglé : {p.MontantPaye:N0} FCFA");
                            c.Item().Text($"Date limite d'échéance : {p.DateEcheance:dd/MM/yyyy}");
                        });
                        row.ConstantItem(170).AlignRight().AlignMiddle().Column(c =>
                        {
                            c.Item().AlignRight().Text("RESTE À RÉGLER").FontSize(8).Bold().FontColor(SlateHex);
                            c.Item().AlignRight().Text($"{reste:N0} FCFA").FontSize(16).Bold().FontColor("#D94452");
                        });
                    });

                    col.Item().Text("Merci de bien vouloir régulariser votre situation auprès de la trésorerie (Espèces, Virement ou Mobile Money).").FontSize(9);
                    col.Item().PaddingTop(8).AlignRight().Text("La Trésorerie - " + orgName).Bold().FontColor(NavyHex);
                });

                page.Footer().AlignCenter().Text("Avis généré automatiquement par IIT FINCORE").FontSize(7.5f).FontColor(SlateHex);
            });
        }).GeneratePdf(outputPath);

        OpenPdf(outputPath);
    }

    private static void KpiBox(RowDescriptor row, string title, string value, string accentHex)
    {
        row.RelativeItem().Border(1).BorderColor("#E0E7F0").Background("#FFFFFF").Padding(8).Column(c =>
        {
            c.Item().Text(title).FontSize(7.5f).Bold().FontColor(SlateHex);
            c.Item().PaddingTop(3).Text(value).FontSize(11.5f).Bold().FontColor(accentHex);
        });
    }

    private static void HeaderCell(TableCellDescriptor h, string text, bool right = false)
    {
        var cell = h.Cell().Background(NavyHex).Padding(6);
        if (right) cell.AlignRight().Text(text).FontSize(8.5f).Bold().FontColor("#FFFFFF");
        else cell.Text(text).FontSize(8.5f).Bold().FontColor("#FFFFFF");
    }

    private static void BodyCell(TableDescriptor t, string text, string bgHex, bool right = false, string? colorHex = null)
    {
        var cell = t.Cell().Background(bgHex).BorderBottom(0.5f).BorderColor("#E2E8F0").Padding(5);
        var txt = right ? cell.AlignRight().Text(text) : cell.Text(text);
        txt.FontSize(8.5f);
        if (!string.IsNullOrEmpty(colorHex)) txt.Bold().FontColor(colorHex);
    }

    private static void OpenPdf(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch { }
    }
}
