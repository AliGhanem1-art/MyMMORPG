namespace MyMMORPG.Shared
{
    public enum PacketType : ushort
    {
        // من الكلاينت للسيرفر
        Move      = 0x01,
        Attack    = 0x02,
        Chat      = 0x03,

        // من السيرفر للكلاينت
        Spawn     = 0x10,  // لاعب جديد ظهر
        Update    = 0x11,  // لاعب اتحرك
        Remove    = 0x12,  // لاعب مشي
     }
}
