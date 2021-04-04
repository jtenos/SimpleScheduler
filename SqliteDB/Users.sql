CREATE TABLE Users
(
    EmailAddress TEXT NOT NULL
);

CREATE UNIQUE INDEX IX_Users_EmailAddress
ON Users (EmailAddress);
