using System.Collections.Generic;
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

            static string GetMetalTypeKey(MetalType metalType) => metalType switch
            {
                MetalType.Gold => "Lbl_Gold",
                MetalType.Silver => "Lbl_Silver",
                MetalType.Platinum => "Lbl_Platinum",
                MetalType.Palladium => "Lbl_Palladium",
                MetalType.Bronce => "Lbl_Bronce",
                _ => metalType.ToString()
            };

            static string GetCollectableTypeKey(CollectableType collectableType) => collectableType switch
            {
                CollectableType.Bullion => "CollectableType_Bullion",
                CollectableType.SemiNumismatic => "CollectableType_SemiNumismatic",
                CollectableType.Numismatic => "CollectableType_Numismatic",
                _ => collectableType.ToString()
            };

            static string TrimTrailingColon(string s) => string.IsNullOrWhiteSpace(s) ? s : s.TrimEnd().TrimEnd(':');

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
                var metalTypeLabel = TrimTrailingColon(L(GetMetalTypeKey(h.MetalType)));
                var collectableTypeLabel = L(GetCollectableTypeKey(h.CollectableType));

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