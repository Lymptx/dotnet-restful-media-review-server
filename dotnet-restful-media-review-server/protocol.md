# Media Ratings Platform

A REST API server in C# for rating movies, TV shows, games, and books. Users can register, login, create media entries, rate them, and get personalized recommendations.

## What it does

- User registration and login with token authentication
- Create and manage media entries (movies, shows, games, etc.)
- Rate media 1-5 stars with optional comments
- Like other people's ratings
- Mark media as favorites
- Search and filter media
- Get personalized recommendations
- Leaderboards for top media and users

## Tech Stack

- **C# / .NET 8.0**
- **PostgreSQL** (running in Docker)
- **Pure HTTP** - no ASP.NET or web frameworks
- **ADO.NET** - no ORM libraries like Entity Framework

This was a requirement to learn how HTTP and databases work at a lower level.

## Project Structure

```
├── Database/
│   ├── Schema/           # SQL files for tables
│   ├── DB.cs            # Database connection
│   └── *Repository.cs   # Database queries
├── Handlers/
│   └── *Handler.cs      # HTTP endpoint logic
├── Server/
│   ├── HttpRestServer.cs
│   └── HttpRestEventArgs.cs
├── System/
│   └── *.cs             # Models (User, Media, Rating, etc.)
└── Program.cs
```

## Running the Project

1. Start the database:
```bash
docker-compose up -d
```

2. Run the server:
```bash
dotnet run
```

Server runs on `http://localhost:12000`

## Main Endpoints

**Users:**
- `POST /users` - Register
- `POST /sessions` - Login
- `DELETE /sessions` - Logout
- `GET /profile` - Get profile and stats

**Media:**
- `GET /media` - List all
- `POST /media` - Create (auth required)
- `PUT /media/{id}` - Update (owner only)
- `DELETE /media/{id}` - Delete (owner only)
- `GET /media/search` - Search with filters

**Ratings:**
- `GET /ratings?mediaId={id}` - Get ratings
- `POST /ratings` - Create/update rating
- `POST /ratings/{id}/like` - Like a rating
- `POST /ratings/{id}/confirm` - Confirm (admin only)

**Other:**
- `GET /favorites` - Your favorites
- `POST /favorites/{mediaId}` - Add favorite
- `GET /recommendations` - Personalized suggestions
- `GET /leaderboard/media` - Top rated media
- `GET /leaderboard/users` - Most active users

## How it Works

### Routing
Each handler checks the path and method:
```csharp
if (e.Path == "/media" && e.Method == HttpMethod.Post)
{
    HandleCreateMedia(e);
}
```

Handlers are auto-discovered using reflection so you just create a new handler class and it works.

### Authentication
Login gives you a token, then you send it in the Authorization header:
```
Authorization: Bearer <your-token>
```

The token is checked in `e.Session` and expires after 30 minutes.

### Database
Using pure ADO.NET with parameterized queries to prevent SQL injection:
```csharp
new NpgsqlParameter("@username", username)
```

No ORM like Dapper or Entity Framework - all SQL is written manually.

### Security
- Passwords are hashed with SHA256 + unique salt per user
- SQL injection prevented with parameters
- Only owners can edit/delete their media
- Comments need admin approval before showing publicly

## Design Patterns

- **Repository pattern** - separates database logic from handlers
- **Handler pattern** - each resource type has its own handler
- **Token-based auth** - stateless authentication

## Business Logic

**Rating System:**
- 1-5 stars required
- One rating per user per media
- Can edit your rating anytime
- Comments need admin confirmation

**Recommendations:**
Based on what you rated highly (4-5 stars). Looks at:
- Genres you like
- Media you haven't rated yet
- Popular stuff (high ratings)

**Leaderboard:**
- Top media needs at least 3 ratings
- Users ranked by ratings + media created

## Testing

Run tests:
```bash
dotnet test
```

Tests cover:
- Password hashing
- Rating validation (1-5 stars)
- Search filters
- User authentication
- Business rules

## Database Schema

Tables:
- `users` - user accounts
- `media_entries` - movies, shows, games, etc.
- `ratings` - user ratings with comments
- `rating_likes` - which users liked which ratings
- `favorites` - user's favorite media
- `sessions` - active login sessions

All tables have proper foreign keys and indexes.

## Lessons learned

- How HTTP works without frameworks
- SQL and preventing injection
- Token authentication
- Password security (hashing + salts)
- REST API design
- Database normalization
