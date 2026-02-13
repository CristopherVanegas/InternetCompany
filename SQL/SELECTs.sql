SELECT * FROM dbo.Menu
SELECT * FROM dbo.RoleMenu
SELECT * FROM dbo.Roles
SELECT * FROM dbo.Users
SELECT * FROM dbo.UserStatus


UPDATE Users
SET PasswordHash = 'PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE='
WHERE Username = 'admin';