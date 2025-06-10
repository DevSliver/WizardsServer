namespace WizardsServer;

using System.Text.RegularExpressions;
using WizardsServer.ServerLogic;

public class NewsService : ICommandProcessor
{
    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "get":
                GetNews(args, client);
                break;
        }
    }

    private void GetNews(string[] args, Client client)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out int newsNumber) || newsNumber < 1)
        {
            client.SendAsync("news error usage");
            return;
        }

        using var cmd = Database.CreateCommand(@"
            SELECT title, content, published_at 
            FROM news 
            ORDER BY published_at DESC 
            OFFSET @offset LIMIT 1");
        cmd.Parameters.AddWithValue("offset", newsNumber - 1);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            client.SendAsync("news error not_found");
            return;
        }

        string title = WrapWithBackticks(reader.GetString(0));
        string content = WrapWithBackticks(reader.GetString(1));
        string date = WrapWithBackticks(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm"));

        cmd.Connection.Close();

        client.SendAsync($"news get {title} {content} {date}");
    }
    private string WrapWithBackticks(string text)
    {
        string escaped = Regex.Replace(text, "`", "'");
        return $"`{escaped}`";
    }
}
