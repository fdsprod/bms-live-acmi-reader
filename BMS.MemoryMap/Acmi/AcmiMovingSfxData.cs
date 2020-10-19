using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiMovingSfxData
    {

        public int Type; // sfx type
        public int User; // misc data
        public int Flags;
        public float X; // position
        public float Y;
        public float Z;
        public float Dx; // vector
        public float Dy;
        public float Dz;
        public float TimeToLive;
        public float Scale;
    }
}