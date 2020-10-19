using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiFeatureStatusData
    {
        public int UniqueId; // identifier of instance
        public int NewStatus;
        public int PrevStatus;
    }
}