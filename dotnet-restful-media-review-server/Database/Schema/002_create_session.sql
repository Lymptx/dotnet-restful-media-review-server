-- 002_create_sessions.sql
CREATE TABLE IF NOT EXISTS sessions (
    token VARCHAR(50) PRIMARY KEY,
    username VARCHAR(50) REFERENCES users(username),
    is_admin BOOLEAN NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
