using System.Net.Sockets;
using MyMMORPG.Shared;
using MyMMORPG.Network;


namespace MyMMORPG.Server.Core
{
    public class ClientSession
    {

        // ① معلومات اللاعب
        public int PlayerId { get; private set; }
        public float X { get; set; } = 100f;
        public float Y { get; set; } = 100f;
        public bool IsConnected { get; private set; }

        // ② الأدوات اللي بنتكلم بيها مع جهازه
        private readonly TcpClient   _tcpClient;
        private readonly NetworkStream _stream;

        // في ClientSession.cs — عدّل الـ Constructor
        private readonly GameServer _server; // ضيف ده

        // ③ buffer بنقرأ فيه الداتا الجاية
        // 1024 byte كافية لأي packet عادية
        private readonly byte[] _receiveBuffer = new byte[1024];

        // ④ Constructor — بيتنادى لما لاعب يتصل
        public ClientSession(TcpClient client, int playerId, GameServer server)
        {
            _tcpClient  = client;
            _stream     = client.GetStream();
            PlayerId    = playerId;
            _server     = server;          // ← ضيف ده
            IsConnected = true;

            Console.WriteLine($"✅ Player {PlayerId} connected from " +
                            $"{client.Client.RemoteEndPoint}");
        }   

        // ⑤ بدأ الاستماع — بيشتغل في background
        public async Task StartListeningAsync()
        {
            try
            {
                // فضل تستنى داتا طول ما اللاعب متصل
                while (IsConnected)
                {
                    // أولاً: اقرأ الـ header بس (4 bytes دايماً)
                    // Header = 2 bytes (type) + 2 bytes (size)
                    byte[] header = new byte[4];
                    int bytesRead = await ReadExactAsync(header, 4);

                    if (bytesRead == 0)
                    {
                        // اللاعب قطع الاتصال
                        break;
                    }

                    // ② افهم الـ header
                    ushort packetType = BitConverter.ToUInt16(header, 0);
                    ushort packetSize = BitConverter.ToUInt16(header, 2);

                    // ③ دلوقتي اقرأ الـ payload (الداتا الفعلية)
                    byte[] payload = new byte[packetSize];
                    await ReadExactAsync(payload, packetSize);

                    // ④ معالجة الـ packet
                  await OnPacketReceived((PacketType)packetType, payload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Player {PlayerId} error: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        // ⑥ بنقرأ عدد bytes محدد بالظبط — مش أقل ولا أكتر
        // ده مهم لأن TCP ممكن يوصل الداتا في أجزاء
        private async Task<int> ReadExactAsync(byte[] buffer, int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = await _stream.ReadAsync(
                    buffer, totalRead, count - totalRead);

                if (read == 0) return 0; // اللاعب قطع

                totalRead += read;
            }

            return totalRead;
        }

        // ⑦ بعت داتا للاعب ده
        public async Task SendAsync(byte[] data)
        {
            if (!IsConnected) return;

            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch
            {
                Disconnect();
            }
        }

        // ⑧ هنكملها في الطبقة التالية
        private async Task OnPacketReceived(PacketType type, byte[] payload)
        {
            Console.WriteLine($"📦 Received {type} from Player {PlayerId}");
            // هنشرح المعالجة في طبقة ٣

            var reader = new PacketReader(payload);
            switch(type)
            {
                case PacketType.Move:
                HandleMove(reader);
                break;
                case PacketType.Chat:
                HandleChat(reader);
                break;

            }
        }

        // عدّل HandleMove
    private async void HandleMove(PacketReader reader)
    {
        float newX = reader.ReadFloat();
        float newY = reader.ReadFloat();

        var player = _server.World.GetPlayer(PlayerId);
        if (player == null) return;

        // ① Validate الحركة
        if (!_server.World.IsValidMove(player, newX, newY))
        {
            // ارجعله مكانه الصح
            var rejectPacket = new PacketWriter(PacketType.Update);
            rejectPacket.Write(PlayerId);
            rejectPacket.Write(player.X);
            rejectPacket.Write(player.Y);
            await SendAsync(rejectPacket.Build());
            return;
        }

        // ② حدّث الـ World State
        _server.World.UpdatePlayerPosition(PlayerId, newX, newY);
        Console.WriteLine($"🏃 Player {PlayerId} moved to X={newX} Y={newY}");

        // ③ Broadcast للاعبين التانيين
        var updatePacket = new PacketWriter(PacketType.Update);
        updatePacket.Write(PlayerId);
        updatePacket.Write(newX);
        updatePacket.Write(newY);
        await _server.BroadcastAsync(updatePacket.Build(), excludePlayerId: PlayerId);
        }


        private void HandleChat(PacketReader reader)
        {
            string message = reader.ReadString();
            Console.WriteLine($"💬 Player {PlayerId}: {message}");
        }

        // عدّل Disconnect
        public void Disconnect()
        {
            if (!IsConnected) return;
            IsConnected = false;
            _stream.Close();
            _tcpClient.Close();
            _server.RemoveSession(PlayerId); // ← ضيف ده
            Console.WriteLine($"🔴 Player {PlayerId} disconnected");
        }


    }
}