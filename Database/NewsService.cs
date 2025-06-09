using System;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WizardsServer
{
    public class NewsService
    {
        public NewsService()
        {
            Server.Instance.CommandProcessor.Subscribe("news", ProcessCommand);
            Server.Instance.CommandProcessor.Subscribe("get", HandleGetNews);
        }
        private void ProcessCommand(string[] args, Client client)
        {
            Server.Instance.CommandProcessor.ProcessCommand(args, client);
        }

        private void HandleGetNews(string[] args, Client client)
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

            client.SendAsync($"news get {title} {content} {date}");
        }
        private string WrapWithBackticks(string text)
        {
            string escaped = Regex.Replace(text, "`", "'");
            return $"`{escaped}`";
        }
    }
}
