// MyMMORPG/GameServer.cs
using System.Net;
using System.Net.Sockets;

namespace MyMMORPG.Server.Core
{
    public class GameServer
    {
        private readonly TcpListener _listener;
        private readonly Dictionary<int, ClientSession> _sessions = new();

        // ① ضيف WorldState هنا
        public readonly WorldState World = new();

        private int _nextPlayerId = 1;

        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("🎮 Server started on port 7171");
            Console.WriteLine("⏳ Waiting for players...\n");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();

                int id         = _nextPlayerId++;
                var session    = new ClientSession(client, id, this);
                _sessions[id]  = session;

                // ② ضيف اللاعب في الـ World
                var playerData = new PlayerData { Id = id };
                World.AddPlayer(playerData);

                _ = session.StartListeningAsync();
            }
        }

        // ③ Broadcast — ابعت packet لكل اللاعبين ما عدا واحد
        public async Task BroadcastAsync(byte[] data, int excludePlayerId)
        {
            foreach (var session in _sessions.Values)
            {
                if (session.PlayerId == excludePlayerId) continue;
                if (!session.IsConnected) continue;

                await session.SendAsync(data);
            }
        }

        // ④ لما لاعب يقطع
        public void RemoveSession(int playerId)
        {
            _sessions.Remove(playerId);
            World.RemovePlayer(playerId);
            Console.WriteLine($"Player {playerId} removed from sessions.");
        }
    }
}