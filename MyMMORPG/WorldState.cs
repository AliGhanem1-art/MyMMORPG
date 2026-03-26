// MyMMORPG/WorldState.cs
namespace MyMMORPG
{
    public class WorldState
    {
        
        private readonly System.Collections.Concurrent.ConcurrentDictionary<int,PlayerData>
        _Players = new ();


        public void AddPlayer(PlayerData player)
        {
            _Players[player.Id] = player;
        }

        public void RemovePlayer(int playerid)
        {
            _Players.TryRemove(playerid,out _);
        }

        public PlayerData? GetPlayer(int playerid)
        {
            _Players.TryGetValue(playerid , out var player);
            return player;
        }

        public IEnumerable<PlayerData> GetOtherPlayers(int excludePlayerId)
        {
            return _Players.Values.Where(p => p.Id != excludePlayerId);
        }

        public bool IsValidMove(PlayerData player , float newX , float newY)
        {
            float px = MathF.Abs(newX - player.LastX);
            float py = MathF.Abs(newY - player.LastY);

            float distance = MathF.Sqrt(px*px + py*py);

            double secondsElapsed = 
            (DateTime.UtcNow - player.LastMoveTime).TotalSeconds;

            float maxSpeed = 200f;
            float maxDistance = maxSpeed * (float)secondsElapsed;

            if(distance > maxDistance +10f)
            {
                return false;
            }
            return true;

        }

        public void UpdatePlayerPosition(int PlayerId , float newX , float newY)
        {
            var player = GetPlayer(PlayerId);
            if(player ==null) return;

            player.LastX = player.x;
            player.LastY = player.y;

            player.x = newX;
            player.y = newY;
            player.LastMoveTime = DateTime.UtcNow;
        }
    }
}