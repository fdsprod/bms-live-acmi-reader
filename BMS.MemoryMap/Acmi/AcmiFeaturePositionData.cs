using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiFeaturePositionData
    {
        public int Type; // base type for creating simbase object
        public int UniqueId; // identifier of instance
        public int LeadUniqueId; // id of lead component (for bridges. bases etc)
        public int Slot; // slot number in component list
        public int SpecialFlags; // campaign feature flag
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public float Pitch;
        public float Roll;
    }
}