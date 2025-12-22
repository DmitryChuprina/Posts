CREATE TABLE post_likes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id uuid NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id uuid NOT NULL,
    liked_at timestamp with time zone DEFAULT (now() at time zone 'utc') NOT NULL,
    row_version integer DEFAULT 0 NOT NULL,
    CONSTRAINT uq_post_likes_post_user UNIQUE (post_id, user_id)
);

CREATE INDEX idx_post_likes_user_id ON post_likes (user_id);