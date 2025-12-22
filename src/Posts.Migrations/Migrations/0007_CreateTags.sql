CREATE TABLE tags (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text NOT NULL,
    
    usage_count integer DEFAULT 0 NOT NULL,
    last_used_at timestamp with time zone DEFAULT (now() at time zone 'utc') NOT NULL,

    row_version integer DEFAULT 0 NOT NULL,
    CONSTRAINT uq_tags_name UNIQUE (name)
);