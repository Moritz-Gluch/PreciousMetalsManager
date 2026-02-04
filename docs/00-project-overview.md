# Project Overview:


## Goal
Develop a desktop application to **manage personal precious metal holdings** (gold, silver, platinum, etc.), including:
- Recording purchase prices and quantities
- Calculating current market value and profit/loss
- Displaying historical data and charts
- Exporting portfolio data to CSV/Excel
- Providing a modern, user-friendly interface for long-term use


## Problem Statement
Many private investors lose track of their precious metal holdings. 
This project aims to clearly record and display purchase prices, current market values, and historical data.


## Target Audience
- Personal use


## Scope

### In Scope (MVP)
- CRUD operations for precious metal holdings
- Display holdings in a table (DataGrid)
- Calculate current value based on stored prices
- Local storage via SQLite

### Future / Optional Features
Future and optional features are now tracked in the [GitHub Issues](https://github.com/Moritz-Gluch/PreciousMetalsManager/issues) page.

### Out of Scope
- Multi-user support or cloud syncing
- Mobile or web versions (desktop-only)
- Complex financial analytics beyond basic profit/loss
- Real-time market alerts
- Multi-currency or tax calculations


## Technologies & Tools
|      Area       |         Technology / Library         |
|-----------------|--------------------------------------|
| Frontend / UI   | C# WPF, MahApps.Metro or MaterialDesignInXAML |
| Backend / Logic | C#                                   |                                          
| Database        | SQLite                               |
| Charts          | LiveCharts2 or OxyPlot               |
| API             | HttpClient (REST)                    |
| Tools           | Visual Studio / VS Code, Git, GitHub |


## Planned Folder Structure
PreciousMetalsManager/
│
├─ docs/                   
├─ tests/                  
├─ README.md
├─ .gitignore
│
└─ PreciousMetalsManager/  
   ├─ Models/              
   ├─ Views/               
   ├─ ViewModel/           
   ├─ Services/            
   ├─ Resources/           
   ├─ App.xaml             
   └─ MainWindow.xaml