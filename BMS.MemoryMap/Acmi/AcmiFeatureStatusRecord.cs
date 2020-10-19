using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiFeatureStatusRecord
    {

        public AcmiRecHeader Header;
        public AcmiFeatureStatusData Data;
    }
}