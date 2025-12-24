CREATE TABLE post_views (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    
    post_id uuid NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    user_id uuid NOT NULL,
    
    first_viewed_at timestamp with time zone DEFAULT (now() at time zone 'utc') NOT NULL,
    last_viewed_at timestamp with time zone DEFAULT (now() at time zone 'utc') NOT NULL,
    
    row_version integer DEFAULT 0 NOT NULL,

    CONSTRAINT uq_post_views_post_user UNIQUE (post_id, user_id)
);

CREATE INDEX idx_post_views_user_id ON post_views (user_id);
CREATE INDEX idx_post_views_user_last_viewed ON post_views (user_id, last_viewed_at DESC);