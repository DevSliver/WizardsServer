using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardsServer
{
    public class GameMatch
    {
        public Guid MatchId { get; }
        public IConnectionContext Player1 { get; }
        public IConnectionContext Player2 { get; }

        // Пример состояния
        public int Player1Health { get; set; } = 20;
        public int Player2Health { get; set; } = 20;

        // Игровое поле, колоды и прочее можно добавить здесь

        public GameMatch(Guid matchId, IConnectionContext player1, IConnectionContext player2)
        {
            MatchId = matchId;
            Player1 = player1;
            Player2 = player2;
        }

        // Методы для управления матчем:
        // - Обработка ходов
        // - Проверка победы
        // - Отправка обновлений игрокам и т.д.
    }
}
