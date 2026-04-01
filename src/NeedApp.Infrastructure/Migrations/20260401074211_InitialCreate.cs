using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enum types - check existence before creating
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'audit_action') THEN CREATE TYPE audit_action AS ENUM ('insert', 'update', 'delete'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'comment_type') THEN CREATE TYPE comment_type AS ENUM ('internal', 'external'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'missing_info_status') THEN CREATE TYPE missing_info_status AS ENUM ('pending', 'answered', 'resolved'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'notification_type') THEN CREATE TYPE notification_type AS ENUM ('missing_info', 'comment', 'status_change'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'request_status') THEN CREATE TYPE request_status AS ENUM ('draft', 'pending', 'missing_info', 'in_progress', 'done'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN CREATE TYPE user_role AS ENUM ('admin', 'staff', 'client'); END IF; END $$;");

            // Existing tables - IF NOT EXISTS (already created via SQL script)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS audit_logs (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    table_name text,
                    record_id uuid,
                    action audit_action,
                    old_data jsonb,
                    new_data jsonb,
                    changed_by uuid,
                    changed_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_audit_logs"" PRIMARY KEY (id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS clients (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    name text NOT NULL,
                    description text,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    created_by uuid,
                    updated_at timestamp with time zone,
                    updated_by uuid,
                    is_deleted boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_clients"" PRIMARY KEY (id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS users (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    email text NOT NULL,
                    name text,
                    role user_role,
                    password_hash text,
                    google_id text,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    created_by uuid,
                    updated_at timestamp with time zone,
                    updated_by uuid,
                    is_deleted boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_users"" PRIMARY KEY (id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS client_users (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    client_id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    role text,
                    created_by uuid,
                    is_deleted boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_client_users"" PRIMARY KEY (id),
                    CONSTRAINT fk_client_users_client FOREIGN KEY (client_id) REFERENCES clients(id),
                    CONSTRAINT fk_client_users_user FOREIGN KEY (user_id) REFERENCES users(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS notifications (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    user_id uuid NOT NULL,
                    title text,
                    content text,
                    type notification_type,
                    reference_id uuid,
                    is_read boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_notifications"" PRIMARY KEY (id),
                    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS requests (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    title text NOT NULL,
                    description text,
                    client_id uuid NOT NULL,
                    assigned_to uuid,
                    status request_status NOT NULL DEFAULT 'draft',
                    priority text,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    created_by uuid,
                    updated_at timestamp with time zone,
                    updated_by uuid,
                    is_deleted boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_requests"" PRIMARY KEY (id),
                    CONSTRAINT fk_requests_client FOREIGN KEY (client_id) REFERENCES clients(id),
                    CONSTRAINT fk_requests_assigned FOREIGN KEY (assigned_to) REFERENCES users(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS activity_logs (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    user_id uuid,
                    action text,
                    description text,
                    request_id uuid,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_activity_logs"" PRIMARY KEY (id),
                    CONSTRAINT fk_activity_user FOREIGN KEY (user_id) REFERENCES users(id),
                    CONSTRAINT fk_activity_request FOREIGN KEY (request_id) REFERENCES requests(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS comments (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    request_id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    content text,
                    type comment_type,
                    is_deleted boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_comments"" PRIMARY KEY (id),
                    CONSTRAINT fk_comments_request FOREIGN KEY (request_id) REFERENCES requests(id),
                    CONSTRAINT fk_comments_user FOREIGN KEY (user_id) REFERENCES users(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS missing_information (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    request_id uuid NOT NULL,
                    question text NOT NULL,
                    answer text,
                    status missing_info_status NOT NULL DEFAULT 'pending',
                    created_by uuid,
                    assigned_to uuid,
                    updated_at timestamp with time zone,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_missing_information"" PRIMARY KEY (id),
                    CONSTRAINT fk_mi_request FOREIGN KEY (request_id) REFERENCES requests(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS request_participants (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    request_id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_request_participants"" PRIMARY KEY (id),
                    CONSTRAINT fk_rp_request FOREIGN KEY (request_id) REFERENCES requests(id),
                    CONSTRAINT fk_rp_user FOREIGN KEY (user_id) REFERENCES users(id)
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS files (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    url text,
                    file_name text,
                    request_id uuid,
                    comment_id uuid,
                    uploaded_by uuid,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_files"" PRIMARY KEY (id),
                    CONSTRAINT fk_files_request FOREIGN KEY (request_id) REFERENCES requests(id),
                    CONSTRAINT fk_files_comment FOREIGN KEY (comment_id) REFERENCES comments(id)
                );");

            // NEW: Add auth columns to existing users table
            migrationBuilder.Sql("ALTER TABLE users ADD COLUMN IF NOT EXISTS password_hash text;");
            migrationBuilder.Sql("ALTER TABLE users ADD COLUMN IF NOT EXISTS google_id text;");

            // NEW: refresh_tokens table
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS refresh_tokens (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    user_id uuid NOT NULL,
                    token text NOT NULL,
                    expires_at timestamp with time zone NOT NULL,
                    revoked_at timestamp with time zone,
                    created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT ""PK_refresh_tokens"" PRIMARY KEY (id),
                    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(id)
                );");

            // Indexes - IF NOT EXISTS
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_activity_logs_request_id ON activity_logs(request_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_activity_logs_user_id ON activity_logs(user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_audit_record ON audit_logs(record_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_audit_table ON audit_logs(table_name);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_client_users_user_id ON client_users(user_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS uq_client_user ON client_users(client_id, user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_comments_request ON comments(request_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_comments_user_id ON comments(user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_files_comment_id ON files(comment_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_files_request_id ON files(request_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_missing_request ON missing_information(request_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_notifications_user ON notifications(user_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS IX_refresh_tokens_token ON refresh_tokens(token);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_refresh_tokens_user_id ON refresh_tokens(user_id);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS uq_request_user ON request_participants(request_id, user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_request_participants_user_id ON request_participants(user_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_requests_client ON requests(client_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_requests_status ON requests(status);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_requests_assigned_to ON requests(assigned_to);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS IX_users_email ON users(email);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS refresh_tokens;");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS password_hash;");
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS google_id;");
        }
    }
}
