CREATE TABLE IF NOT EXISTS ratings (
    id SERIAL PRIMARY KEY,
    media_id INTEGER NOT NULL REFERENCES media_entries(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    stars INTEGER NOT NULL CHECK (stars >= 1 AND stars <= 5),
    comment TEXT,
    is_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    like_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    UNIQUE(media_id, user_id)
);

CREATE INDEX IF NOT EXISTS idx_ratings_media ON ratings(media_id);
CREATE INDEX IF NOT EXISTS idx_ratings_user ON ratings(user_id);
CREATE INDEX IF NOT EXISTS idx_ratings_confirmed ON ratings(is_confirmed);

CREATE TABLE IF NOT EXISTS rating_likes (
    rating_id INTEGER NOT NULL REFERENCES ratings(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    PRIMARY KEY (rating_id, user_id)
);

CREATE INDEX IF NOT EXISTS idx_rating_likes_user ON rating_likes(user_id);