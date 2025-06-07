using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardsServer
{
    public class UserSettings
    {
        public UserSettings()
        {
            var processor = CommandProcessor.Instance;
            processor.Subscribe("change_username", HandleChangeUsername);
        }
        private void HandleChangeUsername(string[] args, IConnectionContext context)
        {
            if (args.Length != 1)
            {
                context.SendAsync("change_username fail usage");
                return;
            }

            string newUsername = args[0];

            if (context is ConnectionContext ctx)
            {
                if (ctx.Client.UserId is not int userId)
                {
                    context.SendAsync("change_username fail not_authenticated");
                    return;
                }

                try
                {
                    using var checkCmd = Database.CreateCommand("SELECT COUNT(*) FROM users WHERE username = @u");
                    checkCmd.Parameters.AddWithValue("u", newUsername);
                    long exists = Convert.ToInt64(checkCmd.ExecuteScalar());

                    if (exists > 0)
                    {
                        context.SendAsync("change_username fail user_exists");
                        return;
                    }

                    using var cmd = Database.CreateCommand("UPDATE users SET username = @u WHERE id = @id");
                    cmd.Parameters.AddWithValue("u", newUsername);
                    cmd.Parameters.AddWithValue("id", userId);
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        context.SendAsync("change_username success");
                    }
                    else
                    {
                        context.SendAsync("change_username fail error");
                    }
                }
                catch
                {
                    context.SendAsync("change_username fail error");
                }
            }
            else
            {
                context.SendAsync("change_username fail invalid_context");
            }
        }
    }
}
