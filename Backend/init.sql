-- Создание схем для enum типов (EF Core Npgsql использует схемы)
CREATE SCHEMA IF NOT EXISTS user_role;
CREATE SCHEMA IF NOT EXISTS session_status;
CREATE SCHEMA IF NOT EXISTS activity_verdict;
CREATE SCHEMA IF NOT EXISTS report_status_color;
CREATE SCHEMA IF NOT EXISTS activity_source_type;

-- Создание enum типов в соответствующих схемах
CREATE TYPE user_role.user_role AS ENUM ('employee', 'admin');
CREATE TYPE session_status.session_status AS ENUM ('active', 'paused', 'completed');
CREATE TYPE activity_verdict.activity_verdict AS ENUM ('work', 'rest', 'unknown');
CREATE TYPE report_status_color.report_status_color AS ENUM ('green', 'yellow', 'red');
CREATE TYPE activity_source_type AS ENUM ('browser', 'desktop_app', 'window', 'system');

-- Таблица пользователей
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    login VARCHAR(32) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role user_role.user_role NOT NULL DEFAULT 'employee',
    settings JSONB NOT NULL DEFAULT '{"Theme": "light", "WorkTime": "08:30:00"}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_users_settings_theme CHECK ((settings->>'Theme') IN ('light', 'dark')),
    CONSTRAINT chk_users_settings_has_work_time CHECK (settings ? 'WorkTime')
);

-- Таблица стека технологий
CREATE TABLE tech_stack_items (
    item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    position INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_tech_stack_items_user_id ON tech_stack_items(user_id);
CREATE UNIQUE INDEX uq_tech_stack_user_position ON tech_stack_items(user_id, position);
CREATE UNIQUE INDEX uq_tech_stack_user_name ON tech_stack_items(user_id, name);

-- Таблица рабочих сессий
CREATE TABLE work_sessions (
    session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    work_date DATE NOT NULL,
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ,
    total_seconds INTEGER NOT NULL DEFAULT 0,
    work_time INTEGER NOT NULL DEFAULT 0,
    rest_time INTEGER NOT NULL DEFAULT 0,
    status session_status.session_status NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_work_sessions_time CHECK (ended_at IS NULL OR ended_at >= started_at),
    CONSTRAINT chk_work_sessions_totals CHECK (total_seconds >= work_time + rest_time)
);

CREATE INDEX idx_work_sessions_user_id ON work_sessions(user_id);
CREATE INDEX idx_work_sessions_user_date ON work_sessions(user_id, work_date);
CREATE INDEX idx_work_sessions_status ON work_sessions(status);
CREATE UNIQUE INDEX uq_work_sessions_one_open_per_user ON work_sessions(user_id) WHERE status IN ('active', 'paused');

-- Таблица записей активности
CREATE TABLE activity_records (
    activity_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES work_sessions(session_id) ON DELETE CASCADE,
    source_type activity_source_type NOT NULL DEFAULT 'browser',
    source_name VARCHAR(255),
    domain VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ NOT NULL,
    duration_sec INTEGER NOT NULL,
    verdict activity_verdict.activity_verdict NOT NULL DEFAULT 'unknown',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_activity_records_time CHECK (ended_at >= started_at)
);

CREATE INDEX idx_activity_records_session_id ON activity_records(session_id);
CREATE INDEX idx_activity_records_session_started_at ON activity_records(session_id, started_at);
CREATE INDEX idx_activity_records_domain ON activity_records(domain);
CREATE INDEX idx_activity_records_verdict ON activity_records(verdict);
CREATE INDEX idx_activity_records_source_type ON activity_records(source_type);
CREATE INDEX idx_activity_records_started_at ON activity_records(started_at);

-- Таблица нарушений
CREATE TABLE violations (
    violation_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    activity_id UUID NOT NULL UNIQUE REFERENCES activity_records(activity_id) ON DELETE CASCADE,
    screenshot_path TEXT NOT NULL DEFAULT '',
    reason VARCHAR(500) NOT NULL,
    is_disputed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_violations_created_at ON violations(created_at);

-- Таблица дневных отчётов
CREATE TABLE daily_reports (
    report_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    report_date DATE NOT NULL,
    work_percent NUMERIC(5,2) NOT NULL,
    rest_percent NUMERIC(5,2) NOT NULL,
    status_color report_status_color.report_status_color NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_daily_reports_total CHECK ((work_percent = 0 AND rest_percent = 0) OR ABS((work_percent + rest_percent) - 100) <= 0.01)
);

CREATE INDEX idx_daily_reports_user_id ON daily_reports(user_id);
CREATE INDEX idx_daily_reports_report_date ON daily_reports(report_date);
CREATE UNIQUE INDEX uq_daily_reports_user_date ON daily_reports(user_id, report_date);

-- Таблица refresh токенов
CREATE TABLE refresh_tokens (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMPTZ,
    CONSTRAINT chk_refresh_tokens_dates CHECK (expires_at > created_at)
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
