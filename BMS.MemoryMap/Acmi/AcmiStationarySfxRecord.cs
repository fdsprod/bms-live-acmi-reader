using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiStationarySfxRecord
    {

        public AcmiRecHeader Header;
        public AcmiStationarySfxData Data;
    }
}