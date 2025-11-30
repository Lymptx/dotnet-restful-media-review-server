# DOTNET Media Ratings Platform – Intermediate Hand-in

## Overview

This project implements a **RESTful HTTP server** in C# for a media ratings platform.
The intermediate hand-in focuses on user registration, login (with token-based authentication), and basic HTTP routing. Media management, ratings, and recommendations will be implemented in the final hand-in.

The server uses a **pure HTTP listener** (`HttpListener`) without any web frameworks, fulfilling the project requirement to avoid ASP.NET or similar libraries.

---

## Design Decisions

### 1. Architecture

* **Single-responsibility handlers**: Each type of request (e.g., users, version) has its own handler class implementing `IHandler`.
* **Event-driven routing**: Incoming HTTP requests are wrapped in `HttpRestEventArgs`, which contain the method, path, body, and session info.
* **Session management**: Token-based authentication is handled through a `Session` class, allowing future expansion for access control.
* **Data models**: Core entities (`User`, `MediaEntry`, `Rating`) inherit from the abstract `Atom` class to enforce a consistent interface for CRUD operations.

---

### 2. Class Structure

* **Atom (abstract)**
  Base class for all domain objects. Provides:

  * Session verification (`_VerifySession`)
  * Admin/owner access checks
  * Abstract methods: `Save()`, `Delete()`, `Refresh()`

* **User**
  Represents a platform user.
  Responsibilities:

  * Store username, password hash, full name, and email
  * Registration and login logic
  * Password hashing and verification
  * Token association via sessions

* **HttpRestEventArgs**
  Wraps incoming HTTP requests, providing:

  * HTTP method, path, and body
  * Parsed JSON content
  * Helper methods to respond with proper status codes

* **UserHandler**
  Handles `/users` requests:

  * POST `/users` → create a new user
  * Performs JSON deserialization and error handling
  * Sends appropriate HTTP response codes

* **VersionHandler** (example)
  Returns current API version for testing connectivity.

---

### 3. Routing and HTTP Design

Routing is implemented by **checking `e.Path` and `e.Method`** in each handler.

* Example:

```csharp
if (e.Path.StartsWith("/users") && e.Method == HttpMethod.Post)
{
    // create user
}
```

Invalid paths or methods respond with `400 Bad Request`.

---

### 4. SOLID Principles

* **Single Responsibility**: Each handler is responsible for one resource type.
* **Open/Closed**: New handlers can be added without modifying existing ones.
* **Liskov Substitution**: `Atom` allows any entity (User, MediaEntry, Rating) to be managed uniformly.
* **Interface Segregation**: `IHandler` ensures only required methods are implemented.
* **Dependency Inversion**: Core logic is independent of HTTP specifics; `HttpRestEventArgs` abstracts the request/response.

---

### 5. Testing

* **Integration**:

  * Tested using `curl` commands to create users, login, and fetch version info.
* **Unit Tests**:

  * Planned for session validation, password hashing, and user creation logic.

---

### 6. Git Repository

https://github.com/Lymptx/dotnet-restful-media-review-server