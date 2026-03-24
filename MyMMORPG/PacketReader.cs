// Network/PacketReader.cs
using System;
using System.Text;

namespace MyMMORPG.Network
{
    public class PacketReader
    {
        private readonly byte[] _data;

        // position — إحنا واقفين فين في الـ bytes دلوقتي
        private int _position = 0;

        // بناخد الـ payload بس — من غير الـ header
        public PacketReader(byte[] payload)
        {
            _data = payload;
        }

        // ① اقرأ رقم صحيح صغير — 2 bytes
        public ushort ReadUShort()
        {
            // ToUInt16 بتحول 2 bytes لـ ushort
            ushort value = BitConverter.ToUInt16(_data, _position);
            _position += 2; // تقدم 2 bytes للأمام
            return value;
        }

        // ② اقرأ رقم صحيح كبير — 4 bytes
        public int ReadInt()
        {
            int value = BitConverter.ToInt32(_data, _position);
            _position += 4;
            return value;
        }

        // ③ اقرأ رقم عشري — 4 bytes
        // للإحداثيات X و Y
        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(_data, _position);
            _position += 4;
            return value;
        }

        // ④ اقرأ نص
        public string ReadString()
        {
            // أولاً اعرف الطول
            ushort length = ReadUShort();

            // بعدين اقرأ الحروف
            string value = Encoding.UTF8.GetString(_data, _position, length);
            _position += length;
            return value;
        }
    }
}