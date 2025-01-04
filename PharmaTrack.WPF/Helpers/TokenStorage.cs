using System;
using System.IO;

namespace PharmaTrack.WPF.Helpers
{
    public static class TokenStorage
    {
        private static readonly string FilePath = "tokens.txt";

        // Save tokens to file
        public static void SaveTokens(string accessToken, string refreshToken, string userName)
        {
            try
            {
                File.WriteAllText(FilePath, $"{accessToken}\n{refreshToken}\n{userName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tokens: {ex.Message}");
            }
        }

        // Read tokens from file
        public static (string AccessToken, string RefreshToken, string UserName) ReadTokens()
        {
            try
            {
                // Ensure the file exists
                if (!File.Exists(FilePath))
                {
                    File.Create(FilePath).Close(); // Create and close the file
                    return (string.Empty, string.Empty, string.Empty); // Return empty values
                }

                // Read all lines
                string[] lines = File.ReadAllLines(FilePath);

                // Ensure file has enough lines to avoid IndexOutOfRangeException
                if (lines.Length >= 3)
                {
                    return (lines[0], lines[1], lines[2]);
                }
                else
                {
                    Console.WriteLine("Token file is not in the expected format.");
                    return (string.Empty, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tokens: {ex.Message}");
                return (string.Empty, string.Empty, string.Empty); // Return empty values on error
            }
        }
    }
}
