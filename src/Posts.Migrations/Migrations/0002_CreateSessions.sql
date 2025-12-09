CREATE TABLE IF NOT EXISTS sessions
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    user_id UUID NOT NULL,
    access_token VARCHAR(255) NOT NULL,
    refresh_token VARCHAR(255) NOT NULL,

    expires_at TIMESTAMP NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,

    created_by UUID NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_by UUID NULL,
    updated_at TIMESTAMP NULL,

    CONSTRAINT fk_sessions_user FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_sessions_user ON sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_sessions_refresh ON sessions(refresh_token);