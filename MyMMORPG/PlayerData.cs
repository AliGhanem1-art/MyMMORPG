// MyMMORPG/PlayerData.cs
namespace MyMMORPG
{
    public class PlayerData
    {
        public int Id       { get; set; }
        public string Name  { get; set; } = "Unknown";
        public float X      { get; set; } = 100f;
        public float Y      { get; set; } = 100f;

        // آخر وقت اتحرك فيه — هنستخدمه للـ validate
        public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;

        // آخر مكان كان فيه — للـ validate كمان
        public float LastX  { get; set; } = 100f;
        public float LastY  { get; set; } = 100f;
    }
}