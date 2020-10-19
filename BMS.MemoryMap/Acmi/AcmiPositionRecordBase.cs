namespace BMS.MemoryMap.Acmi
{
    public abstract class AcmiPositionRecordBase : AcmiRecordBase
    {
        public int ObjectType
        {
            get;
            set;
        }
        public int UniqueId
        {
            get;
            set;
        }
        public EntityFlags EntityFlag
        {
            get;
            set;
        }
        public float X
        {
            get;
            set;
        }
        public float Y
        {
            get;
            set;
        }
        public float Z
        {
            get;
            set;
        }
        public float Yaw
        {
            get;
            set;
        }
        public float Pitch
        {
            get;
            set;
        }
        public float Roll
        {
            get;
            set;
        }
    }
}