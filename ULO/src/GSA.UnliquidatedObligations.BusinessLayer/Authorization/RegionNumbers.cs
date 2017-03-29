using System;

namespace GSA.UnliquidatedObligations.BusinessLayer.Authorization
{
    [Flags]
    public enum RegionNumbers
    {
        Region1     = 0x0001,
        Region2     = 0x0002,
        Region3     = 0x0004,
        Region4     = 0x0008,
        Region5     = 0x0010,
        Region6     = 0x0020,
        Region7     = 0x0040,
        Region8     = 0x0080,
        Region9     = 0x0100,
        Region10    = 0x0200,
        Region11    = 0x0400,
        AllRegions  = 
            Region1 |
            Region2 |
            Region3 |
            Region4 |
            Region5 |
            Region6 |
            Region7 |
            Region8 |
            Region9 |
            Region10 |
            Region11,
        NoRegions = 0,
        AnyRegion = 0,
    }
}
