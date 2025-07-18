﻿using Microsoft.Data.Sqlite;

namespace PharmaTrack.WPF.Helpers
{
    public static class TokenStorage
    {
        private static readonly string DatabasePath = "tokens.db";
        private static readonly string ConnectionString = $"Data Source={DatabasePath};";

        public static string? AccessToken
        {
            get { return App.Current.Properties.Contains("AccessToken")
                    ? App.Current.Properties["AccessToken"] as string
                    : string.Empty;}
            set { App.Current.Properties["AccessToken"] = value; }
        }

        public static string? RefreshToken
        {
            get { return App.Current.Properties.Contains("RefreshToken")
                    ? App.Current.Properties["RefreshToken"] as string
                    : string.Empty; }
            set { App.Current.Properties["RefreshToken"] = value; }
        }

        public static string? UserName
        {
            get { return App.Current.Properties.Contains("UserName")
                    ? App.Current.Properties["UserName"] as string
                    : string.Empty; }
            set { App.Current.Properties["UserName"] = value; }
        }

        public static bool IsUserAdmin
        {
            get
            {
                if (App.Current.Properties.Contains("IsAdmin") && App.Current.Properties["IsAdmin"] is bool value)
                {
                    return value;
                }
                return false; // Default to false if the property doesn't exist or isn't a bool
            }
            set
            {
                App.Current.Properties["IsAdmin"] = value;
            }
        }


        static TokenStorage()
        {
            InitializeDatabase();
        }

        // Initialize the database
        private static void InitializeDatabase()
        {
            // SQLite will create the file when a connection is opened
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Ensure the Tokens table exists
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Tokens (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccessToken TEXT NOT NULL,
                    RefreshToken TEXT NOT NULL,
                    UserName TEXT NOT NULL,
                    IsUserAdmin BOOLEAN DEFAULT 0
                );";
            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }


        // Save tokens to the database
        public static void SaveTokens(string accessToken, string refreshToken, string userName, bool isUserAdmin, bool remember = false)
        {

            AccessToken = accessToken;
            RefreshToken = refreshToken;
            UserName = userName;
            IsUserAdmin = isUserAdmin;

            if (remember == false) return;
            //If remember is true, then save tokens persistently (in SQLite)
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Clear existing tokens 
                string deleteQuery = "DELETE FROM Tokens;";
                using (var deleteCommand = new SqliteCommand(deleteQuery, connection))
                {
                    deleteCommand.ExecuteNonQuery();
                }

                // Insert new tokens
                string insertQuery = @"
                    INSERT INTO Tokens (AccessToken, RefreshToken, UserName, IsUserAdmin)
                    VALUES (@AccessToken, @RefreshToken, @UserName, @IsUserAdmin);";
                using var insertCommand = new SqliteCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@AccessToken", accessToken);
                insertCommand.Parameters.AddWithValue("@RefreshToken", refreshToken);
                insertCommand.Parameters.AddWithValue("@UserName", userName);
                insertCommand.Parameters.AddWithValue("@IsUserAdmin", isUserAdmin);

                insertCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tokens: {ex.Message}");
            }
        }

        // Read tokens from the database
        public static (string AccessToken, string RefreshToken, string UserName, bool IsUserAdmin) ReadTokens()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                string selectQuery = "SELECT AccessToken, RefreshToken, UserName, IsUserAdmin FROM Tokens LIMIT 1;";
                using var command = new SqliteCommand(selectQuery, connection);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string accessToken = reader.GetString(0);
                    string refreshToken = reader.GetString(1);
                    string userName = reader.GetString(2);
                    bool isUserAdmin = reader.GetBoolean(3);
                    return (accessToken, refreshToken, userName, isUserAdmin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tokens: {ex.Message}");
            }

            // Return empty values if no tokens are found
            return (string.Empty, string.Empty, string.Empty, false);
        }

        //Delete token from the database
        public static void DeleteTokens()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Clear existing tokens 
                string deleteQuery = "DELETE FROM Tokens;";
                using var deleteCommand = new SqliteCommand(deleteQuery, connection);
                deleteCommand.ExecuteNonQuery();

                // Remove from App.Current.Properties
                if (App.Current.Properties.Contains("AccessToken"))
                    App.Current.Properties.Remove("AccessToken");

                if (App.Current.Properties.Contains("RefreshToken"))
                    App.Current.Properties.Remove("RefreshToken");

                if (App.Current.Properties.Contains("UserName"))
                    App.Current.Properties.Remove("UserName");

                if (App.Current.Properties.Contains("IsAdmin"))
                    App.Current.Properties.Remove("IsAdmin");

                // Set the static properties to null or empty
                AccessToken = null;
                RefreshToken = null;
                UserName = null;
                IsUserAdmin = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tokens: {ex.Message}");
            }
        }
    }
}
