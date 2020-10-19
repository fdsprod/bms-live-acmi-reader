using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcmiTapeHeader
    {
        public int FileId;
        public int FileSize;
        public int NumEntities;
        public int NumFeat;
        public int EntityBlockOffset; // 5th
        public int FeatBlockOffset;
        public int NumEntityPositions;
        public int TimelineBlockOffset;
        public int FirstEntEventOffset;
        public int FirstGeneralEventOffset; // 10th => 40 Bytes
        public int FirstEventTrailerOffset;
        public int FirstTextEventOffset;
        public int FirstFeatEventOffset;
        public int NumEvents;
        public int NumEntEvents; // 15th => 60 Byte
        public int NumTextEvents;
        public int NumFeatEvents;
        public float StartTime;
        public float TotPlayTime;
        public float TodOffset;
    }
}
