CREATE TABLE post_media (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id uuid NOT NULL REFERENCES posts(id) ON DELETE CASCADE,

    key varchar(1024) NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,

    created_by uuid,
    created_at timestamp with time zone DEFAULT (now() at time zone 'utc'),
    updated_by uuid,
    updated_at timestamp with time zone,
    row_version integer DEFAULT 0 NOT NULL
);

CREATE INDEX idx_post_media_post_id ON post_media(post_id);
CREATE UNIQUE INDEX idx_post_media_key ON post_media(key);