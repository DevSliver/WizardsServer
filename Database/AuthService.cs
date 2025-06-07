using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardsServer
{
    public class AuthService
    {
        public AuthService()
        {
            var processor = CommandProcessor.Instance;
            processor.Subscribe("register", HandleRegister);
            processor.Subscribe("login", HandleLogin);
        }

        private void HandleRegister(string[] args, IConnectionContext context)
        {
            if (args.Length != 2)
            {
                context.SendAsync("register fail usage");
                return;
            }

            string username = args[0];
            string password = args[1];

            try
            {
                using var checkCmd = Database.CreateCommand("SELECT COUNT(*) FROM users WHERE username = @u");
                checkCmd.Parameters.AddWithValue("u", username);
                if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0)
                {
                    context.SendAsync("register fail user_exists");
                    return;
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(password);

                using var cmd = Database.CreateCommand("INSERT INTO users (username, password_hash) VALUES (@u, @p) RETURNING id");
                cmd.Parameters.AddWithValue("u", username);
                cmd.Parameters.AddWithValue("p", hash);

                var userId = Convert.ToInt32(cmd.ExecuteScalar());
                AuthenticateClient(context, userId);
                context.SendAsync($"register success {userId}");
            }
            catch
            {
                context.SendAsync("register fail error");
            }
        }

        private void HandleLogin(string[] args, IConnectionContext context)
        {
            if (args.Length != 2)
            {
                context.SendAsync("login fail usage");
                return;
            }

            string username = args[0];
            string password = args[1];

            try
            {
                using var cmd = Database.CreateCommand("SELECT id, password_hash FROM users WHERE username = @u");
                cmd.Parameters.AddWithValue("u", username);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    context.SendAsync("login fail user_not_found");
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, reader.GetString(1)))
                {
                    context.SendAsync("login fail wrong_password");
                    return;
                }

                int userId = reader.GetInt32(0);
                AuthenticateClient(context, userId);
                context.SendAsync($"login success {userId}");
            }
            catch
            {
                context.SendAsync("login fail error");
            }
        }

        private void AuthenticateClient(IConnectionContext context, int userId)
        {
            if (context is ConnectionContext ctx)
                ctx.Client.Authenticate(userId);
        }
    }
}
