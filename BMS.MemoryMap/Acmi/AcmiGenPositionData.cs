using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiGenPositionData
    {
        public int ObjectType; // base type for creating simbase object
        public int UniqueId; // identifier of instance
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public float Pitch;
        public float Roll;
    }
}