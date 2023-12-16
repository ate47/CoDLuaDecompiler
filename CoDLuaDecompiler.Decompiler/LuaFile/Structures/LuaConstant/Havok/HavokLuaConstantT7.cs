using System;
using System.IO;
using System.Text;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaConstant.Havok
{
    public class HavokLuaConstantT7 : HavokLuaConstant
    {
        public HavokLuaConstantT7(BinaryReader reader) : base(reader)
        {
        }

        protected override string ReadString()
        {
            var length = Reader.ReadInt32();
            Reader.ReadInt32(); // Unknown, always null
            var str = Encoding.ASCII.GetString(Reader.ReadBytes(length - 1));
            Reader.ReadByte();

            // Check if it's a require string
            if (!String.IsNullOrEmpty(str) && str.Length > 10 && str.StartsWith("x64:"))
            {
                // Grab the hash and check if we have it dehashed
                var hash = Convert.ToUInt64(str.Substring(4), 16) & 0x7FFFFFFFFFFFFFFF;
                if (Decompiler.HashEntries.ContainsKey(hash))
                    str = Decompiler.HashEntries[hash];
                else
                    // Replace the hash with the masked version so it's easier to find the actual file back
                    str = $"x64:{hash:x}";
            }

            return str;
        }

        protected override double ReadNumber()
        {
            return Math.Round(Reader.ReadSingle(), 2, MidpointRounding.ToEven);
        }

        protected override bool ReadBool()
        {
            return Reader.ReadByte() == 1;
        }

        protected override ulong ReadHash()
        {
            return Reader.ReadUInt64() & 0xFFFFFFFFFFFFFFF;
        }
    }
}