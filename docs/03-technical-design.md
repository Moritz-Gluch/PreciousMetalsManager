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
- Table: Holdings
  - Id (Primary Key)
  - MetalType (integer/enum)
  - Form (string)
  - Purity (decimal)
  - Weight (decimal)
  - Quantity (integer)
  - PurchasePrice (decimal)
  - PurchaseDate (date)
  - CurrentValue / TotalValue are calculated in-memory (not persisted in DB)

## 4. Services
- LocalStorageService → handles CRUD operations to SQLite
- Calculations are currently performed in the ViewModel 
- ApiService → fetches current market prices (optional)
- ExportService → CSV/Excel export

## 5. UI Design
- Main window:
  - DataGrid for holdings (columns for metal type, form, purity, weight, quantity, purchase price, and date)
  - Add/Edit/Delete buttons
  - Filter/Sort controls
- Dialog windows:
  - Add/Edit dialogs for holdings
- Charts: LiveCharts2 / OxyPlot (future enhancement)
- Styling: MahApps.Metro or MaterialDesignInXAML
