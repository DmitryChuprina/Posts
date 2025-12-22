CREATE TABLE posts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    
    content varchar(4096),
    tags text[] DEFAULT '{}'::text[],
    
    reply_for_id uuid REFERENCES posts(id) ON DELETE SET NULL,
    repost_id uuid REFERENCES posts(id) ON DELETE SET NULL,
    depth integer DEFAULT 0 NOT NULL,
    
    likes_count integer DEFAULT 0 NOT NULL,
    views_count integer DEFAULT 0 NOT NULL,
    
    created_by uuid,
    created_at timestamp with time zone DEFAULT (now() at time zone 'utc'),
    updated_by uuid,
    updated_at timestamp with time zone,

    row_version integer DEFAULT 0 NOT NULL
);

CREATE INDEX idx_posts_reply_for_id ON posts(reply_for_id);
CREATE INDEX idx_posts_repost_id ON posts(repost_id);
CREATE INDEX idx_posts_created_by ON posts(created_by);
CREATE INDEX idx_posts_tags ON posts USING GIN (tags);