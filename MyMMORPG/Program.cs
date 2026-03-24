// Server/Program.cs
using MyMMORPG.Server.Core;

// Program.cs
Console.OutputEncoding = System.Text.Encoding.UTF8;

GameServer server = new GameServer(port: 7171);
await server.StartAsync();
Console.WriteLine("Server stopped.");