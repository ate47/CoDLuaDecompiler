namespace CoDHVKDecompiler.Common.LuaFunction.Structures
{
    public class Instruction
    {
        public LuaOpCode OpCode { get; set; }
        public uint A { get; set; }
        public uint B { get; set; }
        public uint C { get; set; }
        public bool ExtraCBit { get; set; } = false;
        public uint Bx { get; set; }
        public int SBx { get; set; }

        public override string ToString()
        {
            return $"OpCode: {OpCode}, A: {A}, B: {B}, C: {C}, ExtraCBit: {ExtraCBit}, Bx: {Bx}: SBx: {SBx}";
        }
    }
}