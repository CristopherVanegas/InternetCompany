INSERT INTO Roles (Name) VALUES
('Admin'),
('Gestor'),
('Cajero'),
('Cliente');
GO

INSERT INTO UserStatus (Code, Description) VALUES
('ACT', 'Activo'),
('INA', 'Inactivo'),
('PEN', 'Pendiente');
GO

INSERT INTO Users 
(Username, Email, PasswordHash, RoleId, StatusId)
VALUES 
(
    'admin',
    'admin@internet.com',
    'TEMP_HASH_REPLACE_LATER',
    1,  -- Admin
    1   -- ACT
);
GO
