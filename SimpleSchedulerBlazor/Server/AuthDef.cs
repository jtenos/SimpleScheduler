using System;

namespace SimpleSchedulerBlazor.Server;

public record class AuthDef(
    string EmailAddress,
    DateTime ExpirationDate,
    string AuthCode
);
