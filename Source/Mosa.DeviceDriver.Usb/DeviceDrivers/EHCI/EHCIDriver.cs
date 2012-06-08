using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mosa.DeviceSystem;

namespace Mosa.CoolWorld.x86.DeviceDrivers.EHCI
{
    public class EHCIDriver : DeviceDriver
    {
        public EHCIDriver() : base (new PCIDeviceDriverAttribute
                                 {
                                   VendorID = 0x15AD, 
                                   DeviceID = 0x0770, 
                                   Platforms = PlatformArchitecture.X86AndX64  
                                 }, new Type() )
        {
        }
    }
}
