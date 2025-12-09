CREATE TABLE IF NOT EXISTS users
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    username varchar(255) NOT NULL UNIQUE,
    email varchar(255) NOT NULL UNIQUE,
    password varchar(255) NOT NULL,
    
    role SMALLINT NOT NULL,

    first_name varchar(255) NULL,
    last_name varchar(255) NULL,
    description TEXT NULL,
    profile_image_url varchar(1024) NULL,

    email_is_confirmed BOOLEAN NOT NULL DEFAULT FALSE,

    blocked_at TIMESTAMP NULL,
    block_reason TEXT NULL,

    created_by UUID NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_by UUID NULL,
    updated_at TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);