using System;
using System.Globalization;
using System.IO;

namespace CoDLuaDecompiler.Decompiler.LuaFile.Structures.LuaConstant.Havok
{
    public abstract class HavokLuaConstant : IHavokLuaConstant
    {
        // private variables
        protected readonly BinaryReader Reader;

        protected HavokLuaConstant(BinaryReader reader)
        {
            Reader = reader;
            Parse();
        }

        private void Parse()
        {
            Type = (HavokConstantType) Reader.ReadByte();

            switch (Type)
            {
                case HavokConstantType.TString:
                    StringValue = ReadString();
                    break;
                case HavokConstantType.TNumber:
                    NumberValue = ReadNumber();
                    break;
                case HavokConstantType.TBoolean:
                    BoolValue = ReadBool();
                    break;
                case HavokConstantType.THash:
                    HashValue = ReadHash();
                    if (Decompiler.HashEntries.ContainsKey(HashValue))
                    {
                        StringValue = "@\"" + Decompiler.HashEntries[HashValue] + "\"";
                        Type = HavokConstantType.TString;
                    }
                    break;
                case HavokConstantType.TNil:
                    break;
                case HavokConstantType.TUI64:
                    Type = HavokConstantType.THash;
                    HashValue = ReadHash();
                    break;
                case HavokConstantType.TLightUserData:
                case HavokConstantType.TTable:
                case HavokConstantType.TFunction:
                case HavokConstantType.TUserData:
                case HavokConstantType.TThread:
                case HavokConstantType.TIFunction:
                case HavokConstantType.TCFunction:
                case HavokConstantType.TStruct:
                case HavokConstantType.TUnk:
                    throw new NotImplementedException();
                    break;
                default:
                    break; // throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract string ReadString();
        protected abstract double ReadNumber();
        protected abstract bool ReadBool();
        protected abstract ulong ReadHash();

        public override string ToString()
        {
            return Type switch
            {
                HavokConstantType.TString => StringValue,
                HavokConstantType.TNumber => NumberValue.ToString(CultureInfo.InvariantCulture),
                HavokConstantType.TNil => "nil",
                HavokConstantType.TBoolean => BoolValue ? "true" : "false",
                HavokConstantType.THash => $"@\"hash_{HashValue & 0x7FFFFFFFFFFFFFFF:X}\"",
                _ => "NULL"
            };
        }
    }
}