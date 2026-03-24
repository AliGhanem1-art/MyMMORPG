// Server/Core/GameServer.cs
using System.Net;
using System.Net.Sockets;

namespace MyMMORPG.Server.Core
{
    public class GameServer
    {
        private readonly TcpListener _listener;

        // كل لاعب متصل موجود هنا
        // ConcurrentDictionary آمن لو أكتر من thread بيكتب فيه
        private readonly Dictionary<int, ClientSession> _sessions = new();

        private int _nextPlayerId = 1; // counter بسيط للـ IDs

        public GameServer(int port)
        {
            // استنى على كل الـ IP addresses على الجهاز
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("🎮 Server started on port 7171");
            Console.WriteLine("⏳ Waiting for players...\n");

            // فضل تستنى لاعبين للأبد
            while (true)
            {
                // ✅ الطريقة الأفضل - معالجة الأخطاء:
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    
                    int id = _nextPlayerId++;

                    ClientSession session = new ClientSession(client, id);
                    session.OnDissconected += RemovePlayer;

                    _sessions[id] = session;
                    
                    _ = session.StartListeningAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ خطأ في الاتصال: {ex.Message}");
                    // لا نوجد session إذا فشل الاتصال
                }
            }

            
        }
        public void RemovePlayer(int PlayerId)
            {
                if (_sessions.ContainsKey(PlayerId))
                {
                    _sessions[PlayerId].Disconnect();
                    _sessions.Remove(PlayerId);
                    System.Console.WriteLine($"Player {PlayerId} removed from sessions.");
                }   
            }
    }
}