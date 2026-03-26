// TestClient1/Program.cs
using System.Net.Sockets;
using MyMMORPG.Network;
using MyMMORPG.Shared;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("🎮 Client starting...");

// اتصل بالسيرفر
using TcpClient client = new TcpClient();
await client.ConnectAsync("127.0.0.1", 7171);
NetworkStream stream = client.GetStream();

Console.WriteLine("✅ Connected to server!");

// ① ابعت حركة
// بدل ما تبعت X=120 Y=500 دفعة واحدة
await Task.Delay(500);
await ClientMove(stream, 110f, 110f); // خطوة صغيرة الأول
await Task.Delay(500);
await ClientMove(stream, 120f, 120f); // خطوة تانية
await Task.Delay(500);
await ClientMove(stream, 100f, 200f); // دلوقتي تقدر تتحرك أكتر
await Task.Delay(500);

// ③ ابعت chat
await ClientChat(stream, "Hello everyone! im new Player");

await Task.Delay(500);
Console.WriteLine("✅ Done!");

await Task.Delay(1000); // ← ضيف ده — استنى قبل ما الـ connection يقفل


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