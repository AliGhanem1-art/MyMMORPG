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
await ClientMove(stream, 120f, 500f);

await Task.Delay(500);

// ② ابعت حركة تانية
await ClientMove(stream, 100f, 200f);

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