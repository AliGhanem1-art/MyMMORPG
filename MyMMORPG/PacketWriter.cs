// Network/PacketWriter.cs
using System;
using System.Text;
using MyMMORPG.Shared;

namespace MyMMORPG.Network
{
    public class PacketWriter
    {
        // List<byte> بنضيف فيها الـ bytes واحد ورا التاني
        private readonly List<byte> _buffer = new();

        // PacketType بيتكتب أول حاجة دايماً
        public PacketWriter(PacketType type)
        {
            // اكتب نوع الـ packet — 2 bytes
            Write((ushort)type);
        }

        // ① كتابة رقم صحيح صغير — 2 bytes
        public void Write(ushort value)
        {
            // BitConverter بيحول الرقم لـ bytes
            byte[] bytes = BitConverter.GetBytes(value);
            _buffer.AddRange(bytes);
        }

        // ② كتابة رقم صحيح كبير — 4 bytes
        public void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            _buffer.AddRange(bytes);
        }

        // ③ كتابة رقم عشري — 4 bytes
        // بنستخدمه للإحداثيات X و Y
        public void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            _buffer.AddRange(bytes);
        }

        // ④ كتابة نص — بنحول الحروف لـ bytes
        public void Write(string value)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);

            // اكتب طول النص الأول عشان القارئ يعرف يوقف فين
            Write((ushort)strBytes.Length);

            // بعدين اكتب النص نفسه
            _buffer.AddRange(strBytes);
        }

        // ⑤ الخطوة الأخيرة — اعمل الـ packet كامل
        // بيحط الـ header في الأول ثم الداتا
        public byte[] Build()
        {
            // حسب حجم الـ payload (الداتا بدون الـ header)
            // أول 2 bytes هما النوع — الباقي هو الـ payload
            int payloadSize = _buffer.Count - 2;

            // ابني الـ packet النهائي
            var packet = new List<byte>();

            // [ النوع — 2 bytes ] موجود في الـ buffer
            // [ الحجم — 2 bytes ] بنضيفه هنا
            // [ الداتا — باقي bytes ]

            // خد الـ 2 bytes الأولى (النوع)
            packet.AddRange(_buffer.GetRange(0, 2));

            // ضيف الحجم بعد النوع
            packet.AddRange(BitConverter.GetBytes((ushort)payloadSize));

            // ضيف الداتا بعدهم
            packet.AddRange(_buffer.GetRange(2, payloadSize));

            return packet.ToArray();
        }
    }
}