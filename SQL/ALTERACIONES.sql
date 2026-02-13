ALTER TABLE Users
ADD IsApproved BIT NOT NULL DEFAULT 0;

ALTER TABLE Users
ADD ApprovedByUserId INT NULL;

ALTER TABLE Users
ADD ApprovedAt DATETIME2 NULL;


UPDATE Users
SET IsApproved = 1
WHERE Username = 'admin';
