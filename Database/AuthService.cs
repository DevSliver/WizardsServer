using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardsServer.ServerLogic;

namespace WizardsServer;

public class AuthService : ICommandProcessor
{
    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "register":
                Register(args, client);
                break;
            case "login":
                Login(args, client);
                break;
        }
    }
    private void Register(string[] args, Client client)
    {
        if (args.Length != 2)
        {
            client.SendAsync("auth register error usage");
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
                client.SendAsync("auth register error user_exists");
                return;
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            using var cmd = Database.CreateCommand("INSERT INTO users (username, password_hash) VALUES (@u, @p) RETURNING id");
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", hash);
            var userId = Convert.ToInt32(cmd.ExecuteScalar());

            cmd.Connection.Close();

            AuthenticateClient(client, userId);
            client.SendAsync($"auth register success");
        }
        catch (Exception ex)
        {
            client.SendAsync($"auth register error unknown");
        }
    }

    private void Login(string[] args, Client client)
    {
        if (args.Length != 2)
        {
            client.SendAsync("auth login error usage");
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
                client.SendAsync("auth login error user_not_found");
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, reader.GetString(1)))
            {
                client.SendAsync("auth login error wrong_password");
                return;
            }

            int userId = reader.GetInt32(0);
            AuthenticateClient(client, userId);
            client.SendAsync($"auth login success");
        }
        catch
        {
            client.SendAsync("auth login error unknown");
        }
    }

    private void AuthenticateClient(Client client, int userId)
    {
        client.Authenticate(userId);
    }
}
