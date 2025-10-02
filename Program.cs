using Octokit;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private const string FilePath = @"C:\apps\metadata\i2p\allowlist.json";
    private const string RepoOwner = "mercuryy-1337";
    private const string RepoName = "geforcenow";
    private const string Branch = "main"; // change if your default branch is different

    // 🔑 Hardcode your GitHub token here
    private const string Token = "ghp_yourPersonalAccessTokenHere";

    static async Task Main()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            LogError("GitHub token not set. Please provide a valid token.");
            return;
        }

        if (!File.Exists(FilePath))
        {
            LogError($"File not found: {FilePath}");
            return;
        }

        var github = new GitHubClient(new ProductHeaderValue("UploaderApp"))
        {
            Credentials = new Credentials(Token)
        };

        try
        {
            var repo = await github.Repository.Get(RepoOwner, RepoName);

            string fileName = Path.GetFileName(FilePath);
            string fileContent = File.ReadAllText(FilePath);

            try
            {
                // ✅ Try to fetch existing file
                var existingFile = await github.Repository.Content.GetAllContentsByRef(
                    repo.Id, fileName, Branch);

                // Update existing file
                var updateRequest = new UpdateFileRequest(
                    $"Update {fileName}",
                    fileContent,
                    existingFile[0].Sha,
                    Branch);

                await github.Repository.Content.UpdateFile(repo.Id, fileName, updateRequest);
                LogSuccess($"File '{fileName}' updated in repo.");
            }
            catch (NotFoundException)
            {
                // File does not exist → create it
                var createRequest = new CreateFileRequest(
                    $"Add {fileName}",
                    fileContent,
                    Branch);

                await github.Repository.Content.CreateFile(repo.Id, fileName, createRequest);
                LogSuccess($"File '{fileName}' created in repo.");
            }
        }
        catch (Exception ex)
        {
            LogError($"Unexpected error: {ex.Message}");
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
