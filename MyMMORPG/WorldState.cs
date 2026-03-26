// MyMMORPG/WorldState.cs
namespace MyMMORPG
{
    public class WorldState
    {
        // Dictionary — كل لاعب ليه ID وبياناته
        // ConcurrentDictionary آمن لو أكتر من thread بيكتب فيه
        private readonly System.Collections.Concurrent.ConcurrentDictionary
            <int, PlayerData> _players = new();

        // ① ضيف لاعب جديد
        public void AddPlayer(PlayerData player)
        {
            _players[player.Id] = player;
            Console.WriteLine($"🌍 World: Player {player.Id} added " +
                              $"at X={player.X} Y={player.Y}");
        }

        // ② شيل لاعب
        public void RemovePlayer(int playerId)
        {
            _players.TryRemove(playerId, out _);
            Console.WriteLine($"🌍 World: Player {playerId} removed");
        }

        // ③ جيب بيانات لاعب معين
        public PlayerData? GetPlayer(int playerId)
        {
            _players.TryGetValue(playerId, out var player);
            return player;
        }

        // ④ جيب كل اللاعبين ما عدا واحد
        public IEnumerable<PlayerData> GetOtherPlayers(int excludeId)
        {
            return _players.Values.Where(p => p.Id != excludeId);
        }

        // ⑤ Validate الحركة — هل اللاعب بيغش؟
        public bool IsValidMove(PlayerData player, float newX, float newY)
        {
            // احسب المسافة بين المكان القديم والجديد
            float dx = Math.Abs(newX - player.LastX);
            float dy = Math.Abs(newY - player.LastY);
            float distance = MathF.Sqrt(dx * dx + dy * dy);

            // احسب الوقت اللي فات من آخر حركة
            double secondsElapsed = 
                (DateTime.UtcNow - player.LastMoveTime).TotalSeconds;

            // أقصى سرعة = 200 وحدة في الثانية
            float maxSpeed    = 200f;
            float maxDistance = maxSpeed * (float)secondsElapsed;

            if (distance > maxDistance + 10f) // +10 هامش خطأ بسيط
            {
                Console.WriteLine($"⚠️ Player {player.Id} moving too fast! " +
                                  $"distance={distance:F1} max={maxDistance:F1}");
                return false;
            }

            return true;
        }

        // ⑥ حدّث مكان اللاعب
        public void UpdatePlayerPosition(int playerId, float newX, float newY)
        {
            var player = GetPlayer(playerId);
            if (player == null) return;

            player.LastX        = player.X;
            player.LastY        = player.Y;
            player.X            = newX;
            player.Y            = newY;
            player.LastMoveTime = DateTime.UtcNow;
        }
    }
}