namespace BMS.MemoryMap.Acmi
{
    public enum AcmiHeaderType
    {
        //Generic positional data
        AcmiRecGenPosition = 0,
        //Missile position
        AcmiRecMissilePosition,
        //Feature (like bridges and airbases)
        AcmiRecFeaturePosition,
        ///Aircraft position
        AcmiRecAircraftPosition,
        AcmiRecTracerStart,
        AcmiRecStationarySfx,
        AcmiRecMovingSfx,
        AcmiRecSwitch,
        AcmiRecDof,
        AcmiRecChaffPosition,
        AcmiRecFlarePosition,
        AcmiRecTodOffset,
        AcmiRecFeatureStatus,
        AcmiCallsignList,
        AcmiRecMaxTypes
    };
}