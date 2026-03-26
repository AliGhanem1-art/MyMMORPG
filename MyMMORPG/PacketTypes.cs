namespace MyMMORPG.Shared
{
    public enum PacketType : ushort
{
    Move   = 0x01,
    Attack = 0x02,
    Chat   = 0x03,
    Spawn  = 0x10,
    Update = 0x11,  // ← تأكد إنه موجود
    Remove = 0x12,
}
}
