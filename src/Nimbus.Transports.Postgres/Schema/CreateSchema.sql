-- Idempotent schema creation for the Nimbus PostgreSQL transport.
-- Safe to run on every application start when AutoCreateSchema is enabled.

CREATE TABLE IF NOT EXISTS nimbus_messages (
    message_id    UUID        NOT NULL,
    destination   TEXT        NOT NULL,
    body          BYTEA       NOT NULL,
    visible_after TIMESTAMPTZ NOT NULL,
    expires_at    TIMESTAMPTZ NULL,
    CONSTRAINT pk_nimbus_messages PRIMARY KEY (message_id)
);

CREATE INDEX IF NOT EXISTS ix_nimbus_messages_dequeue
    ON nimbus_messages (destination, visible_after) INCLUDE (expires_at);

CREATE TABLE IF NOT EXISTS nimbus_subscriptions (
    topic_name       TEXT NOT NULL,
    subscriber_queue TEXT NOT NULL,
    CONSTRAINT pk_nimbus_subscriptions PRIMARY KEY (topic_name, subscriber_queue)
);

CREATE TABLE IF NOT EXISTS nimbus_dead_letters (
    message_id           UUID        NOT NULL,
    original_destination TEXT        NULL,
    body                 BYTEA       NOT NULL,
    delivery_attempts    INTEGER     NOT NULL,
    failed_at            TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT pk_nimbus_dead_letters PRIMARY KEY (message_id)
);
