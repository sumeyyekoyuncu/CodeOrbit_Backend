# 🚀 CodeOrbit — Backend API

CodeOrbit is a backend API for a mobile coding education platform. Users can learn programming through quizzes, daily challenges, and track their progress with streaks, badges, and leaderboards.

---

## ✨ Features

- 📚 **Question & Quiz System** — Category-based questions with multiple options, quiz start/complete flow
- 🏆 **Daily Challenges** — Daily coding questions with attempt tracking
- 📈 **Progress Tracking** — Per-user progress, streaks, and activity history
- 🥇 **Leaderboard** — Global and friend-based rankings
- 👥 **Friends System** — Send/accept friend requests, view friends' progress
- 🎖️ **Badges** — Achievement badges awarded based on user activity
- ⭐ **Favorites** — Save and revisit favorite questions
- 🔔 **Notifications** — In-app notification system
- 📊 **Statistics** — Detailed user performance analytics
- 🔐 **Authentication** — JWT-based auth

---

## 🏗️ Architecture

The project follows **Clean Architecture**, ensuring each layer is independent and testable.

```
CodeOrbit/
├── CodeOrbit.API             # Controllers, entry point
├── CodeOrbit.Application     # Interfaces, DTOs (no external dependencies)
├── CodeOrbit.Domain          # Entities, Enums (pure business model)
├── CodeOrbit.Infrastructure  # Service implementations, EF Core, Migrations
└── CodeOrbit.Tests           # Unit tests
```

**Dependency rule:** Domain → Application → Infrastructure → API. Outer layers depend on inner layers, never the other way around.

---

## 🛠️ Tech Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core | Web API framework |
| Entity Framework Core | ORM / Database access |
| JWT | Authentication & authorization |
| xUnit | Unit testing |

---

## 📦 Domain Entities

| Entity | Description |
|---|---|
| `User` | Platform user |
| `Question` / `Option` | Questions with multiple choice options |
| `Category` | Question categories (e.g. C#, Algorithms) |
| `Quiz` / `QuizQuestion` | Quiz sessions and their questions |
| `DailyChallenge` / `DailyChallengeQuestion` | Daily coding challenges |
| `UserChallengeAttempt` / `UserChallengeAnswer` | User's challenge attempts and answers |
| `UserProgress` | Per-category progress tracking |
| `UserStreak` | Daily usage streak |
| `UserActivity` | Activity history |
| `Badge` / `UserBadge` | Achievement badges |
| `FriendRequest` / `Friendship` | Social features |
| `FavoriteQuestion` | Saved questions |
| `Notification` | In-app notifications |

---

## 📡 API Endpoints

| Controller | Description |
|---|---|
| `/api/auth` | Register, login, token refresh |
| `/api/question` | List, filter, and manage questions |
| `/api/category` | Question categories |
| `/api/quiz` | Start and complete quizzes |
| `/api/challenge` | Daily challenge flow |
| `/api/userprogress` | Track learning progress |
| `/api/leaderboard` | Global and friend rankings |
| `/api/friend` | Friend requests and friendships |
| `/api/badge` | User badges and achievements |
| `/api/favorite` | Save/remove favorite questions |
| `/api/notification` | User notifications |
| `/api/statistics` | User performance stats |
| `/api/activity` | Activity history |

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server 

### Run Locally

```bash
git clone https://github.com/sumeyyekoyuncu/CodeOrbit_Backend.git
cd CodeOrbit_Backend
```

Update `appsettings.json` with your configuration:

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

```bash
dotnet restore
dotnet ef database update --project CodeOrbit.Infrastructure --startup-project CodeOrbit.API
dotnet run --project CodeOrbit.API
```

---

## 🧪 Tests

```bash
dotnet test
```


