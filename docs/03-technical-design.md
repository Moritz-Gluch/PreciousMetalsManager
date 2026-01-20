# Technical Design: 

## 1. Architecture
- WPF application using MVVM pattern
- Separation of concerns:
  - Models → data representation (Metal, Portfolio)
  - ViewModels → handle business logic, UI binding, calculations
  - Views → XAML UI, DataGrids, dialogs, charts

## 2. Folder Structure
Refer to `00-project-overview.md` for the complete folder structure of the project.

## 3. Database Design (for Local Storage)
- SQLite local database
- Table: Metals
  - Id (Primary Key)
  - MetalType (string)
  - Variant (string)
  - Purity (decimal)
  - Weight (decimal)
  - Quantity (decimal)
  - PurchasePrice (decimal)
  - PurchaseDate (date)
  - CurrentValue (decimal)
  - TotalValue (decimal)

## 4. Services
- DatabaseService → handles CRUD operations to SQLite
- CalculationService → computes CurrentValue and TotalValue
- ApiService → fetches current market prices (optional)
- ExportService → CSV/Excel export

## 5. UI Design
- Main window:
  - DataGrid for holdings (columns as defined in STORY-01)
  - Add/Edit/Delete buttons
  - Filter/Sort controls (STORY-02)
- Dialog windows:
  - Add/Edit dialogs for holdings
- Charts: LiveCharts2 / OxyPlot (future enhancement)
- Styling: MahApps.Metro or MaterialDesignInXAML
