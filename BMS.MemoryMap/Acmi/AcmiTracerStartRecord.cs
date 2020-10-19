using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiTracerStartRecord
    {

        public AcmiRecHeader Header;
        public AcmiTracerStartData Data;
    }
}