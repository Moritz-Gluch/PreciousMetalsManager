# Technical Design – Edelmetallverwaltung

## Architektur
- MVVM-Pattern
- Layers: UI → ViewModel → Services → Repository → DB

## Datenbankmodell
| Feld | Typ | Beschreibung |
|------|-----|-------------|
| Id   | int | Primärschlüssel |
| Typ  | string | Edelmetalltyp (Gold/Silber/Platin) |
| Menge | decimal | Anzahl / Gewicht |
| Kaufpreis | decimal | Preis beim Kauf |
| Datum | DateTime | Kaufdatum |

## UI-Konzept
- Tabellenansicht für alle Edelmetalle
- Diagramme für Historie
- Buttons für CRUD-Funktionen
- Menü für Export / Optionen

## Logging & Fehlerbehandlung
- Exceptions werden geloggt
- Fehler werden dem Nutzer in einer MessageBox angezeigt
