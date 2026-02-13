using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Services
{
    public static class CsvExportService
    {
        public static void ExportHoldings(IEnumerable<MetalHolding> holdings, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var h in holdings)
            {
                sb.AppendLine($"{(int)h.MetalType};{h.Form};{(int)h.CollectableType};{h.Purity};{h.Weight};{h.Quantity};{h.PurchasePrice};{h.PurchaseDate:yyyy-MM-dd}");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportHoldingsDetailed(IEnumerable<MetalHolding> holdings, string filePath)
        {
            var sb = new StringBuilder();

            string L(string key) => Application.Current?.TryFindResource(key) as string ?? key;
            sb.AppendLine(
                $"{L("Common_MetalType")}; " +
                $"{L("Common_Form")}; " +
                $"{L("Common_CollectableType")}; " +
                $"{L("Common_Purity")}; " +
                $"{L("Common_Weight")}; " +
                $"{L("Common_Quantity")}; " +
                $"{L("Common_PurchasePrice")}; " +
                $"{L("Common_PurchaseDate")}; "
            );

            foreach (var h in holdings)
            {
                var metalTypeLabel = L(h.MetalType.ToString());
                var collectableTypeLabel = L(h.CollectableType.ToString());

                sb.AppendLine(
                    $"{metalTypeLabel}; " +
                    $"{h.Form}; " +
                    $"{collectableTypeLabel}; " +
                    $"{h.Purity}; " +
                    $"{h.Weight}; " +
                    $"{h.Quantity}; " +
                    $"{h.PurchasePrice:F2}; " +
                    $"{h.PurchaseDate:dd.MM.yyyy}; "
                );
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}