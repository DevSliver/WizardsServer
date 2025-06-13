using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.services;

public class AuthService
{
    public Args Register(Session session, string username, string password)
    {
        using var checkCmd = Database.CreateCommand("SELECT COUNT(*) FROM users WHERE username = @u");
        checkCmd.Parameters.AddWithValue("u", username);

        if (Convert.ToInt64(checkCmd.ExecuteScalar()) > 0)
            return new Args().Add("Message", "user exists");

        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        using var cmd = Database.CreateCommand("INSERT INTO users (username, password_hash) VALUES (@u, @p) RETURNING id");
        cmd.Parameters.AddWithValue("u", username);
        cmd.Parameters.AddWithValue("p", hash);

        int userId = Convert.ToInt32(cmd.ExecuteScalar());
        if (Server.Instance.IsUserAuthed(userId))
            return new Args().Add("Message", "user already authed");

        session.Auth(userId);
        return new Args().Add("Message", "success");
    }

    public Args Login(Session session, string username, string password)
    {
        using var cmd = Database.CreateCommand("SELECT id, password_hash FROM users WHERE username = @u");
        cmd.Parameters.AddWithValue("u", username);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return new Args().Add("Message", "user not exists");

        string hash = reader.GetString(1);
        if (!BCrypt.Net.BCrypt.Verify(password, hash))
            return new Args().Add("Message", "incorrect password");

        int userId = reader.GetInt32(0);
        if (Server.Instance.IsUserAuthed(userId))
            return new Args().Add("Message", "user already authed");

        session.Auth(userId);
        return new Args().Add("Message", "success");
    }
}