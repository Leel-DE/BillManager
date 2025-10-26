# BillManager

## Project Overview
- Desktop application built with C# (.NET 8), WPF, and MVVM.
- Enables users to create, validate, and persist bill records in JSON or plain text format.
- Provides instant feedback for validation issues and supports viewing the most recently saved bill.

## Validation Rules
- **Description**: 3–20 characters, trimmed input.
- **Price**: Numeric value greater than 0 (decimal separators from current culture or `.`).
- **Items**: Whole number between 0 and 5 inclusive.
- **Filename**: 1–10 characters, alphanumeric or underscore (`^[A-Za-z0-9_]+$`).

## How to Run
1. Open Visual Studio 2022 (17.8+) with .NET 8 SDK installed.
2. Select `File` → `Open` → `Project/Solution…` and choose `BillManager.sln` from the repository root.
3. Ensure `BillManager` is the startup project (bold in Solution Explorer).
4. Press `Ctrl+Shift+B` to build, then `F5` or `Ctrl+F5` to run the application.

## Folder Structure
```
BillManager.sln
BillManager/
├── App.xaml
├── App.xaml.cs
├── BillManager.csproj
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Models/
├── Services/
│   ├── Application/
│   ├── Messaging/
│   ├── Storage/
│   └── Validation/
├── Views/
├── ViewModels/
├── Commands/
└── README.md
```

## GitFlow & Commit Rules
- Branch flow: work on `feature/*` branches → merge into `develop` → merge into `main` when stable.
- Use Conventional Commits (`feat:`, `fix:`, `chore:`, etc.) for every commit.
- Local workflow example:
  ```bash
  git checkout develop
  git checkout -b feature/create-bill-app
  git add .
  git commit -m "feat: implement bill management application"
  git checkout develop
  git merge feature/create-bill-app
  ```
