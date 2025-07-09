
-- Ensure the Users table exists before inserting, or handle potential duplicates
-- This script assumes your migrations will create the Users table.
-- If running this script before migrations, you might need to add CREATE TABLE IF NOT EXISTS statements.

INSERT INTO "Users" ("Email", "PasswordHash")
VALUES (
    'test@test.com',
    '$2a$11$o52jdFghKYlsM0k.DpfSEe16E50l6thIxP4dX4.CUL7M1nu.cVsEK'
)
ON CONFLICT ("Email") DO NOTHING; -- Prevents insertion if email already exists
