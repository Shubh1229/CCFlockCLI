using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Diagnostics;

public static class SecretProtector
{
    private static readonly string secretDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock");
    private static readonly string secretKey = Path.Combine(secretDir, "APIsecretKeyAPIKEY.txt");
    private static string key = LoadKey();
    private static string LoadKey()
    {
        if (OperatingSystem.IsWindows())
        {
            File.Decrypt(secretKey);
        }
        return Encoding.UTF8.GetString(File.ReadAllBytes(secretKey));
    }
    public static byte[] Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();
        var data = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

        // Combine IV and ciphertext
        var result = new byte[iv.Length + encrypted.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
        return result;
    }

    public static string Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);

        // Extract IV from encrypted data
        byte[] iv = new byte[16];
        byte[] ciphertext = new byte[encryptedData.Length - 16];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(encryptedData, 16, ciphertext, 0, ciphertext.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(decrypted);
    }
}
