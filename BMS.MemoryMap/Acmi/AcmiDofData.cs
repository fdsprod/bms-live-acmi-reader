using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiDofData
    {

        public int Type; // base type for creating simbase object
        public int UniqueId; // identifier of instance
        public int DofNum;
        public float DofVal;
        public float PrevDofVal;
    }
}