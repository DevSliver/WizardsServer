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
            CommandProcessor.Instance.Subscribe("register", HandleRegister);
            CommandProcessor.Instance.Subscribe("login", HandleLogin);
        }

        private void HandleRegister(string[] args, IConnectionContext context)
        {
            if (args.Length != 2)
            {
                context.SendMessageAsync("register fail usage");
                return;
            }

            string username = args[0];
            string password = args[1];

            try
            {
                using var checkCmd = Database.CreateCommand("SELECT COUNT(*) FROM users WHERE username = @u");
                checkCmd.Parameters.AddWithValue("u", username);
                long exists = Convert.ToInt64(checkCmd.ExecuteScalar());

                if (exists > 0)
                {
                    context.SendMessageAsync("register fail user_exists");
                    return;
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(password);

                using var cmd = Database.CreateCommand("INSERT INTO users (username, password_hash) VALUES (@u, @p)");
                cmd.Parameters.AddWithValue("u", username);
                cmd.Parameters.AddWithValue("p", hash);
                cmd.ExecuteNonQuery();

                context.SendMessageAsync("register success");
            }
            catch
            {
                context.SendMessageAsync($"register fail error");
            }
        }

        private void HandleLogin(string[] args, IConnectionContext context)
        {
            if (args.Length != 2)
            {
                context.SendMessageAsync("login fail usage");
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
                    context.SendMessageAsync("login fail user_not_found");
                    return;
                }

                string hash = reader.GetString(1);
                if (!BCrypt.Net.BCrypt.Verify(password, hash))
                {
                    context.SendMessageAsync("login fail wrong_password");
                    return;
                }

                int userId = reader.GetInt32(0);
                context.SendMessageAsync($"login success {userId}");
            }
            catch
            {
                context.SendMessageAsync("login fail error");
            }
        }
    }
}
