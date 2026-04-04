using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class CsvExportServiceTest
    {
        private string _tempFile = null!;

        [TestInitialize]
        public void Setup()
        {
            if (Application.Current == null)
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            _tempFile = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        private static List<MetalHolding> GetSampleHoldings() =>
            new()
            {
                new MetalHolding
                {
                    MetalType = MetalType.Gold,
                    Form = "Bar",
                    CollectableType = CollectableType.Bullion,
                    Purity = 999.9m,
                    Weight = 10.5m,
                    Quantity = 2,
                    PurchasePrice = 1000m,
                    PurchaseDate = new DateTime(2024, 1, 1)
                }
            };

        private static DetailedExportTexts CreateDetailedExportTexts() =>
            new()
            {
                MetalTypeHeader = "Metal Type",
                FormHeader = "Form",
                CollectableTypeHeader = "Collectable Type",
                PurityHeader = "Purity",
                WeightHeader = "Weight",
                QuantityHeader = "Quantity",
                PurchasePriceHeader = "Purchase Price",
                PurchaseDateHeader = "Purchase Date",
                GoldLabel = "Gold",
                SilverLabel = "Silver",
                BronceLabel = "Bronze",
                PlatinumLabel = "Platinum",
                PalladiumLabel = "Palladium",
                BullionLabel = "Bullion",
                SemiNumismaticLabel = "Semi-numismatic",
                NumismaticLabel = "Numismatic"
            };

        [TestMethod]
        public void ExportHoldings_WritesOneLine_PerHolding_AndNoHeader()
        {
            var holdings = GetSampleHoldings();

            CsvExportService.ExportHoldings(holdings, _tempFile);

            var lines = File.ReadAllLines(_tempFile);
            Assert.HasCount(1, lines);
        }

        [TestMethod]
        public void ExportHoldings_WritesTechnicalFormat_IncludingEnumInts_AndIsoDate()
        {
            var holdings = GetSampleHoldings();

            CsvExportService.ExportHoldings(holdings, _tempFile);

            var line = File.ReadAllLines(_tempFile).Single();

            // Format: MetalType(int);Form;CollectableType(int);Purity;Weight;Quantity;PurchasePrice;yyyy-MM-dd
            Assert.AreEqual("0;Bar;0;999,9;10,5;2;1000;2024-01-01", line);
        }

        [TestMethod]
        public void ExportHoldings_WritesMultipleHoldings_AsMultipleLines()
        {
            var holdings = GetSampleHoldings();
            holdings.Add(new MetalHolding
            {
                MetalType = MetalType.Silver,
                Form = "Coin",
                CollectableType = CollectableType.Numismatic,
                Purity = 900m,
                Weight = 31.1m,
                Quantity = 1,
                PurchasePrice = 123.45m,
                PurchaseDate = new DateTime(2023, 12, 31)
            });

            CsvExportService.ExportHoldings(holdings, _tempFile);

            var lines = File.ReadAllLines(_tempFile);       
            Assert.HasCount(2, lines);  
            Assert.AreEqual("0;Bar;0;999,9;10,5;2;1000;2024-01-01", lines[0]);
            Assert.AreEqual("1;Coin;2;900;31,1;1;123,45;2023-12-31", lines[1]);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_WritesHeaderRow_FromProvidedTexts()
        {
            var holdings = GetSampleHoldings();
            var texts = CreateDetailedExportTexts();

            CsvExportService.ExportHoldingsDetailed(holdings, _tempFile, texts);

            var lines = File.ReadAllLines(_tempFile);
            Assert.HasCount(2, lines);

            Assert.AreEqual(
                "Metal Type; Form; Collectable Type; Purity; Weight; Quantity; Purchase Price; Purchase Date; ",
                lines[0]);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_WritesLocalizedValues_UsingProvidedTexts()
        {
            var holdings = GetSampleHoldings();
            var texts = CreateDetailedExportTexts();

            CsvExportService.ExportHoldingsDetailed(holdings, _tempFile, texts);

            var dataLine = File.ReadAllLines(_tempFile)[1];

            StringAssert.Contains(dataLine, "Gold; ");
            StringAssert.Contains(dataLine, "Bullion; ");
            StringAssert.Contains(dataLine, "1000.00; ");
            StringAssert.Contains(dataLine, "01.01.2024; ");
        }

        [TestMethod]
        public void ExportHoldings_WithNoHoldings_WritesEmptyFile()
        {
            CsvExportService.ExportHoldings(Array.Empty<MetalHolding>(), _tempFile);

            var content = File.ReadAllText(_tempFile);

            Assert.AreEqual(string.Empty, content);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_WithNoHoldings_WritesOnlyHeaderRow()
        {
            var texts = CreateDetailedExportTexts();

            CsvExportService.ExportHoldingsDetailed(Array.Empty<MetalHolding>(), _tempFile, texts);

            var lines = File.ReadAllLines(_tempFile);

            Assert.HasCount(1, lines);
            Assert.AreEqual(
                "Metal Type; Form; Collectable Type; Purity; Weight; Quantity; Purchase Price; Purchase Date; ",
                lines[0]);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_WritesExpectedDetailedLine_ForSilverNumismaticHolding()
        {
            var holding = new MetalHolding
            {
                MetalType = MetalType.Silver,
                Form = "Coin",
                CollectableType = CollectableType.Numismatic,
                Purity = 900m,
                Weight = 31.1m,
                Quantity = 1,
                PurchasePrice = 123.45m,
                PurchaseDate = new DateTime(2023, 12, 31)
            };

            var holdings = new List<MetalHolding> { holding };
            var texts = CreateDetailedExportTexts();

            CsvExportService.ExportHoldingsDetailed(holdings, _tempFile, texts);

            var lines = File.ReadAllLines(_tempFile);

            Assert.HasCount(2, lines);

            var expected =
                $"{texts.SilverLabel}; " +
                $"{holding.Form}; " +
                $"{texts.NumismaticLabel}; " +
                $"{holding.Purity}; " +
                $"{holding.Weight}; " +
                $"{holding.Quantity}; " +
                $"{holding.PurchasePrice.ToString("F2", CultureInfo.InvariantCulture)}; " +
                $"{holding.PurchaseDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}; ";

            Assert.AreEqual(expected, lines[1]);
        }
    }
}