using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

        public static void ExportHoldingsDetailed(IEnumerable<MetalHolding> holdings, string filePath, DetailedExportTexts texts)
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                $"{texts.MetalTypeHeader}; " +
                $"{texts.FormHeader}; " +
                $"{texts.CollectableTypeHeader}; " +
                $"{texts.PurityHeader}; " +
                $"{texts.WeightHeader}; " +
                $"{texts.QuantityHeader}; " +
                $"{texts.PurchasePriceHeader}; " +
                $"{texts.PurchaseDateHeader}; ");

            foreach (var h in holdings)
            {
                var metalTypeLabel = GetMetalTypeLabel(h.MetalType, texts);
                var collectableTypeLabel = GetCollectableTypeLabel(h.CollectableType, texts);

                sb.AppendLine(
                    $"{metalTypeLabel}; " +
                    $"{h.Form}; " +
                    $"{collectableTypeLabel}; " +
                    $"{h.Purity}; " +
                    $"{h.Weight}; " +
                    $"{h.Quantity}; " +
                    $"{h.PurchasePrice.ToString(PriceNumberFormat, CultureInfo.InvariantCulture)}; " +
                    $"{h.PurchaseDate.ToString(DetailedExportDateFormat, CultureInfo.InvariantCulture)}; ");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string GetMetalTypeLabel(MetalType metalType, DetailedExportTexts texts)
            => metalType switch
            {
                MetalType.Gold => texts.GoldLabel,
                MetalType.Silver => texts.SilverLabel,
                MetalType.Bronce => texts.BronceLabel,
                MetalType.Platinum => texts.PlatinumLabel,
                MetalType.Palladium => texts.PalladiumLabel,
                _ => metalType.ToString()
            };

        private static string GetCollectableTypeLabel(CollectableType collectableType, DetailedExportTexts texts)
            => collectableType switch
            {
                CollectableType.Bullion => texts.BullionLabel,
                CollectableType.SemiNumismatic => texts.SemiNumismaticLabel,
                CollectableType.Numismatic => texts.NumismaticLabel,
                _ => collectableType.ToString()
            };
    }
}