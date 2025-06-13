namespace WizardsServer.services;

using WizardsServer.ServerLogic.CommandSystem;

public class NewsService
{
    public Args Get(int number)
    {
        using var cmd = Database.CreateCommand(@"
            SELECT title, content, published_at 
            FROM news 
            ORDER BY published_at DESC 
            OFFSET @offset LIMIT 1");
        cmd.Parameters.AddWithValue("offset", number - 1);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return new Args().Add("Message", "news not found");

        string title = reader.GetString(0);
        string content = reader.GetString(1);
        string date = reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm");

        return new Args().Add("Message", "success")
            .Add("Title", title).Add("Content", content).Add("Date", date);
    }
}
