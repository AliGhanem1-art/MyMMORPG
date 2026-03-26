// MyMMORPG/GameServer.cs
using System.Net;
using System.Net.Sockets;
using MyMMORPG.Network;

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
                var playerData = new PlayerData { Id = id };

                // ① ضيف اللاعب في الـ World قبل ما تعمله session
                World.AddPlayer(playerData);

                var session    = new ClientSession(client, id, this);
                _sessions[id]  = session;

                // ② ابعت Spawn للاعبين التانيين — عرّفهم باللاعب الجديد
                await SpawnForOthersAsync(playerData);

                // ③ ابعت للاعب الجديد كل اللاعبين الموجودين
                await SpawnExistingPlayersAsync(session);

                _ = session.StartListeningAsync();
            }
        }
        
        // ابعت للاعبين الموجودين إن لاعب جديد ظهر
        private async Task SpawnForOthersAsync(PlayerData newplayer)
        {
            var spawnpacket = new PacketWriter(Shared.PacketType.Spawn);
            spawnpacket.Write(newplayer.Id);
            spawnpacket.Write(newplayer.X);
            spawnpacket.Write(newplayer.Y);

            byte[] data = spawnpacket.Build();

            await BroadcastAsync(data, newplayer.Id);
            Console.WriteLine($"📢 Spawned Player {newplayer.Id} for others");

        }

        // ابعت للاعب الجديد كل اللاعبين اللي موجودين قبله
        private async Task SpawnExistingPlayersAsync(ClientSession newSession)
        {
            foreach (var player in World.GetOtherPlayers(newSession.PlayerId))
            {
                var spwanpacket = new PacketWriter(Shared.PacketType.Spawn);
                spwanpacket.Write(player.Id);
                spwanpacket.Write(player.X);
                spwanpacket.Write(player.Y);
    
                byte[] data = spwanpacket.Build();
                await newSession.SendAsync(data);
            }
            Console.WriteLine($"📢 Sent existing players to Player {newSession.PlayerId}");

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

        // لما لاعب يقطع — بلّغ الكل
        public async Task RemoveSession(int playerId)
        {
            _sessions.Remove(playerId);
            World.RemovePlayer(playerId);

            // ابعت Remove للاعبين التانيين
            var removepacket = new PacketWriter(Shared.PacketType.Remove);
            removepacket.Write(playerId);
            byte[] data = removepacket.Build();
            await BroadcastAsync(data, excludePlayerId: playerId);
            
            Console.WriteLine($"📢 Player {playerId} removed — others notified");

        }
    }
}