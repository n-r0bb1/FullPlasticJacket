using System.Text.Json;

namespace TheAdventure;

public sealed class ScoreData
{
    public int HighScore { get; set; }
}

/*Start of AI generated code*/
/// Handles loading and saving the high-score JSON file next to the executable.
public sealed class SaveManager
{
    private static readonly string SavePath = Path.Combine(
        AppContext.BaseDirectory, "highscore.json");

    public int LoadHighScore()
    {
        if (!File.Exists(SavePath))
            return 0;

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<ScoreData>(json);
            return data?.HighScore ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task SaveHighScoreAsync(int score)
    {
        var data = new ScoreData { HighScore = score };
        var json = JsonSerializer.Serialize(data);
        await File.WriteAllTextAsync(SavePath, json);
    }
}
/*End of AI generated code*/
