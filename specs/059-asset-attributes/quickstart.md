# Quickstart: Asset Attributes

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- SQL Server LocalDB or Docker

## Setup

```bash
# Restore and build
dotnet restore
dotnet build

# Run database migration
dotnet ef database update --project src/Infrastructure --startup-project src/Web

# Start the application
dotnet run --project src/AppHost
```

## Verify Backend

```bash
# Run unit tests
dotnet test tests/Application.UnitTests/

# Run functional tests
dotnet test tests/Application.FunctionalTests/
```

## Verify Frontend

```bash
cd src/Web/ClientApp
npm install
npm run dev
```

## Smoke Test Flow

1. Navigate to "تعريفات الخصائص" → Create a definition (Code: COLOR, Name: اللون, Type: Text)
2. Navigate to "مجموعات الأصول" → Open a group → Click "إدارة الخصائص" → Bind COLOR (Required: yes)
3. Navigate to "سجل الأصول" → Create an asset under that group → See "اللون" field appear → Enter "أبيض" → Save
4. Open the asset detail → See "الخصائص" tab → Verify "اللون: أبيض" is displayed
