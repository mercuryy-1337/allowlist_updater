using Octokit;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private const string FilePath = @"C:\apps\metadata\allowlist.json";
    private const string RepoOwner = "mercuryy-1337";
    private const string RepoName = "geforcenow";
    private const string Branch = "main";

    // 🔑 Hardcode Github Token {Personal Use only}
    private const string Token = "token here";

    static async Task Main()
    {
        LogInfo("Starting Allowlist Uploader...");

        if (string.IsNullOrEmpty(Token))
        {
            LogError("GitHub token is missing. Please set the token in code.");
            return;
        }

        if (!File.Exists(FilePath))
        {
            LogError("File not found: " + FilePath);
            return;
        }

        string fileName = Path.GetFileName(FilePath);
        string rawJson = File.ReadAllText(FilePath);

        // ✅ Pretty-print JSON
        string formattedJson;
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            formattedJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            LogError("Failed to parse/format JSON: " + ex.Message);
            return;
        }

        var github = new GitHubClient(new ProductHeaderValue("UploaderApp"))
        {
            Credentials = new Credentials(Token)
        };

        var repo = await github.Repository.Get(RepoOwner, RepoName);

        try
        {
            // Check if file already exists
            var existingFile = await github.Repository.Content.GetAllContentsByRef(
                repo.Id, fileName, Branch);

            // Update existing file
            var updateRequest = new UpdateFileRequest(
                $"Update {fileName}",
                formattedJson,
                existingFile[0].Sha,
                Branch);

            await github.Repository.Content.UpdateFile(repo.Id, fileName, updateRequest);
            LogSuccess($"Updated {fileName} in repo.");
        }
        catch (NotFoundException)
        {
            // Create new file if not found
            var createRequest = new CreateFileRequest(
                $"Add {fileName}",
                formattedJson,
                Branch);

            await github.Repository.Content.CreateFile(repo.Id, fileName, createRequest);
            LogSuccess($"Created {fileName} in repo.");
        }
        catch (Exception ex)
        {
            LogError("GitHub operation failed: " + ex.Message);
        }
    }

    // 🔹 Logging helpers
    private static void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    private static void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {message}");
        Console.ResetColor();
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }
}
