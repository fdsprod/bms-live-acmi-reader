using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiStationarySfxData
    {
        public int type; // sfx type
        public float X; // position
        public float Y;
        public float Z;
        public float TimeToLive;
        public float Scale;
    }
}