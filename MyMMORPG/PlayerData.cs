
// MyMMORPG/PlayerData.cs
namespace MyMMORPG
{
    public class PlayerData
    {
        public int Id {get; set;}
        public string name {get; set;} = "Player";
        public float x {get; set;}
        public float y {get; set;}

        public DateTime LastMoveTime  {get; set;} = DateTime.UtcNow;

        public float LastX   {get; set;} = 100f;
        public float LastY   {get; set;} = 100f;
    }
}