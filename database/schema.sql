-- VacayVision Database Schema
-- PostgreSQL 14+
-- Last Updated: January 30, 2026
-- 
-- This file contains the complete database schema for the VacayVision application.
-- It includes all tables, indexes, and constraints used by the ASP.NET Core application.

-- =============================================================================
-- ROLES TABLE
-- =============================================================================
-- Stores user permission levels
-- 0 = Regular user, 1 = Moderator, 2 = Admin
CREATE TABLE IF NOT EXISTS roles (
    role_id INTEGER PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL UNIQUE
);

-- Insert default roles
INSERT INTO roles (role_id, role_name) VALUES 
    (0, 'user'),
    (1, 'moderator'), 
    (2, 'admin')
ON CONFLICT (role_id) DO NOTHING;

-- =============================================================================
-- USERS TABLE
-- =============================================================================
-- Stores user account information including email, hashed password, and role
CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    bio TEXT,
    password VARCHAR(255) NOT NULL,
    role_id INTEGER DEFAULT 0 REFERENCES roles(role_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for users table
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);

-- =============================================================================
-- VACATIONS TABLE
-- =============================================================================
-- Stores vacation information created by users
-- Note: Status is tracked in community_requests table, not here
CREATE TABLE IF NOT EXISTS vacations (
    vacation_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    destination VARCHAR(255) NOT NULL,
    description TEXT,
    image_url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for vacations table
CREATE INDEX IF NOT EXISTS idx_vacations_user_id ON vacations(user_id);
CREATE INDEX IF NOT EXISTS idx_vacations_created_at ON vacations(created_at DESC);

-- =============================================================================
-- VACATION_COMMENTS TABLE
-- =============================================================================
-- Stores comments on vacation posts
CREATE TABLE IF NOT EXISTS vacation_comments (
    comment_id SERIAL PRIMARY KEY,
    vacation_id INTEGER NOT NULL REFERENCES vacations(vacation_id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for vacation_comments table
CREATE INDEX IF NOT EXISTS idx_vacation_comments_vacation_id ON vacation_comments(vacation_id);
CREATE INDEX IF NOT EXISTS idx_vacation_comments_user_id ON vacation_comments(user_id);
CREATE INDEX IF NOT EXISTS idx_vacation_comments_created_at ON vacation_comments(created_at DESC);

-- =============================================================================
-- VACATION_WIDGETS TABLE
-- =============================================================================
-- Stores additional content widgets attached to vacations
-- Types: "video", "quote", "product", "location", etc.
CREATE TABLE IF NOT EXISTS vacation_widgets (
    vacation_widget_id SERIAL PRIMARY KEY,
    vacation_id INTEGER NOT NULL REFERENCES vacations(vacation_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    type VARCHAR(100) NOT NULL,
    content TEXT NOT NULL,
    external_url TEXT,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for vacation_widgets table
CREATE INDEX IF NOT EXISTS idx_vacation_widgets_vacation_id ON vacation_widgets(vacation_id);
CREATE INDEX IF NOT EXISTS idx_vacation_widgets_sort_order ON vacation_widgets(sort_order);

-- =============================================================================
-- COMMUNITY_REQUESTS TABLE
-- =============================================================================
-- Stores vacation submission requests for community approval
-- Status values: 'pending', 'approved', 'rejected'
CREATE TABLE IF NOT EXISTS community_requests (
    request_id SERIAL PRIMARY KEY,
    vacation_id INTEGER NOT NULL REFERENCES vacations(vacation_id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    submitted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    reviewed_at TIMESTAMP,
    reviewed_by INTEGER REFERENCES users(user_id),
    rejection_reason TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for community_requests table
CREATE INDEX IF NOT EXISTS idx_community_requests_vacation_id ON community_requests(vacation_id);
CREATE INDEX IF NOT EXISTS idx_community_requests_user_id ON community_requests(user_id);
CREATE INDEX IF NOT EXISTS idx_community_requests_status ON community_requests(status);
CREATE INDEX IF NOT EXISTS idx_community_requests_submitted_at ON community_requests(submitted_at DESC);

-- =============================================================================
-- HORIZON_HEADLINES TABLE
-- =============================================================================
-- Stores admin announcement messages displayed on community page
CREATE TABLE IF NOT EXISTS horizon_headlines (
    headline_id SERIAL PRIMARY KEY,
    message TEXT NOT NULL,
    created_by INTEGER NOT NULL REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP
);

-- Indexes for horizon_headlines table
CREATE INDEX IF NOT EXISTS idx_horizon_headlines_created_by ON horizon_headlines(created_by);
CREATE INDEX IF NOT EXISTS idx_horizon_headlines_expires_at ON horizon_headlines(expires_at);
CREATE INDEX IF NOT EXISTS idx_horizon_headlines_created_at ON horizon_headlines(created_at DESC);

-- =============================================================================
-- SUPPORT_FEEDBACK TABLE
-- =============================================================================
-- Stores user support feedback and issue reports
CREATE TABLE IF NOT EXISTS support_feedback (
    support_id SERIAL PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    message TEXT NOT NULL,
    submitted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    submitted_by INTEGER NOT NULL REFERENCES users(user_id)
);

-- Indexes for support_feedback table
CREATE INDEX IF NOT EXISTS idx_support_feedback_submitted_by ON support_feedback(submitted_by);
CREATE INDEX IF NOT EXISTS idx_support_feedback_type ON support_feedback(type);
CREATE INDEX IF NOT EXISTS idx_support_feedback_submitted_at ON support_feedback(submitted_at DESC);

-- =============================================================================
-- NOTES ON SCHEMA
-- =============================================================================
-- 
-- 1. All primary keys use SERIAL (auto-incrementing integer)
-- 2. All tables include created_at timestamp for audit trail
-- 3. Foreign key constraints use ON DELETE CASCADE where appropriate
-- 4. Indexes are created on commonly queried columns for performance
-- 5. Status tracking for vacations is in community_requests, not vacations table
-- 6. Passwords in users table are stored as bcrypt hashes
-- 7. Sessions are handled by ASP.NET Core middleware, no sessions table needed
-- 
-- TIMESTAMP HANDLING:
-- - Database columns use TIMESTAMP (without time zone) but store UTC values
-- - Application layer (C# models) uses DateTime.UtcNow for all timestamp generation
-- - PostgreSQL CURRENT_TIMESTAMP defaults to server time (should be UTC)
-- - Future migration: Consider TIMESTAMPTZ for explicit UTC storage
-- 
-- TABLE NAMING:
-- - horizon_headlines table is legacy naming from Node.js app
-- - TODO: Rename to 'headlines' at end of migration for simplicity
-- - This will require updating: Models/Headline.cs, AppDbContext.cs, all queries
-- 
-- =============================================================================
-- MIGRATION NOTES
-- =============================================================================
-- 
-- If migrating from Node.js version:
-- 1. Remove 'status' column from vacations table if it exists:
--    ALTER TABLE vacations DROP COLUMN IF EXISTS status;
-- 
-- 2. Add created_at and updated_at to community_requests if missing:
--    ALTER TABLE community_requests ADD COLUMN IF NOT EXISTS created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
--    ALTER TABLE community_requests ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
-- 
-- 3. Ensure all indexes are created for optimal query performance
-- 
-- 4. Verify PostgreSQL server timezone is set to UTC:
--    SHOW timezone;  -- Should return 'UTC'
--    ALTER DATABASE postgres SET timezone TO 'UTC';  -- If needed
-- 
-- =============================================================================
