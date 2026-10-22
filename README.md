# Takwene Track Management

One .NET 10 API project and React/TypeScript frontend. Active path E:/TakweneTask.

## SQL Server and migrations (Step 2)
Development defaults target .\SQLEXPRESS, database TakweneTask, using Windows authentication. TrustServerCertificate=True is restricted to the development configuration; use a trusted certificate in production. Set ConnectionStrings__DefaultConnection for other environments.

From E:\TakweneTask:
~~~powershell
dotnet tool restore
dotnet ef database update --project backend/Takwene.Api -- --environment Development
dotnet user-secrets set 'Seed:DemoPassword' '<choose-a-password-of-at-least-12-characters>' --project backend/Takwene.Api
dotnet run --project backend/Takwene.Api --launch-profile http -- --seed-demo-user
sqlcmd -S '.\SQLEXPRESS' -E -C -d TakweneTask -b -i scripts/verify-persistence.sql
~~~
The demo login email is demo@example.com. Seeding creates a missing user only and does not reset an existing user's password. This workspace's initial password was randomly generated and retained in local .NET user secrets; do not commit or share the secret store. Login endpoints are planned for Step 3.
Business sample data is deterministic migration data. The demo user is seeded separately using the framework password hasher. No automatic migrations or demo-user seeding run on normal application startup.
Model annotations and database constraints are present; request DTO validation/normalization and business validation will be implemented in the API milestone.

