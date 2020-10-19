using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiMovingSfxRecord
    {
        public AcmiRecHeader Header;
        public AcmiMovingSfxData Data;
    }
}