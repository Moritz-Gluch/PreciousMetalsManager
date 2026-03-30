using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Services
{
    public static class CsvExportService
    {
        private const string SimpleExportDateFormat = "yyyy-MM-dd";
        private const string DetailedExportDateFormat = "dd.MM.yyyy";
        private const string PriceNumberFormat = "F2";

        public static void ExportHoldings(IEnumerable<MetalHolding> holdings, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var h in holdings)
            {
                sb.AppendLine($"{(int)h.MetalType};{h.Form};{(int)h.CollectableType};{h.Purity};{h.Weight};{h.Quantity};{h.PurchasePrice};{h.PurchaseDate.ToString(SimpleExportDateFormat, CultureInfo.InvariantCulture)}");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportHoldingsDetailed(IEnumerable<MetalHolding> holdings, string filePath)
        {
            var sb = new StringBuilder();

            string L(string key) => Application.Current?.TryFindResource(key) as string ?? key;

            static string GetMetalTypeKey(MetalType metalType)
                => DomainReferenceData.TryGetMetalLabelResourceKey(metalType, out var key)
                    ? key
                    : metalType.ToString();

            static string GetCollectableTypeKey(CollectableType collectableType)
                => DomainReferenceData.TryGetCollectableLabelResourceKey(collectableType, out var key)
                    ? key
                    : collectableType.ToString();

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
                    $"{h.PurchasePrice.ToString(PriceNumberFormat, CultureInfo.InvariantCulture)}; " +
                    $"{h.PurchaseDate.ToString(DetailedExportDateFormat, CultureInfo.InvariantCulture)}; "
                );
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}