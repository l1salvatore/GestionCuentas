USE GC_Account_DB;
GO

BEGIN TRANSACTION;

-- Creacion de cuenta para el usuario Juan Perez con un balance inicial de 800.00
INSERT INTO Accounts (
    UserId,
    FirstName,
    LastName,
    Balance,
    CreatedAt
)
VALUES (
    1,
    'Juan',
    'Perez',
    800.00, 
    GETUTCDATE()
);

-- Obtener el AccountId generado para insertar las transacciones correspondientes
DECLARE @AccountId INT = (SELECT UserId FROM Accounts WHERE UserId = 1);

-- Inserción de una transacción de 1000.00 (depósito)
INSERT INTO Transactions (
    AccountId,
    Amount,
    Type,
    TransactionDate
)
VALUES (
    @AccountId,
    1000.00,
    0,
    GETUTCDATE()
);

-- Inserción de una transacción de 200.00 (retiro)
INSERT INTO Transactions (
    AccountId,
    Amount,
    Type,
    TransactionDate
)
VALUES (
    @AccountId,
    200.00,
    1,
    GETUTCDATE()
);

COMMIT;
GO
