using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiSwitchRecord
    {

        public AcmiRecHeader Header;
        public AcmiSwitchData Data;
    }
}