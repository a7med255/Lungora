# Lungora

**Lungora** is a modular .NET Core project structured following the Clean Architecture pattern.  
This design keeps the codebase maintainable, testable, and scalable by separating concerns into distinct layers.
Live Demo for Hosting:https://lungora.runasp.net/swagger/index.html
Link Repo of Mobile:https://github.com/omarAbdullahMoharam/Lungora
Link Repo of Web DashBoard:https://github.com/AhmedNabil2003/Dashboard_Lungora

## 📦 Project Structure

```text
Lungora.sln
│
├── Lungora.API/                ← Presentation Layer
│   ├── Controllers/
│   ├── JWT/
│   └── Program.cs, appsettings.json
│
├── Lungora.Application/        ← Application Layer
│   ├── DTOs/
│   ├── Interfaces/
│
├── Lungora.Domain/             ← Domain Layer
│   ├── Models/
│   ├── Enums/
│
├── Lungora.Infrastructure/     ← Infrastructure Layer
│   ├── LungoraContext/
│   ├── Helpers/
│   ├── Repositories/
│   ├── Services/
│   └── ExternalServices/

🧩 Layers Explained
Presentation (Lungora.API):
Contains controllers, middleware, authentication (JWT), and API configuration.

Application (Lungora.Application):
Defines business logic interfaces, use cases, and Data Transfer Objects (DTOs).

Domain (Lungora.Domain):
Includes core entities, enums, and business rules. This layer has no dependencies on other layers.

Infrastructure (Lungora.Infrastructure):
Contains implementations for data access (EF Core), external services integration, and helper classes.
