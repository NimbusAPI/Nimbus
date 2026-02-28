-- Idempotent schema creation for the Nimbus SQL Server transport.
-- Safe to run on every application start when AutoCreateSchema is enabled.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NimbusMessages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.NimbusMessages
    (
        MessageId    UNIQUEIDENTIFIER NOT NULL,
        Destination  NVARCHAR(255)    NOT NULL,
        Body         VARBINARY(MAX)   NOT NULL,
        VisibleAfter DATETIME2        NOT NULL,
        ExpiresAt    DATETIME2        NULL,
        CONSTRAINT PK_NimbusMessages PRIMARY KEY (MessageId)
    );

    CREATE INDEX IX_NimbusMessages_Dequeue
        ON dbo.NimbusMessages (Destination, VisibleAfter)
        INCLUDE (ExpiresAt);
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NimbusSubscriptions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.NimbusSubscriptions
    (
        TopicName       NVARCHAR(255) NOT NULL,
        SubscriberQueue NVARCHAR(255) NOT NULL,
        CONSTRAINT PK_NimbusSubscriptions PRIMARY KEY (TopicName, SubscriberQueue)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NimbusDeadLetters' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.NimbusDeadLetters
    (
        MessageId           UNIQUEIDENTIFIER NOT NULL,
        OriginalDestination NVARCHAR(255)    NULL,
        Body                VARBINARY(MAX)   NOT NULL,
        DeliveryAttempts    INT              NOT NULL,
        FailedAt            DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_NimbusDeadLetters PRIMARY KEY (MessageId)
    );
END
