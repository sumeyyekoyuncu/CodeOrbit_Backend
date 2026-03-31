# CodeOrbit Backend

A scalable REST API powering the CodeOrbit mobile coding education platform. Users progress through structured quizzes, daily challenges, and a social learning environment backed by streaks, badges, and leaderboards.

---

## Features

| Domain | Capabilities |
|---|---|
| **Quiz & Questions** | Category-based question bank, multi-option answers, quiz session management |
| **Daily Challenges** | Rotating daily coding problems with per-user attempt tracking |
| **Progress & Streaks** | Per-category progress, daily usage streaks, activity history |
| **Leaderboard** | Global rankings and friend-based competitive views |
| **Social** | Friend requests, friendship management, friends' progress visibility |
| **Badges** | Achievement system with activity-based badge awards |
| **Favorites** | Save and revisit questions across sessions |
| **Notifications** | In-app notification delivery and management |
| **Statistics** | Detailed performance analytics per user |
| **Authentication** | JWT-based registration, login, and token refresh |

---

## Architecture

CodeOrbit follows **Clean Architecture** principles, ensuring each layer is independent, testable, and has a clear single responsibility.

```
CodeOrbit/
├── CodeOrbit.API             # HTTP controllers and application entry point
├── CodeOrbit.Application     # Use case interfaces, DTOs, and abstractions
├── CodeOrbit.Domain          # Core entities and enumerations (no external dependencies)
├── CodeOrbit.Infrastructure  # EF Core, service implementations, database migrations
└── CodeOrbit.Tests           # Unit test suite
```

**Dependency rule:** `Domain → Application → Infrastructure → API`

Outer layers depend on inner layers. Inner layers have no knowledge of the layers above them.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core | Web API framework |
| Entity Framework Core | ORM and database access |
| SQL Server | Relational data store |
| JWT | Authentication and authorization |
| xUnit | Unit testing |

---

## Domain Model

| Entity | Description |
|---|---|
| `User` | Platform user account |
| `Question` / `Option` | Multiple-choice question with answer options |
| `Category` | Question classification (e.g. C#, Algorithms) |
| `Quiz` / `QuizQuestion` | Quiz session and its associated questions |
| `DailyChallenge` / `DailyChallengeQuestion` | Daily coding challenge structure |
| `UserChallengeAttempt` / `UserChallengeAnswer` | User submissions for challenges |
| `UserProgress` | Per-category learning progress |
| `UserStreak` | Daily engagement streak tracking |
| `UserActivity` | Full activity history log |
| `Badge` / `UserBadge` | Achievement definitions and user awards |
| `FriendRequest` / `Friendship` | Social connection model |
| `FavoriteQuestion` | User-saved questions |
| `Notification` | In-app notification records |

---

## API Reference

| Controller | Endpoints |
|---|---|
| `/api/auth` | Register, login, token refresh |
| `/api/question` | List, filter, and manage questions |
| `/api/category` | Question categories |
| `/api/quiz` | Start and complete quiz sessions |
| `/api/challenge` | Daily challenge flow |
| `/api/userprogress` | Learning progress tracking |
| `/api/leaderboard` | Global and friend-based rankings |
| `/api/friend` | Friend requests and friendship management |
| `/api/badge` | User badges and achievement history |
| `/api/favorite` | Save and remove favorite questions |
| `/api/notification` | User notification management |
| `/api/statistics` | User performance analytics |
| `/api/activity` | Activity history |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server instance (local or remote)

### Local Setup

**1. Clone the repository**

```bash
git clone https://github.com/sumeyyekoyuncu/CodeOrbit_Backend.git
cd CodeOrbit_Backend
```

**2. Configure the application**

Update `appsettings.json` with your environment values:

```json
{
  "ConnectionStrings": {
    "Default": "your_connection_string"
  },
  "JwtSettings": {
    "SecretKey": "your_secret_key",
    "ExpiryMinutes": 60
  }
}
```

**3. Restore, migrate, and run**

```bash
dotnet restore
dotnet ef database update --project CodeOrbit.Infrastructure --startup-project CodeOrbit.API
dotnet run --project CodeOrbit.API
```

---

## Running Tests

```bash
dotnet test
```

Test coverage targets core application logic and domain rules. All tests are isolated from infrastructure dependencies.

---
