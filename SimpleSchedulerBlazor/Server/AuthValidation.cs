using System;
using System.Text.Json;

namespace SimpleSchedulerBlazor.Server;

public static class AuthValidation
{
    public static bool IsValidAuth(IConfiguration config, string authValue)
    {
        try
        {
            byte[] encryptedAuth = Convert.FromHexString(authValue);
            byte[] authBytes = Crypto.Decrypt(encryptedAuth,
                Convert.FromHexString(config["CryptoKey"]),
                Convert.FromHexString(config["AuthKey"]));
            AuthDef authDef = JsonSerializer.Deserialize<AuthDef>(authBytes)!;

            if (authDef.ExpirationDate < DateTime.Now)
            {
                return false;
            }

            if (authDef.AuthCode != config["AuthCode"])
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
