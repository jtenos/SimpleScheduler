using System.Security.Cryptography;

byte[] buffer = new byte[32];
using RandomNumberGenerator rng = RandomNumberGenerator.Create();
rng.GetBytes(buffer);

Console.WriteLine(Convert.ToHexString(buffer));
