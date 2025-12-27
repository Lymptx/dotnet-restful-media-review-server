CREATE TABLE IF NOT EXISTS media_entries (
    id SERIAL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    media_type VARCHAR(50) NOT NULL,
    genre VARCHAR(100) NOT NULL,
    release_year INTEGER NOT NULL,
    age_restriction INTEGER NOT NULL DEFAULT 0,
    description TEXT,
    creator_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_media_title ON media_entries(title);
CREATE INDEX IF NOT EXISTS idx_media_type ON media_entries(media_type);
CREATE INDEX IF NOT EXISTS idx_media_genre ON media_entries(genre);
CREATE INDEX IF NOT EXISTS idx_media_creator ON media_entries(creator_user_id);
CREATE INDEX IF NOT EXISTS idx_media_release_year ON media_entries(release_year);