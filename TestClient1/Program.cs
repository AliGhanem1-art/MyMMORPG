// TestClient1/Program.cs
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using MyMMORPG.Network;
using MyMMORPG.Shared;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("🎮 Client starting...");

// اتصل بالسيرفر
using TcpClient client = new TcpClient();
await client.ConnectAsync("127.0.0.1", 7171);
NetworkStream stream = client.GetStream();

Console.WriteLine("✅ Connected to server!");

_= RecivingLoopAsync(); // استنى من السيرفر في background

// ② ابعت حركات
await ClientMove(stream, 110f, 110f);
await Task.Delay(500);
await ClientMove(stream, 130f, 130f);
await Task.Delay(500);
await ClientChat(stream, "Hello everyone! im new Player");
await Task.Delay(5000); // استنى عشان تستقبل من السيرفر
Console.WriteLine("✅ Done!");
Console.ReadKey();

async Task RecivingLoopAsync()
{
   
   byte[] header = new byte[4];
   try
   {
      while(true)
      {
         int read = await stream.ReadAsync(header , 0 ,4 );
         if(read == 0)
         {
            Console.WriteLine("🔴 Server disconnected");
            break;
         }

         ushort type = BitConverter.ToUInt16(header, 0);
         ushort size = BitConverter.ToUInt16(header, 2);

         byte[] payload = new byte[size];

         _= await stream.ReadAsync(payload,0,size);
         var reader = new PacketReader(payload);
         switch((PacketType)type)
         {
            case PacketType.Spawn:
               {
                  int spawnid = reader.ReadInt();
                  float sx = reader.ReadFloat();
                  float sy = reader.ReadFloat();
                  Console.WriteLine($"👾 Spawned Player {spawnid} at X={sx}, Y={sy}");
               }
            break;
            case PacketType.Update:
               {
                  int updateId = reader.ReadInt();
                  float ux = reader.ReadFloat();
                  float uy = reader.ReadFloat();
                  Console.WriteLine($"🔄 Updated Player {updateId} to X={ux}, Y={uy}");
               }
            break;
            case PacketType.Remove:
               {
                  int removeId = reader.ReadInt();
                  Console.WriteLine($"❌ Removed Player {removeId}");
               }
            break;
         }
      }
   }
   catch{}
}

async Task ClientMove(NetworkStream stream , float x , float y)
{
   var movePacket = new PacketWriter(PacketType.Move);
   movePacket.Write(x); // X
   movePacket.Write(y); // Y
   await stream.WriteAsync(movePacket.Build());
   Console.WriteLine($"📦 Sent MOVE → X={x}, Y={y}");
}

async Task ClientChat(NetworkStream stream , string message)
{
   var chatPacket = new PacketWriter(PacketType.Chat);
   chatPacket.Write(message);
   await stream.WriteAsync(chatPacket.Build());
   Console.WriteLine("📦 Sent CHAT");
}