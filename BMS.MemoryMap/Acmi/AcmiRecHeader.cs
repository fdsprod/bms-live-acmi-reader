using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiRecHeader
    {
        public byte Type; // one of the enumerated types
        public float Time; // time stamp
    }
}