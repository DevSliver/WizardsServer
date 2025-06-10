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
                HandleAuth(args, client, isRegistration: true);
                break;
            case "login":
                HandleAuth(args, client, isRegistration: false);
                break;
        }
    }

    private void HandleAuth(string[] args, Client client, bool isRegistration)
    {
        if (args.Length != 2)
        {
            client.SendAsync($"auth {(isRegistration ? "register" : "login")} error usage");
            return;
        }

        string username = args[0];
        string password = args[1];

        try
        {
            if (isRegistration)
            {
                RegisterUser(username, password, client);
            }
            else
            {
                LoginUser(username, password, client);
            }
        }
        catch
        {
            client.SendAsync($"auth {(isRegistration ? "register" : "login")} error unknown");
        }
    }

    private void RegisterUser(string username, string password, Client client)
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

        int userId = Convert.ToInt32(cmd.ExecuteScalar());
        AuthenticateAndRespond(client, userId, "register");
    }

    private void LoginUser(string username, string password, Client client)
    {
        using var cmd = Database.CreateCommand("SELECT id, password_hash FROM users WHERE username = @u");
        cmd.Parameters.AddWithValue("u", username);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            client.SendAsync("auth login error user_not_found");
            return;
        }

        string hash = reader.GetString(1);
        if (!BCrypt.Net.BCrypt.Verify(password, hash))
        {
            client.SendAsync("auth login error wrong_password");
            return;
        }

        int userId = reader.GetInt32(0);
        AuthenticateAndRespond(client, userId, "login");
    }

    private void AuthenticateAndRespond(Client client, int userId, string operation)
    {
        bool alreadyLoggedIn = Server.Instance.Clients.Values.Any(c => c.UserId == client.UserId);
        if (alreadyLoggedIn)
        {
            client.SendAsync($"auth {operation} error alredy_logged_in");
            return;
        }
        client.UserId = userId;
        client.SendAsync($"auth {operation} success {userId}");
    }
}