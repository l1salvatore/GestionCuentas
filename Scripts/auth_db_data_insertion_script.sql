USE GC_Auth_DB;
GO

BEGIN TRANSACTION;

-- Creacion de cuenta para el usuario Juan Perez con un balance inicial de 800.00
INSERT INTO Users
(
    Email,
    Password
)
VALUES
(
    'johnperez@email.com',
    -- Password: 12345678
    '100000.mMwINebS/nBOdKgaGspDJA==.a/Jwp4dEBrX5epXSSK+e51n8SZ4OlgD284V7RnqKrl8='
);

COMMIT;
GO
