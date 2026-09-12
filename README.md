# SecureCheck

QR-based identity verification for exam admit cards — Team Alpha Beats, B.Sc. IT,
Graphic Era University, Dehradun (PC#N-V-2026-T003).

This is the starting file structure for the project described in the proposal and
architecture diagram. It compiles conceptually against .NET 8; a few pieces are
marked `TODO` where the team still needs to make a decision (mainly invigilator
device authentication).

## Solution layout

```
SecureCheck/
├── SecureCheck.sln
├── src/
│   ├── SecureCheck.Core/              Domain models, DTOs, interfaces — no dependencies
│   │   ├── Entities/                  Student, ExamRegistration, VerificationLog
│   │   ├── Enums/                     VerificationStatus
│   │   ├── DTOs/                      Request/response shapes for the API
│   │   └── Interfaces/                IQrCodeService, IAdmitCardService, IRegistrationRepository
│   │
│   ├── SecureCheck.Infrastructure/    Implementations: EF Core, QRCoder, QuestPDF
│   │   ├── Data/                      SecureCheckDbContext
│   │   ├── Repositories/              RegistrationRepository
│   │   └── Services/                  QrCodeService, AdmitCardPdfService
│   │
│   ├── SecureCheck.Api/               ASP.NET Core Web API — Registration Module + Verification API
│   │   └── Controllers/               RegistrationController, VerificationController
│   │
│   └── SecureCheck.InvigilatorClient/ Razor Pages app — the browser client invigilators use to scan
│       ├── Pages/                     Scan.cshtml (camera scan), Result.cshtml (verdict)
│       └── wwwroot/                   JS (QR scanning + fetch to the API) and CSS
│
├── tests/
│   └── SecureCheck.Tests/             xUnit tests (starting with QrCodeService)
│
└── docs/                              Architecture diagram, notes
```

This maps directly onto the architecture diagram:

| Diagram box              | Where it lives                                            |
|---------------------------|-------------------------------------------------------------|
| Registration Module       | `SecureCheck.Api/Controllers/RegistrationController.cs`     |
| Database                  | `SecureCheck.Infrastructure/Data`, `Repositories`            |
| QR Generation Service     | `SecureCheck.Infrastructure/Services/QrCodeService.cs`       |
| Admit Card Generator      | `SecureCheck.Infrastructure/Services/AdmitCardPdfService.cs` |
| Invigilator Client        | `SecureCheck.InvigilatorClient`                              |
| Verification API          | `SecureCheck.Api/Controllers/VerificationController.cs`      |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Internet access the first time you build, so NuGet can restore: `QRCoder`, `QuestPDF`,
  `Microsoft.EntityFrameworkCore.Sqlite`, `Swashbuckle.AspNetCore`, `xunit`.

## Running it

```bash
# from the SecureCheck/ folder
dotnet restore
dotnet build

# run the API (registration + verification endpoints, Swagger at /swagger)
dotnet run --project src/SecureCheck.Api

# in a second terminal, run the invigilator client
dotnet run --project src/SecureCheck.InvigilatorClient
```

The SQLite database (`securecheck.db`) is created automatically on first run in
`src/SecureCheck.Api/`, and EF Core startup migration is applied automatically.
Generated admit-card PDFs are saved under `src/SecureCheck.Api/wwwroot/admitcards/`.

## Next steps for the team

1. **EF Core migrations** — run `dotnet ef migrations add <Name>` from
   `SecureCheck.Api` (needs `dotnet-ef` tool installed) when schema changes are made.
2. **Invigilator device authentication** — the `TODO` in `Program.cs` and
   `VerificationController.cs`. Decide between a per-device API key or a JWT
   issued at login, per the proposal's "restricted to authorized invigilator
   devices" objective.
3. **Wire the invigilator client's HTTP call** — `scanner.js` currently fetches
   the verification URL directly from the QR payload; once auth is added, this
   needs to go through the authenticated `HttpClient` registered in
   `InvigilatorClient/Program.cs` instead.
4. **Seed test data** for demoing (a few sample students/registrations) so the
   scan-and-verify flow can be shown end-to-end without manual registration first.

## Team

Alpha Beats — Somnath Poudel (Lead), Abishek Prasad Lekhak, Krishna Kashyap, Jassi
Mentor: Mr. Ravi Raushan
