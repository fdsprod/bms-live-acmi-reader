using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiTextEvent
    {
        public int IntTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string TimeStr;//[20];
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string MsgStr; //[100];
    }
}