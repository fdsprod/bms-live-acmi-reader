using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiTracerStartData
    {
        public float X;
        public float Y;
        public float Z;
        public float Dx;
        public float Dy;
        public float Dz;
    }
}