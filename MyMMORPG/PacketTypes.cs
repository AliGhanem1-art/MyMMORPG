namespace MyMMORPG.Shared
{
    public enum PacketType : ushort
    {
        // من الكلاينت للسيرفر
        Move   = 0x01,   // اللاعب اتحرك
        Attack = 0x02,   // اللاعب ضرب
        Chat   = 0x03,   // اللاعب كتب كلام

        // من السيرفر للكلاينت
        Spawn  = 0x10,   // لاعب ظهر في الدنيا
        Update = 0x11,   // تحديث موقع لاعب
        Remove = 0x12,   // لاعب خرج من اللعبة
    }
}
