using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiDofRecord
    {

        public AcmiRecHeader Header;
        public AcmiDofData Data;
    }
}