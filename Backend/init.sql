-- Создание enum типов
CREATE TYPE user_role AS ENUM ('employee', 'admin');
CREATE TYPE session_status AS ENUM ('active', 'paused', 'completed');
CREATE TYPE activity_verdict AS ENUM ('work', 'rest', 'unknown');
CREATE TYPE report_status_color AS ENUM ('green', 'yellow', 'red');

-- Таблица пользователей
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    login VARCHAR(32) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role user_role NOT NULL DEFAULT 'employee',
    settings JSONB NOT NULL DEFAULT '{"Theme": "light", "WorkTime": "08:30:00"}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Таблица стека технологий
CREATE TABLE tech_stack_items (
    item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    position INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, position)
);

-- Таблица рабочих сессий
CREATE TABLE work_sessions (
    session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    work_date DATE NOT NULL,
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ,
    work_time INTEGER NOT NULL DEFAULT 0,
    rest_time INTEGER NOT NULL DEFAULT 0,
    status session_status NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_work_sessions_user_date ON work_sessions(user_id, work_date);
CREATE INDEX idx_work_sessions_status ON work_sessions(status);

-- Таблица записей активности
CREATE TABLE activity_records (
    activity_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES work_sessions(session_id) ON DELETE CASCADE,
    domain VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ NOT NULL,
    duration_sec INTEGER NOT NULL,
    verdict activity_verdict NOT NULL DEFAULT 'unknown',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_activity_records_session ON activity_records(session_id);
CREATE INDEX idx_activity_records_domain ON activity_records(domain);
CREATE INDEX idx_activity_records_verdict ON activity_records(verdict);

-- Таблица нарушений
CREATE TABLE violations (
    violation_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    activity_id UUID NOT NULL UNIQUE REFERENCES activity_records(activity_id) ON DELETE CASCADE,
    screenshot_path TEXT NOT NULL DEFAULT '',
    reason VARCHAR(500) NOT NULL,
    is_disputed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Таблица дневных отчётов
CREATE TABLE daily_reports (
    report_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    report_date DATE NOT NULL,
    work_percent NUMERIC(5,2) NOT NULL,
    rest_percent NUMERIC(5,2) NOT NULL,
    status_color report_status_color NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, report_date)
);

-- Таблица refresh токенов
CREATE TABLE refresh_tokens (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMPTZ
);
