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

        private static void AssertThrows<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail($"Expected exception '{typeof(TException).Name}' was not thrown.");
            }
            catch (TException)
            {
                // expected
            }
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
        public void ExportHoldingsDetailed_WritesHeaderRow()
        {
            App.SetLanguage("en");
            var holdings = GetSampleHoldings();

            CsvExportService.ExportHoldingsDetailed(holdings, _tempFile);

            var lines = File.ReadAllLines(_tempFile);
            Assert.HasCount(2, lines);

            string L(string key) => Application.Current?.TryFindResource(key) as string ?? key;
            var expectedHeader = $"{L("Common_MetalType")}; {L("Common_Form")}; {L("Common_CollectableType")}; {L("Common_Purity")}; {L("Common_Weight")}; {L("Common_Quantity")}; {L("Common_PurchasePrice")}; {L("Common_PurchaseDate")}; ";
            Assert.AreEqual(expectedHeader, lines[0]);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_WritesLocalizedHeaders_DE()
        {
            App.SetLanguage("de");
            var holdings = GetSampleHoldings();

            CsvExportService.ExportHoldingsDetailed(holdings, _tempFile);

            string L(string key) => Application.Current?.TryFindResource(key) as string ?? key;
            var expectedHeader = $"{L("Common_MetalType")}; {L("Common_Form")}; {L("Common_CollectableType")}; {L("Common_Purity")}; {L("Common_Weight")}; {L("Common_Quantity")}; {L("Common_PurchasePrice")}; {L("Common_PurchaseDate")}; ";
            var header = File.ReadAllLines(_tempFile)[0];
            Assert.AreEqual(expectedHeader, header);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_EmptyHoldings_WritesOnlyHeader()
        {
            App.SetLanguage("en");

            CsvExportService.ExportHoldingsDetailed(Array.Empty<MetalHolding>(), _tempFile);

            string L(string key) => Application.Current?.TryFindResource(key) as string ?? key;
            var expectedHeader = $"{L("Common_MetalType")}; {L("Common_Form")}; {L("Common_CollectableType")}; {L("Common_Purity")}; {L("Common_Weight")}; {L("Common_Quantity")}; {L("Common_PurchasePrice")}; {L("Common_PurchaseDate")}; ";

            var lines = File.ReadAllLines(_tempFile);
            Assert.HasCount(1, lines);
            Assert.AreEqual(expectedHeader, lines[0]);
        }

        [TestMethod]
        public void ExportHoldingsDetailed_ThrowsOnInvalidPath()
        {
            App.SetLanguage("en");
            var holdings = GetSampleHoldings();

            AssertThrows<DirectoryNotFoundException>(() =>
                CsvExportService.ExportHoldingsDetailed(holdings, @"Z:\definitely\not\a\real\path\file.csv"));
        }

        [TestMethod]
        public void ExportHoldings_ThrowsOnInvalidPath()
        {
            var holdings = GetSampleHoldings();

            AssertThrows<DirectoryNotFoundException>(() =>
                CsvExportService.ExportHoldings(holdings, @"Z:\definitely\not\a\real\path\file.csv"));
        }
    }
}