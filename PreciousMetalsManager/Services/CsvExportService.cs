using System.Collections.Generic;
using System.IO;
using System.Text;
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
                sb.AppendLine($"{(int)h.MetalType};{h.Form};{h.Purity};{h.Weight};{h.Quantity};{h.PurchasePrice};{h.PurchaseDate:yyyy-MM-dd};{(int)h.CollectableType}");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}