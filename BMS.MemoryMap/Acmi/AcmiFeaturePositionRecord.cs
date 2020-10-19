using System.Runtime.InteropServices;

namespace BMS.MemoryMap.Acmi
{
    public class AcmiFeaturePositionRecord : AcmiPositionRecordBase
    {
        public int LeadUniqueId
        { 
            get; 
            set;
        }

        public int Slot
        {
            get;
            set;
        }

        public int SpecialFlags
        {
            get;
            set;
        }

    }
}