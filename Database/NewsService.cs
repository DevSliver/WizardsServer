using System;
using System.Data;
using System.Text.RegularExpressions;

namespace WizardsServer
{
    public class NewsService
    {
        public NewsService()
        {
            CommandProcessor.Instance.Subscribe("get_news", HandleGetNews);
        }

        private void HandleGetNews(string[] args, IConnectionContext context)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int newsNumber) || newsNumber < 1)
            {
                context.SendAsync("news fail invalid_argument");
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
                context.SendAsync("news fail not_found");
                return;
            }

            string title = WrapWithBackticks(reader.GetString(0));
            string content = WrapWithBackticks(reader.GetString(1));
            string date = WrapWithBackticks(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm"));

            context.SendAsync($"news success {title} {content} {date}");
        }
        private string WrapWithBackticks(string text)
        {
            string escaped = Regex.Replace(text, "`", "'");
            return $"`{escaped}`";
        }
    }
}
