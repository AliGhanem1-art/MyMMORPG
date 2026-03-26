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

        private readonly GameServer _server;

        // ③ buffer بنقرأ فيه الداتا الجاية
        // 1024 byte كافية لأي packet عادية
        private readonly byte[] _receiveBuffer = new byte[1024];

        // ④ Constructor — بيتنادى لما لاعب يتصل
        public ClientSession(TcpClient client, int playerId , GameServer server)
        {
            _tcpClient = client;
            _stream    = client.GetStream(); // ← القناة اللي هنتكلم بيها
            PlayerId   = playerId;
            _server    = server;

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
                    OnPacketReceived((PacketType)packetType, payload);
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
               await HandleMove(reader);
                break;
                case PacketType.Chat:
                HandleChat(reader);
                break;

            }
        }

        private async Task HandleMove(PacketReader reader)
        {
            // اقرأ الإحداثيات من الـ packet
            float newX = reader.ReadFloat();
            float newY = reader.ReadFloat();
            
            var playerData = _server.World.GetPlayer(PlayerId);
            if(playerData == null) return;

            if(!_server.World.IsValidMove(playerData,newX , newY))
            {
                var RejectPacket = new PacketWriter(PacketType.Update);
                RejectPacket.Write(PlayerId);
                RejectPacket.Write(X);
                RejectPacket.Write(Y);
                await SendAsync(RejectPacket.Build());
                Console.WriteLine($"⚠️ Player {PlayerId} sent invalid move. " +
                                  $"Rejecting and resetting position.");
                                  return;
            }

            _server.World.UpdatePlayerPosition(PlayerId, newX, newY);

            var upadtePacket = new PacketWriter(PacketType.Update);
            upadtePacket.Write(PlayerId);
            upadtePacket.Write(newX);
            upadtePacket.Write(newY);

            await _server.BroadCastAsync(upadtePacket.Build(),PlayerId);

            Console.WriteLine($"🏃 Player {PlayerId} moved to X={newX} Y={newY}");

            // حدّث موقع اللاعب
        }


        private void HandleChat(PacketReader reader)
        {
            string message = reader.ReadString();
            Console.WriteLine($"💬 Player {PlayerId}: {message}");
        }

        // ⑨ قطع الاتصال بشكل نضيف
        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            _stream.Close();
            _tcpClient.Close();
            _server.RemoveSession(PlayerId);
            Console.WriteLine($"🔴 Player {PlayerId} disconnected");
        }


    }
}