using WizardsServer.Utils;

namespace WizardsServer.GameLogic;

public class Match
{
    public Guid Id { get; }
    public Player Player1 { get; }
    public Player Player2 { get; }
    public CommandProcessor CommandProcessor { get; }
    public Battlefield Battlefield { get; }

    public Player ActivePlayer { get; private set; }

    public Match(Guid id, Client client1, Client client2)
    {
        Id = id;
        Player1 = new Player(client1, 1);
        Player2 = new Player(client2, 2);

        client1.SetMatchInfo(this, Player1);
        client2.SetMatchInfo(this, Player2);

        CommandProcessor = new CommandProcessor();
        Battlefield = new Battlefield();

        CommandProcessor.Subscribe("unit_move", OnUnitMoveCommand);

        // Установка активного игрока
        ActivePlayer = Player1;

        // Можно сразу сбросить очки движения у его юнитов
        ResetMovementPoints(ActivePlayer);
    }

    public void BroadcastAsync(string message)
    {
        Player1.Client.SendAsync(message);
        Player2.Client.SendAsync(message);
    }

    public bool IsPlayerTurn(Player player)
    {
        return player == ActivePlayer;
    }

    public void EndTurn(Player player)
    {
        if (!IsPlayerTurn(player))
            return;

        // Переключение активного игрока
        ActivePlayer = (ActivePlayer == Player1) ? Player2 : Player1;

        // Сброс очков движения юнитов нового активного игрока
        ResetMovementPoints(ActivePlayer);

        BroadcastAsync($"turn_start {ActivePlayer.Id}");
    }

    private void ResetMovementPoints(Player player)
    {
        // Находит всех юнитов игрока и сбрасывает им очки движения
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var unit = Battlefield.GetUnitAt(new Vector2Int(x, y));
                if (unit != null && unit.Owner == player)
                {
                    unit.ResetMovementPoints();
                }
            }
        }
    }
    private void OnUnitMoveCommand(string[] args, Client client)
    {
        if (args.Length < 4)
            return;

        if (client.Match == null || client.Player == null)
            return;

        var match = client.Match;
        var player = client.Player;

        if (!match.IsPlayerTurn(player))
            return;

        if (!int.TryParse(args[0], out int fromX) ||
            !int.TryParse(args[1], out int fromY) ||
            !int.TryParse(args[2], out int toX) ||
            !int.TryParse(args[3], out int toY))
            return;

        var from = new Vector2Int(fromX, fromY);
        var to = new Vector2Int(toX, toY);

        var battlefield = match.Battlefield;
        var movingUnit = battlefield.GetUnitAt(from);

        if (movingUnit == null || movingUnit.Owner != player)
            return;

        if (!battlefield.IsInside(to) || !movingUnit.CanMoveTo(to))
            return;

        var targetUnit = battlefield.GetUnitAt(to);

        if (targetUnit == null)
        {
            // Клетка пуста — просто двигаем
            battlefield.MoveUnit(from, to);
            movingUnit.MoveTo(to);
            match.BroadcastAsync($"unit_moved {fromX} {fromY} {toX} {toY}");
            return;
        }

        // Клетка занята — происходит бой
        targetUnit.ReceiveDamage(movingUnit.AttackDamage);
        movingUnit.ReceiveDamage(targetUnit.AttackDamage);

        bool movingAlive = movingUnit.Health > 0;
        bool targetAlive = targetUnit.Health > 0;

        // Удаляем мёртвых
        if (!movingAlive)
            battlefield.RemoveUnit(from);
        if (!targetAlive)
            battlefield.RemoveUnit(to);

        // Если выжил только один — он занимает клетку
        if (movingAlive && !targetAlive)
        {
            battlefield.MoveUnit(from, to);
            movingUnit.MoveTo(to);
        }

        // Иначе (оба живы, или оба мертвы) — никто не двигается

        match.BroadcastAsync($"unit_battle {fromX} {fromY} {toX} {toY} " +
            $"{(movingAlive ? 1 : 0)} {(targetAlive ? 1 : 0)}");

        // Можно дополнительно сообщать об удалении юнитов:
        if (!movingAlive)
            match.BroadcastAsync($"unit_died {fromX} {fromY}");
        if (!targetAlive)
            match.BroadcastAsync($"unit_died {toX} {toY}");
    }
}