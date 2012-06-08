using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Mosa.DeviceSystem;
using Mosa.DeviceSystem.PCI;

namespace Mosa.DeviceDrivers.HCI
{
    /// <summary>
    /// 
    /// </summary>

    [PCIDeviceDriver(VendorID = 0x15AD, DeviceID = 0x0770, Platforms = PlatformArchitecture.X86AndX64)]
	[DeviceDriverPhysicalMemory(MemorySize = 7 * 4, MemoryAlignment = 4, RestrictUnder4G = true)]
	[DeviceDriverPhysicalMemory(MemorySize = 80 * 4, MemoryAlignment = 16, RestrictUnder4G = true)]
	[DeviceDriverPhysicalMemory(MemorySize = 80 * 4, MemoryAlignment = 16, RestrictUnder4G = true)]
	[DeviceDriverPhysicalMemory(MemorySize = 2048 * 32, MemoryAlignment = 1, RestrictUnder4G = true)]
    public class EHCI  : HardwareDevice
    {
        private const ushort VendorId = 0x15AD; // VMWare Inc.
        private const ushort DeviceId = 0x0770; // Standard Enhanced PCI to USB Host Controller

        /// <summary>
        /// 
        /// </summary>
        public enum Register : ushort
        {
            /// <summary>
            /// Register implementation as needed for specific PCI device
            /// </summary>
            ID = 0x00,

            /// <summary>
            /// Class Code  
            /// 
            /// Default Value:  0C0320h 
            /// Size: 24 bits 
            /// Attribute: RO 
            /// Address Offset:  09−0Bh 
            /// 
            /// This register contains the device programming interface information related to the Sub-Class Code and Base 
            /// Class Code definition. This register also identifies the Base Class Code and the function sub-class in relation 
            /// to the Base Class Code.
            /// 
            /// Bit     Description 
            /// 23:16   Base Class Code (BASEC).  0Ch= Serial Bus controller. 
            /// 15:8    Sub-Class Code (SCC). 03h=Universal Serial Bus Host Controller. 
            /// 7:0     Programming Interface (PI). 20h=USB 2.0 Host Controller that conforms to this specification.
            /// </summary>
            ClassCode = 0x09,

            /// <summary>
            /// Register implementation as needed for specific PCI device 
            /// </summary>
            Unknown1 = 0x0C,

            /// <summary>
            /// USBBASE  Register Space Base Address Register  
            /// 
            /// Address Offset:  10−13h 
            /// Default Value:  Implementation Dependent 
            /// Attribute: R/W 
            /// Size: 32 bits 
            /// 
            /// This register contains the base address of the DWord-aligned memory-mapped host controller Registers. The 
            /// number of writable bits in this register determines the actual size of the required memory space window. The 
            /// minimum required is specified in this specification. Individual implementations may vary.
            /// 
            /// Bit     Description 
            /// 31:8    Base Address R/W.  Corresponds to memory address signals [31:8].  
            /// 7:3     Reserved. RO.  This bits are read only and hardwired to zero. 
            /// 2:1     Type. RO. This field has two valid values: 
            ///             Value   Meaning 
            ///             00b     May only be mapped into 32-bit addressing space (Recommended). 
            ///             10b     May be mapped into 64-bit addressing space.  
            /// 0       Reserved RO. This bit is read only and hardwired to zero
            /// </summary>
            UsbBase = 0x10,

            /// <summary>
            /// Register implementation as needed for specific PCI device
            /// </summary>
            Unknown2 = 0x14,

            /// <summary>
            /// SBRN Serial Bus Release Number Register  
            /// 
            /// Address Offset:  60h 
            /// Default Value:  See Description below 
            /// Attribute: RO 
            /// Size: 8 bits 
            /// 
            /// This register contains the release of the Universal Serial Bus Specification with which this Universal Serial 
            /// Bus Host Controller module is compliant. 
            /// 
            /// Bit     Description 
            /// 7:0     Serial Bus Specification Release Number. All other combinations are reserved. 
            ///         Bits[7:0]   Release Number 
            ///         20h         Release 2.0
            /// </summary>
            SBRN = 0x60,

            /// <summary>
            /// Frame Length Adjustment Register (FLADJ) 
            /// 
            /// Address Offset:  61h 
            /// Default Value:  20h 
            /// Attribute: R/W 
            /// Size: 8 bits 
            ///  
            /// This register is in the auxiliary power well. This feature is used to adjust any offset from the clock source 
            /// that generates the clock that drives the SOF counter. When a new value is written into these six bits, the 
            /// length of the frame is adjusted. Its initial programmed value is system dependent based on the accuracy of 
            /// hardware USB clock and is initialized by system BIOS. This register should only be modified when the 
            /// HCHalted bit in the USBSTS register is a one.  Changing value of this register while the host controller is 
            /// operating yields undefined results. It should not be reprogrammed by USB system software unless the 
            /// default or BIOS programmed values are incorrect, or the system is restoring the register while returning 
            /// from a suspended state.  
            /// 
            /// Bit     Description 
            /// 7:6     Reserved. These bits are reserved for future use and should read as zero. 
            /// 5:0     Frame Length Timing Value. Each decimal value change to this register corresponds to 16 high-speed bit times. 
            ///         The SOF cycle time (number of SOF counter clock periods to generate a SOF micro-frame length) 
            ///         is equal to 59488 + value in this field. The default value is decimal 32 (20h), 
            ///         which gives a SOF cycle time of 60000.  
            ///         Frame Length 
            ///         (# High Speed bit times)    FLADJ Value 
            ///         (decimal)                   (decimal) 
            ///         59488                       0 (00h) 
            ///         59504                       1 (01h) 
            ///         59520                       2 (02h) 
            ///         …  
            ///         59984                       31 (1Fh) 
            ///         60000                       32 (20h) 
            ///         … 
            ///         60480                       62 (3Eh) 
            ///         60496                       63 (3Fh)  
            /// </summary>
            FLADJ = 0x61,

            /// <summary>
            /// Port Wake Capability Register (PORTWAKECAP) 
            /// 
            /// Address Offset:  62h 
            /// Default Value:  Implementation Dependent 
            /// Attribute: R/W 
            /// Size: 16 bits 
            ///  
            /// This register is optional. When implemented this register is in the auxiliary power well. The intended use of 
            /// this register is to establish a policy about which ports are to be used for wake events. Bit positions 1-15 in 
            /// the mask correspond to a physical port implemented on the current EHCI controller. A one in a bit position 
            /// indicates that a device connected below the port can be enabled as a wake-up device and the port may be 
            /// enabled for disconnect/connect or over-current events as wake-up events. This is an information only mask 
            /// register. The bits in this register DO NOT affect the actual operation of the EHCI host controller. The 
            /// system-specific policy can be established by BIOS initializing this register to a system-specific value. 
            /// System software uses the information in this register when enabling devices and ports for remote wake-up. 
            /// 
            /// Bit     Description 
            /// 15:0    Port Wake Up Capability Mask. Bit position zero of this register indicates whether the register is 
            ///         implemented. A one in bit position zero indicates that the register is implemented. Bit positions 1 
            ///         through 15 correspond to a physical port implemented on this host controller. For example, bit 
            ///         position 1 corresponds to port 1, position 2 to port 2, etc. 
            /// </summary>
            PORTWAKECAP = 0x62,

            /// <summary>
            /// Register implementation as needed for specific PCI device 
            /// </summary>
            Unknown3 = 0x63,
        }

        /// <summary>
        /// Memory-mapped USB Host Controller Registers. This block of registers is memory-mapped into 
        /// non-cacheable memory (see Figure 1-3). This memory space must begin on a DWord (32-bit) boundary. 
        /// This register space is divided into two sections: a set of read-only capability registers and a set of 
        /// read/write operational registers. Table 2-1, describes each register space. 
        /// Note that host controllers are not required to support exclusive-access mechanisms (such as PCI LOCK) 
        /// for accesses to the memory-mapped register space. Therefore, if software attempts exclusive-access 
        /// mechanisms to the host controller memory-mapped register space, the results are undefined.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CapabilityRegister
        {
            /// <summary>
            /// CAPLENGTH Capability Registers Length 
            /// 
            /// Address: Base+ (00h) 
            /// Default Value  Implementation Dependent 
            /// Attribute: RO 
            /// Size: 8 bits 
            ///  
            /// This register is used as an offset to add to register base to find the beginning of the Operational Register 
            /// Space.
            /// </summary>
            public byte CAPLENGTH;

            /// <summary>
            /// N/A
            /// </summary>
            public byte Reserved;

            /// <summary>
            /// HCIVERSION Host Controller Interface Version Number 
            /// 
            /// Address: Base+ (02h) 
            /// Default Value:  0100h 
            /// Attribute RO 
            /// Size: 16 bits 
            ///  
            /// This is a two-byte register containing a BCD encoding of the EHCI revision number supported by this host 
            /// controller. The most significant byte of this register represents a major revision and the least significant byte 
            /// is the minor revision. 
            /// </summary>
            public ushort HCIVERSION;

            /// <summary>
            /// HCSPARAMS Structural Parameters 
            /// 
            /// Address: Base+ (04h) 
            /// Default Value  Implementation Dependent 
            /// Attribute RO 
            /// Size:  32 bits  
            ///  
            /// This is a set of fields that are structural parameters: Number of downstream ports, etc. 
            /// 
            /// Bit     Description 
            /// 31:24   Reserved. These bits are reserved and should be set to zero. 
            /// 23:20   Debug Port Number. Optional. This register identifies which of the host controller ports 
            ///         is the debug port. The value is the port number (one-based) of the debug port. A non-zero value in this field indicates the presence of a debug port. The value in this register 
            ///         must not be greater than N_PORTS (see below). 
            /// 19:17   Reserved. These bits are reserved and should be set to zero. 
            /// 16      Port Indicators (P_INDICATOR). This bit indicates whether the ports support port 
            ///         indicator control. When this bit is a one, the port status and control registers include a 
            ///         read/writeable field for controlling the state of the port indicator. See Section 2.3.9 for 
            ///         definition of the port indicator control field.  
            /// 15:12   Number of Companion Controller (N_CC). This field indicates the number of 
            ///         companion controllers associated with this USB 2.0 host controller.  
            ///         A zero in this field indicates there are no companion host controllers. Port-ownership 
            ///         hand-off is not supported. Only high-speed devices are supported on the host controller 
            ///         root ports. 
            ///         A value larger than zero in this field indicates there are companion USB 1.1 host 
            ///         controller(s). Port-ownership hand-offs are supported. High, Full- and Low-speed 
            ///         devices are supported on the host controller root ports. 
            /// 11:8    Number of Ports per Companion Controller (N_PCC). This field indicates the number 
            ///         of ports supported per companion host controller. It is used to indicate the port routing 
            ///         configuration to system software.  
            ///         For example, if N_PORTS has a value of 6 and N_CC has a value of 2 then N_PCC 
            ///         could have a value of 3. The convention is that the first N_PCC ports are assumed to be 
            ///         routed to companion controller 1, the next N_PCC ports to companion controller 2, etc. 
            ///         In the previous example, the N_PCC could have been 4, where the first 4 are routed to 
            ///         companion controller 1 and the last two are routed to companion controller 2. 
            ///         The number in this field must be consistent with N_PORTS and N_CC.  
            /// 7       Port Routing Rules. This field indicates the method used by this implementation for 
            ///         how all ports are mapped to companion controllers. The value of this field has the 
            ///         following interpretation: 
            ///         Value   Meaning 
            ///         0       The first N_PCC ports are routed to the lowest numbered function 
            ///                 companion host controller, the next N_PCC port are routed to the next 
            ///                 lowest function companion controller, and so on. 
            ///         1       The port routing is explicitly enumerated by the first N_PORTS elements 
            ///                 of the HCSP-PORTROUTE array. 
            /// 6:5     Reserved. These bits are reserved and should be set to zero. 
            /// 4       Port Power Control (PPC). This field indicates whether the host controller 
            ///         implementation includes port power control. A one in this bit indicates the ports have 
            ///         port power switches. A zero in this bit indicates the port do not have port power 
            ///         switches. The value of this field affects the functionality of the Port Power field in each 
            ///         port status and control register (see Section 2.3.8). 
            /// 3:0     N_PORTS. This field specifies the number of physical downstream ports implemented 
            ///         on this host controller. The value of this field determines how many port registers are 
            ///         addressable in the Operational Register Space (see Table 2-8). Valid values are in the 
            ///         range of 1H to FH.  
            ///         A zero in this field is undefined. 
            /// </summary>
            public uint HCSPARAMS;

            /// <summary>
            /// HCCPARAMS Capability Parameters 
            /// 
            /// Address: Base+ (08h) 
            /// Default Value  Implementation Dependent 
            /// Attribute RO 
            /// Size:  32 bits  
            ///  
            /// Multiple Mode control (time-base bit functionality), addressing capability 
            /// 
            /// Bit     Description 
            /// 31:16   Reserved. These bits are reserved and should be set to zero. 
            /// 15:8    EHCI Extended Capabilities Pointer (EECP). Default = Implementation Dependent. 
            ///         This optional field indicates the existence of a capabilities list. A value of 00h indicates 
            ///         no extended capabilities are implemented. A non-zero value in this register indicates the 
            ///         offset in PCI configuration space of the first EHCI extended capability. The pointer value 
            ///         must be 40h or greater if implemented to maintain the consistency of the PCI header 
            ///         defined for this class of device. 
            /// 7:4     Isochronous Scheduling Threshold. Default = implementation dependent. This field 
            ///         indicates, relative to the current position of the executing host controller, where software 
            ///         can reliably update the isochronous schedule. When bit [7] is zero, the value of the least 
            ///         significant 3 bits indicates the number of micro-frames a host controller can hold a set of 
            ///         isochronous data structures (one or more) before flushing the state. When bit [7] is a 
            ///         one, then host software assumes the host controller may cache an isochronous data 
            ///         structure for an entire frame. Refer to Section 4.7.2.1 for details on how software uses 
            ///         this information for scheduling isochronous transfers. 
            /// 3       Reserved. This bit is reserved and should be set to zero.
            /// 2       Asynchronous Schedule Park Capability. Default = Implementation dependent. If this 
            ///         bit is set to a one, then the host controller supports the park feature for high-speed 
            ///         queue heads in the Asynchronous Schedule. The feature can be disabled or enabled 
            ///         and set to a specific level by using the Asynchronous Schedule Park Mode Enable and 
            ///         Asynchronous Schedule Park Mode Count fields in the USBCMD register. 
            /// 1       Programmable Frame List Flag. Default = Implementation dependent. If this bit is set 
            ///         to a zero, then system software must use a frame list length of 1024 elements with this 
            ///         host controller. The USBCMD register Frame List Size field is a read-only register and 
            ///         should be set to zero. 
            ///         If set to a one, then system software can specify and use a smaller frame list and 
            ///         configure the host controller via the USBCMD register Frame List Size field. The frame 
            ///         list must always be aligned on a 4K page boundary. This requirement ensures that the 
            ///         frame list is always physically contiguous. 
            /// 0       64-bit Addressing Capability.
            ///         This field documents the addressing range capability of 
            ///         this implementation. The value of this field determines whether software should use the 
            ///         data structures defined in Section 3 (32-bit) or those defined in Appendix B (64-bit). 
            ///         Values for this field have the following interpretation: 
            ///         0b  data structures using 32-bit address memory pointers 
            ///         1b  data structures using 64-bit address memory pointers  
            ///         This is not tightly coupled with the USBBASE address register mapping control. The 64-bit Addressing 
            ///         Capability bit indicates whether the host controller can generate 64-bit addresses as a master. The USBBASE 
            ///         register indicates the host controller only needs to decode 32-bit addresses as a slave. 
            /// </summary>
            public uint HCCPARAMS;

            /// <summary>
            /// HCSP-PORTROUTE Companion Port Route Description 
            /// 
            /// Address: Base+ (0Ch) 
            /// Default Value  Implementation Dependent 
            /// Attribute RO 
            /// Size:  60 bits  
            ///  
            /// This optional field is valid only if Port Routing Rules field in the HCSPARAMS register is set to a one.  
            /// The rules for organizing companion host controllers and an EHCI host controllers within PCI space are 
            /// described in detail in Section 4.2. This field is used to allow a host controller implementation to explicitly 
            /// described to which companion host controller each implemented port is mapped.  
            /// This field is a 15-element nibble array (each 4 bits is one array element). Each array location corresponds 
            /// one-to-one with a physical port provided by the host controller (e.g. PORTROUTE[0] corresponds to the 
            /// first PORTSC port, PORTROUTE[1] to the second PORTSC port, etc.). The value of each element indicates 
            /// to which of the companion host controllers this port is routed. Only the first N_PORTS elements have valid 
            /// information. A value of zero indicates that the port is routed to the lowest numbered function companion 
            /// host controller. A value of one indicates that the port is routed to the next lowest numbered function 
            /// companion host controller, and so on.
            /// </summary>
            public ulong HCSP_PORTROUTE;
        }

        /// <summary>
        /// This section defines the enhanced host controller operational registers. These registers are located after the 
        /// capabilities registers (see Section 2.1.7). The operational register base must be DWord aligned and is 
        /// calculated by adding the value in the first capabilities register (CAPLENGTH, Section 2.2.1) to the base 
        /// address of the enhanced host controller register address space. All registers are 32 bits in length. Software 
        /// should read and write these registers using only DWord accesses. 
        /// These registers are divided into two sets. The first set at addresses 00h to 3Fh are implemented in the core 
        /// power well. The second set at addresses 40h to the end of the implemented register space are implemented in 
        /// the auxiliary power well.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OperationalRegister
        {
            /// <summary>
            /// USBCMD USB Command Register 
            /// 
            /// Address:  Operational Base+ (00h) 
            /// Default Value:  00080000h (00080B00h if Asynchronous Schedule Park Capability is a one) 
            /// Attribute  RO, R/W (field dependent), WO 
            /// Size 32 bits 
            /// 
            /// The Command Register indicates the command to be executed by the serial bus host controller. Writing to 
            /// the register causes a command to be executed.  
            /// 
            /// Bit     Description 
            /// 31:24   Reserved. These bits are reserved and should be set to zero. 
            /// 23:16   Interrupt Threshold Control  R/W. Default 08h. This field is used by system software 
            ///         to select the maximum rate at which the host controller will issue interrupts. The only 
            ///         valid values are defined below. If software writes an invalid value to this register, the 
            ///         results are undefined.  
            ///         Value   Maximum Interrupt Interval 
            ///         00h     Reserved 
            ///         01h     1 micro-frame 
            ///         02h     2 micro-frames 
            ///         04h     4 micro-frames  
            ///         08h     8 micro-frames (default, equates to 1 ms) 
            ///         10h     16 micro-frames (2 ms) 
            ///         20h     32 micro-frames (4 ms) 
            ///         40h     64 micro-frames (8 ms) 
            ///         Refer to Section 4.15 for interrupts affected by this register. Any other value in this 
            ///         register yields undefined results. 
            ///         Software modifications to this bit while HCHalted bit is equal to zero results in undefined 
            ///         behavior. 
            /// 15:12   Reserved. These bits are reserved and should be set to zero. 
            /// 11      Asynchronous Schedule Park Mode Enable (OPTIONAL)  RO or R/W. If the 
            ///         Asynchronous Park Capability bit in the HCCPARAMS register is a one, then this bit 
            ///         defaults to a 1h and is R/W. Otherwise the bit must be a zero and is RO. Software uses 
            ///         this bit to enable or disable Park mode. When this bit is one, Park mode is enabled. 
            ///         When this bit is a zero, Park mode is disabled. 
            /// 10      Reserved. This bit is reserved and should be set to zero. 
            /// 9:8     Asynchronous Schedule Park Mode Count (OPTIONAL)  RO or R/W. If the 
            ///         Asynchronous Park Capability bit in the HCCPARAMS register is a one, then this field 
            ///         defaults to 3h and is R/W. Otherwise it defaults to zero and is RO. It contains a count of 
            ///         the number of successive transactions the host controller is allowed to execute from a 
            ///         high-speed queue head on the Asynchronous schedule before continuing traversal of 
            ///         the Asynchronous schedule. See Section 4.10.3.2 for full operational details. Valid 
            ///         values are 1h to 3h. Software must not write a zero to this bit when Park Mode Enable is 
            ///         a one as this will result in undefined behavior. 
            /// 7       Light Host Controller Reset (OPTIONAL)  R/W. This control bit is not required. If 
            ///         implemented, it allows the driver to reset the EHCI controller without affecting the state 
            ///         of the ports or the relationship to the companion host controllers. For example, the 
            ///         PORSTC registers should not be reset to their default values and the CF bit setting 
            ///         should not go to zero (retaining port ownership relationships).  
            ///         A host software read of this bit as zero indicates the Light Host Controller Reset has 
            ///         completed and it is safe for host software to re-initialize the host controller. A host 
            ///         software read of this bit as a one indicates the Light Host Controller Reset has not yet 
            ///         completed. 
            ///         If not implemented a read of this field will always return a zero. 
            /// 6       Interrupt on Async Advance Doorbell  R/W. This bit is used as a doorbell by 
            ///         software to tell the host controller to issue an interrupt the next time it advances 
            ///         asynchronous schedule. Software must write a 1 to this bit to ring the doorbell.  
            ///         When the host controller has evicted all appropriate cached schedule state, it sets the 
            ///         Interrupt on Async Advance status bit in the USBSTS register. If the Interrupt on Async 
            ///         Advance Enable bit in the USBINTR register is a one then the host controller will assert 
            ///         an interrupt at the next interrupt threshold. See Section 4.8.2 for operational details. 
            ///         The host controller sets this bit to a zero after it has set the Interrupt on Async Advance 
            ///         status bit in the USBSTS register to a one. 
            ///         Software should not write a one to this bit when the asynchronous schedule is disabled. 
            ///         Doing so will yield undefined results. 
            /// 5       Asynchronous Schedule Enable  R/W. Default 0b. This bit controls whether the host 
            ///         controller skips processing the Asynchronous Schedule. Values mean: 
            ///         0b      Do not process the Asynchronous Schedule 
            ///         1b      Use the ASYNCLISTADDR register to access the Asynchronous Schedule. 
            /// 4       Periodic Schedule Enable  R/W. Default 0b. This bit controls whether the host 
            ///         controller skips processing the Periodic Schedule. Values mean: 
            ///         0b      Do not process the Periodic Schedule 
            ///         1b      Use the PERIODICLISTBASE register to access the Periodic Schedule. 
            /// 3:2     Frame List Size  (R/W or RO). Default 00b. This field is R/W only if Programmable 
            ///         Frame List Flag in the HCCPARAMS registers is set to a one. This field specifies the 
            ///         size of the frame list. The size the frame list controls which bits in the Frame Index 
            ///         Register should be used for the Frame List Current index. Values mean: 
            ///         00b     1024 elements (4096 bytes) Default value 
            ///         01b     512 elements (2048 bytes) 
            ///         10b     256 elements (1024 bytes) – for resource-constrained environments 
            ///         11b     Reserved 
            /// 1       Host Controller Reset (HCRESET)  R/W. This control bit is used by software to reset 
            ///         the host controller. The effects of this on Root Hub registers are similar to a Chip 
            ///         Hardware Reset.  
            ///         When software writes a one to this bit, the Host Controller resets its internal pipelines, 
            ///         timers, counters, state machines, etc. to their initial value. Any transaction currently in 
            ///         progress on USB is immediately terminated. A USB reset is not driven on downstream 
            ///         ports. 
            ///         PCI Configuration registers are not affected by this reset. All operational registers, 
            ///         including port registers and port state machines are set to their initial values. Port 
            ///         ownership reverts to the companion host controller(s), with the side effects described in 
            ///         Section 4.2. Software must reinitialize the host controller as described in Section 4.1 in 
            ///         order to return the host controller to an operational state. 
            ///         This bit is set to zero by the Host Controller when the reset process is complete. 
            ///         Software cannot terminate the reset process early by writing a zero to this register.  
            ///         Software should not set this bit to a one when the HCHalted bit in the USBSTS register 
            ///         is a zero. Attempting to reset an actively running host controller will result in undefined 
            ///         behavior. 
            /// 0       Run/Stop (RS) R/W.  Default 0b. 1=Run. 0=Stop. When set to a 1, the Host Controller 
            ///         proceeds with execution of the schedule. The Host Controller continues execution as 
            ///         long as this bit is set to a 1. When this bit is set to 0, the Host Controller completes the 
            ///         current and any actively pipelined transactions on the USB and then halts. The Host 
            ///         Controller must halt within 16 micro-frames after software clears the Run bit. The HC 
            ///         Halted bit in the status register indicates when the Host Controller has finished its 
            ///         pending pipelined transactions and has entered the stopped state. Software must not 
            ///         write a one to this field unless the host controller is in the Halted state (i.e. HCHalted in 
            ///         the USBSTS register is a one). Doing so will yield undefined results. 
            /// </summary>
            public uint USBCMD;

            /// <summary>
            /// USBSTS USB Status Register 
            /// 
            /// Address:  Operational Base + (04h) 
            /// Default Value:  00001000h 
            /// Attribute  RO, R/W, R/WC, (field dependent) 
            /// Size 32 bits 
            /// 
            /// This register indicates pending interrupts and various states of the Host Controller. The status resulting from 
            /// a transaction on the serial bus is not indicated in this register. Software sets a bit to 0 in this register by 
            /// writing a 1 to it.  See Section 4.15 for additional information concerning USB interrupt conditions. 
            /// 
            /// Bit     Description 
            /// 31:16   Reserved. These bits are reserved and should be set to zero. 
            /// 15      Asynchronous Schedule Status  RO. 0=Default. The bit reports the current real 
            ///         status of the Asynchronous Schedule. If this bit is a zero then the status of the 
            ///         Asynchronous Schedule is disabled. If this bit is a one then the status of the 
            ///         Asynchronous Schedule is enabled. The Host Controller is not required to immediately 
            ///         disable or enable the Asynchronous Schedule when software transitions the 
            ///         Asynchronous Schedule Enable bit in the USBCMD register. When this bit and the 
            ///         Asynchronous Schedule Enable bit are the same value, the Asynchronous Schedule is 
            ///         either enabled (1) or disabled (0). 
            /// 14      Periodic Schedule Status  RO. 0=Default. The bit reports the current real status of 
            ///         the Periodic Schedule. If this bit is a zero then the status of the Periodic Schedule is 
            ///         disabled. If this bit is a one then the status of the Periodic Schedule is enabled. The 
            ///         Host Controller is not required to immediately disable or enable the Periodic Schedule 
            ///         when software transitions the Periodic Schedule Enable bit in the USBCMD register. 
            ///         When this bit and the Periodic Schedule Enable bit are the same value, the Periodic 
            ///         Schedule is either enabled (1) or disabled (0).  
            /// 13      Reclamation  RO. 0=Default. This is a read-only status bit, which is used to detect an 
            ///         empty asynchronous schedule. The operational model of empty schedule detection is 
            ///         described in Section 4.8.3. The valid transitions for this bit are described in Section 
            ///         4.8.6.  
            /// 12      HCHalted  RO.  1=Default. This bit is a zero whenever the Run/Stop bit is a one. The 
            ///         Host Controller sets this bit to one after it has stopped executing as a result of the 
            ///         Run/Stop bit being set to 0, either by software or by the Host Controller hardware (e.g. 
            ///         internal error). 
            /// 11:6    Reserved. These bits are reserved and should be set to zero. 
            /// 5       Interrupt on Async Advance  R/WC.  0=Default. System software can force the host 
            ///         controller to issue an interrupt the next time the host controller advances the 
            ///         asynchronous schedule by writing a one to the Interrupt on Async Advance Doorbell bit 
            ///         in the USBCMD register. This status bit indicates the assertion of that interrupt source. 
            /// 4       Host System Error  R/WC. The Host Controller sets this bit to 1 when a serious error 
            ///         occurs during a host system access involving the Host Controller module.  In a PCI 
            ///         system, conditions that set this bit to 1 include PCI Parity error, PCI Master Abort, and 
            ///         PCI Target Abort. When this error occurs, the Host Controller clears the Run/Stop bit in 
            ///         the Command register to prevent further execution of the scheduled TDs.  
            /// 3       Frame List Rollover  R/WC. The Host Controller sets this bit to a one when the 
            ///         Frame List Index (see Section 2.3.4) rolls over from its maximum value to zero. The 
            ///         exact value at which the rollover occurs depends on the frame list size. For example, if 
            ///         the frame list size (as programmed in the Frame List Size field of the USBCMD register) 
            ///         is 1024, the Frame Index Register rolls over every time FRINDEX[13] toggles. Similarly, 
            ///         if the size is 512, the Host Controller sets this bit to a one every time FRINDEX[12] 
            ///         toggles. 
            /// 2       Port Change Detect  R/WC. The Host Controller sets this bit to a one when any port 
            ///         for which the Port Owner bit is set to zero (see Section 2.3.9) has a change bit transition 
            ///         from a zero to a one or a Force Port Resume bit transition from a zero to a one as a 
            ///         result of a J-K transition detected on a suspended port. This bit will also be set as a 
            ///         result of the Connect Status Change being set to a one after system software has 
            ///         relinquished ownership of a connected port by writing a one to a port's Port Owner bit 
            ///         (see Section 4.2.2). 
            ///         This bit is allowed to be maintained in the Auxiliary power well. Alternatively, it is also 
            ///         acceptable that on a D3 to D0 transition of the EHCI HC device, this bit is loaded with 
            ///         the OR of all of the PORTSC change bits (including: Force port resume, over-current 
            ///         change, enable/disable change and connect status change). 
            /// 1       USB Error Interrupt (USBERRINT)  R/WC.  The Host Controller sets this bit to 1 
            ///         when completion of a USB transaction results in an error condition (e.g., error counter 
            ///         underflow). If the TD on which the error interrupt occurred also had its IOC bit set, both 
            ///         this bit and USBINT bit are set. See Section 4.15.1 for a list of the USB errors that will 
            ///         result in this bit being set to a one. 
            /// 0       USB Interrupt (USBINT)  R/WC.  The Host Controller sets this bit to 1 on the 
            ///         completion of a USB transaction, which results in the retirement of a Transfer Descriptor 
            ///         that had its IOC bit set.  
            ///         The Host Controller also sets this bit to 1 when a short packet is detected (actual 
            ///         number of bytes received was less than the expected number of bytes).  
            /// </summary>
            public uint USBSTS;

            /// <summary>
            /// USBINTR USB Interrupt Enable Register 
            /// 
            /// Address:  Operational Base + (08h) 
            /// Default Value:  00000000h 
            /// Attributes R/W 
            /// Size 32 bits 
            /// 
            /// This register enables and disables reporting of the corresponding interrupt to the software. When a bit is set 
            /// and the corresponding interrupt is active, an interrupt is generated to the host. Interrupt sources that are 
            /// disabled in this register still appear in the USBSTS to allow the software to poll for events.  
            /// Each interrupt enable bit description indicates whether it is dependent on the interrupt threshold mechanism 
            /// (see Section 4.15). 
            /// 
            /// Bit     Interrupt Source  Description 
            /// 31:6    Reserved.   These bits are reserved and should be zero. 
            /// 5       Interrupt on Async Advance Enable  When this bit is a one, and the Interrupt on 
            ///         Async Advance bit in the USBSTS register is a one, the host controller will issue an 
            ///         interrupt at the next interrupt threshold. The interrupt is acknowledged by software 
            ///         clearing the Interrupt on Async Advance bit. 
            /// 4       Host System Error Enable  When this bit is a one, and the Host System 
            ///         Error Status bit in the USBSTS register is a one, the host controller will issue an 
            ///         interrupt. The interrupt is acknowledged by software clearing the Host System Error bit. 
            /// 3       Frame List Rollover Enable.   When this bit is a one, and the Frame List 
            ///         Rollover bit in the USBSTS register is a one, the host controller will issue an 
            ///         interrupt.  The interrupt is acknowledged by software clearing the Frame List Rollover bit. 
            /// 2       Port Change Interrupt Enable.  When this bit is a one, and the Port Change 
            ///         Detect bit in the USBSTS register is a one, the host controller will issue an interrupt. 
            ///         The interrupt is acknowledged by software clearing the Port Change Detect bit. 
            /// 1       USB Error Interrupt Enable.   When this bit is a one, and the USBERRINT 
            ///         bit in the USBSTS register is a one, the host controller will issue an interrupt at the next 
            ///         interrupt threshold. The interrupt is acknowledged by software clearing the USBERRINT bit.  
            /// 0       USB Interrupt Enable.   When this bit is a one, and the USBINT bit 
            ///         in the USBSTS register is a one, the host controller will issue an interrupt at the next 
            ///         interrupt threshold. The interrupt is acknowledged by software clearing the USBINT bit. 
            /// </summary>
            public uint USBINTR;

            /// <summary>
            /// FRINDEX  Frame Index Register 
            /// 
            /// Address:  Operational Base + (0Ch) 
            /// Default Value:  00000000h 
            /// Attribute:  R/W (Writes must be DWord Writes)  
            /// Size 32 bits 

            /// This register is used by the host controller to index into the periodic frame list. The register updates every 
            /// 125 microseconds (once each micro-frame). Bits [N:3] are used to select a particular entry in the Periodic 
            /// Frame List during periodic schedule execution. The number of bits used for the index depends on the size of 
            /// the frame list as set by system software in the Frame List Size field in the USBCMD register (see Table 2-9). 
            /// This register must be written as a DWord. Byte writes produce undefined results. This register cannot be 
            /// written unless the Host Controller is in the Halted state as indicated by the HCHalted bit (USBSTS register 
            /// Section 2.3.2). A write to this register while the Run/Stop bit is set to a one (USBCMD register, Section 
            /// 2.3.1) produces undefined results. Writes to this register also affect the SOF value. See Section 4.5 for 
            /// details.

            /// Bit     Description 
            /// 31:14   Reserved.  
            /// 13:0    Frame Index. The value in this register increments at the end of each time frame (e.g. 
            ///         micro-frame). Bits [N:3] are used for the Frame List current index. This means that each 
            ///         location of the frame list is accessed 8 times (frames or micro-frames) before moving to 
            ///         the next index. The following illustrates values of N based on the value of the Frame List 
            ///         Size field in the USBCMD register. 
            ///         USBCMD[Frame List Size]     Number Elements     N 
            ///         00b                         (1024)              12 
            ///         01b                         (512)               11 
            ///         10b                         (256)               10 
            ///         11b                         Reserved 
            ///         The SOF frame number value for the bus SOF token is derived or alternatively managed from this register. 
            ///         Please refer to Section 4.5 for a detailed explanation of the SOF value management requirements on the host 
            ///         controller. The value of FRINDEX must be 125 µsec (1 micro-frame) ahead of the SOF token value. The 
            ///         SOF value may be implemented as an 11-bit shadow register. For this discussion, this shadow register is 11 
            ///         bits and is named SOFV. SOFV updates every 8 micro-frames. (1 millisecond). An example implementation 
            ///         to achieve this behavior is to increment SOFV each time the FRINDEX[2:0] increments from a zero to a 
            ///         one.  
            ///         Software must use the value of FRINDEX to derive the current micro-frame number, both for high-speed 
            ///         isochronous scheduling purposes and to provide the get micro-frame number function required for client 
            ///         drivers. Therefore, the value of FRINDEX and the value of SOFV must be kept consistent if chip is reset or 
            ///         software writes to FRINDEX. Writes to FRINDEX must also write-through FRINDEX[13:3] to 
            ///         SOFV[10:0]. In order to keep the update as simple as possible, software should never write a FRINDEX 
            ///         value where the three least significant bits are 111b or 000b. Please refer to Section 4.5.
            /// </summary>
            public uint FRINDEX;

            /// <summary>
            /// CTRLDSSEGMENT  Control Data Structure Segment Register 
            /// 
            /// Address:  Operational Base + (10h) 
            /// Default Value:  00000000h 
            /// Attribute:  R/W (Writes must be DWord Writes) 
            /// Size: 32 bits 
            /// 
            /// This 32-bit register corresponds to the most significant address bits [63:32] for all EHCI data structures. If 
            /// the 64-bit Addressing Capability field in HCCPARAMS is a zero, then this register is not used. Software 
            /// cannot write to it and a read from this register will return zeros. 
            /// If the 64-bit Addressing Capability field in HCCPARAMS is a one, then this register is used with the link 
            /// pointers to construct 64-bit addresses to EHCI control data structures. This register is concatenated with the 
            /// link pointer from either the PERIODICLISTBASE, ASYNCLISTADDR, or any control data structure link 
            /// field to construct a 64-bit address. 
            /// This register allows the host software to locate all control data structures within the same 4 Gigabyte 
            /// memory segment. 
            /// </summary>
            public uint CTRLDSSEGMENT;

            /// <summary>
            /// PERIODICLISTBASE  Periodic Frame List Base Address Register 
            /// 
            /// Address:  Operational Base + (14h) 
            /// Default Value:  Undefined 
            /// Attribute:  R/W (Writes must be DWord Writes) 
            /// Size: 32 bits 
            /// 
            /// This 32-bit register contains the beginning address of the Periodic Frame List in the system memory. If the 
            /// host controller is in 64-bit mode (as indicated by a one in the 64-bit Addressing Capability field in the 
            /// HCCSPARAMS register), then the most significant 32 bits of every control data structure address comes 
            /// from the CTRLDSSEGMENT register (see Section 2.3.5). System software loads this register prior to 
            /// starting the schedule execution by the Host Controller (see 4.1). The memory structure referenced by this 
            /// physical memory pointer is assumed to be 4-Kbyte aligned. The contents of this register are combined with 
            /// the Frame Index Register (FRINDEX) to enable the Host Controller to step through the Periodic Frame List 
            /// in sequence.  
            /// 
            /// Bit     Description 
            /// 31:12   Base Address (Low). These bits correspond to memory address signals [31:12], 
            ///         respectively. 
            /// 11:0    Reserved. Must be written as 0s. During runtime, the values of these bits are undefined. 
            /// </summary>
            public uint PERIODICLISTBASE;

            /// <summary>
            /// ASYNCLISTADDR Current Asynchronous List Address Register 
            /// 
            /// Address:  Operational Base + (18h) 
            /// Default Value:  Undefined 
            /// Attribute:  Read/Write (Writes must be DWord Writes) 
            /// Size: 32 bits 
            ///  
            /// This 32-bit register contains the address of the next asynchronous queue head to be executed. If the host 
            /// controller is in 64-bit mode (as indicated by a one in 64-bit Addressing Capability field in the 
            /// HCCPARAMS register), then the most significant 32 bits of every control data structure address comes from 
            /// the CTRLDSSEGMENT register (See Section 2.3.5). Bits [4:0] of this register cannot be modified by system 
            /// software and will always return a zero when read. The memory structure referenced by this physical memory 
            /// pointer is assumed to be 32-byte (cache line) aligned.  
            /// 
            /// Bit     Description 
            /// 31:5    Link Pointer Low (LPL). These bits correspond to memory address signals [31:5], 
            ///         respectively. This field may only reference a Queue Head (QH), see Section 3.6. 
            /// 4:0     Reserved. These bits are reserved and their value has no effect on operation. 
            /// </summary>
            public uint ASYNCLISTADDR;

            /// <summary>
            /// N/A
            /// </summary>
            public uint Reserved1, Reserved2, Reserved3, Reserved4, Reserved5, Reserved6, Reserved7, Reserved8, Reserved9;

            /// <summary>
            /// CONFIGFLAG  Configure Flag Register 
            /// 
            /// Address:  Operational Base+ (40h) 
            /// Default Value:  00000000h 
            /// Attribute R/W 
            /// Size 32 bits 
            ///  
            /// This register is in the auxiliary power well.  It is only reset by hardware when the auxiliary power is initially 
            /// applied or in response to a host controller reset. 
            /// 
            /// Bit     Description 
            /// 31:1    Reserved. These bits are reserved and should be set to zero. 
            /// 0       Configure Flag (CF) R/W. Default 0b. Host software sets this bit as the last action in 
            ///         its process of configuring the Host Controller (see Section 4.1). This bit controls the 
            ///         default port-routing control logic. Bit values and side-effects are listed below. See 
            ///         Section 4.2 For operational details. 
            ///         0b      Port routing control logic default-routes each port to an implementation 
            ///                 dependent classic host controller.  
            ///         1b      Port routing control logic default-routes all ports to this host controller. 
            /// </summary>
            public uint CONFIGFLAG;

            /// <summary>
            /// PORTSC Port Status and Control Register 
            /// 
            /// Address:  Operational Base + (44h + (4*Port Number-1)) 
            /// where: Port Number is 1, 2, 3, … N_PORTS 
            /// Default:   00002000h (w/PPC set to one); 00003000h (w/PPC set to a zero) 
            /// Attribute:  RO, R/W, R/WC (field dependent) 
            /// Size 32 bits 
            ///  
            /// A host controller must implement one or more port registers. The number of port registers implemented by a 
            /// particular instantiation of a host controller is documented in the HCSPARAMs register (Section 2.2.3). 
            /// Software uses this information as an input parameter to determine how many ports need to be serviced. All 
            /// ports have the structure defined below. 
            /// This register is in the auxiliary power well. It is only reset by hardware when the auxiliary power is initially 
            /// applied or in response to a host controller reset. The initial conditions of a port are:   
            /// • No device connected,  
            /// • Port disabled 
            /// If the port has port power control, software cannot change the state of the port until after it applies power to 
            /// the port by setting port power to a 1.  Software must not attempt to change the state of the port until after 
            /// power is stable on the port. The host is required to have power stable to the port within 20 milliseconds of 
            /// the zero to one transition.  
            /// Note1: When a device is attached, the port state transitions to the connected state and system software will 
            /// process this as with any status change notification. Refer to Section 4.3 for operational requirements for how 
            /// change events interact with port suspend mode. 
            /// Note2: If a port is being used as the Debug Port, then the port may report device connected and enabled 
            /// when the Configured Flag is a zero. 
            /// 
            /// Bit     Description 
            /// 31:23   Reserved. These bits are reserved for future use and should return a value of zero when read. 
            /// 22      Wake on Over-current Enable (WKOC_E)  R/W. Default = 0b. Writing this bit to a 
            ///         one enables the port to be sensitive to over-current conditions as wake-up events. See 
            ///         Section 4.3 for effects of this bit on resume event behavior. Refer to Section 4.3.1 for 
            ///         operational model. 
            ///         This field is zero if Port Power is zero. 
            /// 21      Wake on Disconnect Enable (WKDSCNNT_E)  R/W. Default = 0b. Writing this bit to 
            ///         a one enables the port to be sensitive to device disconnects as wake-up events. See 
            ///         Section 4.3 for effects of this bit on resume event behavior. Refer to Section 4.3.1 for 
            ///         operational model. 
            ///         This field is zero if Port Power is zero. 
            /// 20      Wake on Connect Enable (WKCNNT_E)  R/W. Default = 0b. Writing this bit to a one 
            ///         enables the port to be sensitive to device connects as wake-up events. See Section 4.3 
            ///         for effects of this bit on resume event behavior. Refer to Section 4.3.1 for operational 
            ///         model. 
            ///         This field is zero if Port Power is zero. 
            /// 19:16   Port Test ControlR/W. Default = 0000b. When this field is zero, the port is NOT 
            ///         operating in a test mode. A non-zero value indicates that it is operating in test mode and 
            ///         the specific test mode is indicated by the specific value. The encoding of the test mode 
            ///         bits are (0110b - 1111b are reserved): 
            ///         Bits    Test Mode 
            ///         0000b   Test mode not enabled 
            ///         0001b   Test J_STATE 
            ///         0010b   Test K_STATE 
            ///         0011b   Test SE0_NAK 
            ///         0100b   Test Packet 
            ///         0101b   Test FORCE_ENABLE 
            ///         Refer to Section 4.14 for the operational model for using these test modes and the USB 
            ///         Specification Revision 2.0, Chapter 7 for details on each test mode. 
            /// 15:14   Port Indicator Control. Default = 00b. Writing to these bits has no effect if the 
            ///         P_INDICATOR bit in the HCSPARAMS register is a zero. If P_INDICATOR bit is a one, 
            ///         then the bit encodings are: 
            ///         Bit Value   Meaning 
            ///         00b         Port indicators are off 
            ///         01b         Amber 
            ///         10b         Green 
            ///         11b         Undefined 
            ///         Refer to the USB Specification Revision 2.0 for a description on how these bits are to be used. 
            ///         This field is zero if Port Power is zero. 
            /// 13      Port OwnerR/W Default = 1b. This bit unconditionally goes to a 0b when the 
            ///         Configured bit in the CONFIGFLAG register makes a 0b to 1b transition. This bit 
            ///         unconditionally goes to 1b whenever the Configured bit is zero.  
            ///         System software uses this field to release ownership of the port to a selected host 
            ///         controller (in the event that the attached device is not a high-speed device). Software 
            ///         writes a one to this bit when the attached device is not a high-speed device. A one in 
            ///         this bit means that a companion host controller owns and controls the port. See Section 
            ///         4.2 for operational details. 
            /// 12      Port Power (PP)R/W or RO. The function of this bit depends on the value of the Port 
            ///         Power Control (PPC) field in the HCSPARAMS register. The behavior is as follows: 
            ///         PPC     PP      Operation 
            ///         0b      1b      ROHost controller does not have port power control switches. 
            ///                         Each port is hard-wired to power.  
            ///         1b      1b/0b   R/WHost controller has port power control switches. This bit 
            ///                         represents the current setting of the switch (0 = off, 1 = on). When 
            ///                         power is not available on a port (i.e. PP equals a 0), the port is non-functional and will not report attaches, detaches, etc. 
            ///                         When an over-current condition is detected on a powered port and PPC is a one, the PP 
            ///                         bit in each affected port may be transitioned by the host controller from a 1 to 0 
            ///                         (removing power from the port). 
            /// 11:10   Line StatusRO. These bits reflect the current logical levels of the D+ (bit 11) and D- 
            ///         (bit 10) signal lines. These bits are used for detection of low-speed USB devices prior to 
            ///         the port reset and enable sequence. This field is valid only when the port enable bit is 
            ///         zero and the current connect status bit is set to a one. 
            ///         The encoding of the bits are: 
            ///         Bits[11:10]     USB State   Interpretation 
            ///         00b             SE0  Not    Low-speed device, perform EHCI reset 
            ///         10b             J-state     Not Low-speed device, perform EHCI reset 
            ///         01b             K-state     Low-speed device, release ownership of port 
            ///         11b             Undefined   Not Low-speed device, perform EHCI reset. 
            ///         This value of this field is undefined if Port Power is zero.  
            /// 9       Reserved. This bit is reserved for future use, and should return a value of zero when read. 
            /// 8       Port ResetR/W. 1=Port is in Reset. 0=Port is not in Reset. Default = 0. When 
            ///         software writes a one to this bit (from a zero), the bus reset sequence as defined in the 
            ///         USB Specification Revision 2.0 is started. Software writes a zero to this bit to terminate 
            ///         the bus reset sequence. Software must keep this bit at a one long enough to ensure the 
            ///         reset sequence, as specified in the USB Specification Revision 2.0, completes. Note: 
            ///         when software writes this bit to a one, it must also write a zero to the Port Enable bit. 
            ///         Note that when software writes a zero to this bit there may be a delay before the bit 
            ///         status changes to a zero. The bit status will not read as a zero until after the reset has 
            ///         completed. If the port is in high-speed mode after reset is complete, the host controller 
            ///         will automatically enable this port (e.g. set the Port Enable bit to a one). A host controller 
            ///         must terminate the reset and stabilize the state of the port within 2 milliseconds of 
            ///         software transitioning this bit from a one to a zero. For example: if the port detects that 
            ///         the attached device is high-speed during reset, then the host controller must have the 
            ///         port in the enabled state within 2ms of software writing this bit to a zero.  
            ///         The HCHalted bit in the USBSTS register should be a zero before software attempts to 
            ///         use this bit. The host controller may hold Port Reset asserted to a one when the 
            ///         HCHalted bit is a one. 
            ///         This field is zero if Port Power is zero. 
            /// 7       SuspendR/W. 1=Port in suspend state. 0=Port not in suspend state. Default = 0. Port 
            ///         Enabled Bit and Suspend bit of this register define the port states as follows: 
            ///         Bits [Port Enabled, Suspend]    Port State 
            ///         0X                              Disable 
            ///         10                              Enable 
            ///         11                              Suspend 
            ///         When in suspend state, downstream propagation of data is blocked on this port, except 
            ///         for port reset. The blocking occurs at the end of the current transaction, if a transaction 
            ///         was in progress when this bit was written to 1. In the suspend state, the port is sensitive 
            ///         to resume detection.  Note that the bit status does not change until the port is 
            ///         suspended and that there may be a delay in suspending a port if there is a transaction 
            ///         currently in progress on the USB. 
            ///         A write of zero to this bit is ignored by the host controller. The host controller will 
            ///         unconditionally set this bit to a zero when: 
            ///         • Software sets the Force Port Resume bit to a zero (from a one).  
            ///         • Software sets the Port Reset bit to a one (from a zero). 
            ///         If host software sets this bit to a one when the port is not enabled (i.e. Port enabled bit is 
            ///         a zero) the results are undefined.  
            ///         This field is zero if Port Power is zero. 
            /// 6       Force Port Resume  R/W. 1= Resume detected/driven on port. 0=No resume (K-state) detected/driven on port. 
            ///         Default = 0. This functionality defined for manipulating 
            ///         this bit depends on the value of the Suspend bit. For example, if the port is not 
            ///         suspended (Suspend and Enabled bits are a one) and software transitions this bit to a 
            ///         one, then the effects on the bus are undefined. 
            ///         Software sets this bit to a 1 to drive resume signaling. The Host Controller sets this bit to 
            ///         a 1 if a J-to-K transition is detected while the port is in the Suspend state. When this bit 
            ///         transitions to a one because a J-to-K transition is detected, the Port Change Detect bit in 
            ///         the USBSTS register is also set to a one. If software sets this bit to a one, the host 
            ///         controller must not set the Port Change Detect bit. 
            ///         Note that when the EHCI controller owns the port, the resume sequence follows the 
            ///         defined sequence documented in the USB Specification Revision 2.0. The resume 
            ///         signaling (Full-speed 'K') is driven on the port as long as this bit remains a one. Software 
            ///         must appropriately time the Resume and set this bit to a zero when the appropriate 
            ///         amount of time has elapsed. Writing a zero (from one) causes the port to return to high-speed mode 
            ///         (forcing the bus below the port into a high-speed idle). This bit will remain a 
            ///         one until the port has switched to the high-speed idle. The host controller must complete 
            ///         this transition within 2 milliseconds of software setting this bit to a zero. 
            ///         This field is zero if Port Power is zero. 
            /// 5       Over-current ChangeR/WC. Default = 0. 1=This bit gets set to a one when there is a 
            ///         change to Over-current Active. Software clears this bit by writing a one to this bit position.  
            /// 4       Over-current ActiveRO. Default = 0. 1=This port currently has an over-current 
            ///         condition. 0=This port does not have an over-current condition. This bit will automatically 
            ///         transition from a one to a zero when the over current condition is removed. 
            /// 3       Port Enable/Disable ChangeR/WC. 1=Port enabled/disabled status has changed. 
            ///         0=No change. Default = 0. For the root hub, this bit gets set to a one only when a port is 
            ///         disabled due to the appropriate conditions existing at the EOF2 point (See Chapter 11 of 
            ///         the USB Specification for the definition of a Port Error). Software clears this bit by writing 
            ///         a 1 to it. 
            ///         This field is zero if Port Power is zero. 
            /// 2       Port Enabled/DisabledR/W.  1=Enable. 0=Disable. Default = 0. Ports can only be 
            ///         enabled by the host controller as a part of the reset and enable. Software cannot enable 
            ///         a port by writing a one to this field. The host controller will only set this bit to a one when 
            ///         the reset sequence determines that the attached device is a high-speed device. 
            ///         Ports can be disabled by either a fault condition (disconnect event or other fault 
            ///         condition) or by host software. Note that the bit status does not change until the port 
            ///         state actually changes. There may be a delay in disabling or enabling a port due to other 
            ///         host controller and bus events. See Section 4.2 for full details on port reset and enable.  
            ///         When the port is disabled (0b) downstream propagation of data is blocked on this port, 
            ///         except for reset. 
            ///         This field is zero if Port Power is zero. 
            /// 1       Connect Status ChangeR/WC.  1=Change in Current Connect Status. 0=No change. 
            ///         Default = 0. Indicates a change has occurred in the port’s Current Connect Status. The 
            ///         host controller sets this bit for all changes to the port device connect status, even if 
            ///         system software has not cleared an existing connect status change. For example, the 
            ///         insertion status changes twice before system software has cleared the changed 
            ///         condition, hub hardware will be “setting” an already-set bit (i.e., the bit will remain set). 
            ///         Software sets this bit to 0 by writing a 1 to it. 
            ///         This field is zero if Port Power is zero. 
            /// 0       Current Connect StatusRO.  1=Device is present on port. 0=No device is present. 
            ///         Default = 0. This value reflects the current state of the port, and may not correspond 
            ///         directly to the event that caused the Connect Status Change bit (Bit 1) to be set. 
            ///         This field is zero if Port Power is zero.
            /// </summary>
            public uint PORTSC;
        }

        ////private uint EECP;  //(USBBASE) + 0x08 )) & 0x0FF00) >> 16;

        /// <summary>
        /// USBLEGSUP USB Legacy Support Extended Capability 
        /// 
        /// Offset:  EECP + 00h 
        /// Default Value  Implementation Dependent 
        /// Attribute RO, R/W 
        /// Size: 32 bits 
        /// 
        /// This register is an EHCI extended capability register. It includes a specific function section and a pointer to 
        /// the next EHCI extended capability. This register is used by pre-OS software (BIOS) and the operating 
        /// system to coordinate ownership of the EHCI host controller. See Section 5.1 for details.  
        /// Table 2–3. USBLEGSUP  USB Legacy Support Extended Capability 
        /// 
        /// Bit     Description 
        /// 31:25   Reserved. These bits are reserved and must be set to zero. 
        /// 24      HC OS Owned Semaphore  R/W. 0=Default. System software sets this bit to request 
        ///         ownership of the EHCI controller. Ownership is obtained when this bit reads as one and 
        ///         the HC BIOS Owned Semaphore bit reads as zero. 
        /// 23:17   Reserved. These bits are reserved and must be set to zero. 
        /// 16      HC BIOS Owned Semaphore  R/W. 0=Default. The BIOS sets this bit to establish 
        ///         ownership of the EHCI controller. System BIOS will set this bit to a zero in response to a 
        ///         request for ownership of the EHCI controller by system software. 
        /// 15:8    Next EHCI Extended Capability Pointer  RO. This field points to the PCI 
        ///         configuration space offset of the next extended capability pointer. A value of 00h 
        ///         indicates the end of the extended capability list. 
        /// 7:0     Capability ID  RO. This field identifies the extended capability. A value of 01h 
        ///         identifies the capability as Legacy Support. This extended capability requires one 
        ///         additional 32-bit register for control/status information, and this register is located at 
        ///         offset EECP+04h.
        /// </summary>
        ////private uint USBLEGSUP;

        /// <summary>
        /// USBLEGCTLSTS USB Legacy Support Control/Status 
        /// 
        /// Offset:  EECP + 04h 
        /// Default Value  00000000h 
        /// Attribute  RO, R/W, R/WC 
        /// Size: 32 bits 
        /// 
        /// Pre-OS (BIOS) software uses this register to enable SMIs for every EHCI/USB event it needs to track. Bits 
        /// [21:16] of this register are simply shadow bit of USBSTS register [5:0].  
        /// Table 2–4. USBLEGCTLSTS  USB Legacy Support Control/Status 
        /// 
        /// Bit     Description 
        /// 31      SMI on BAR  R/WC.  0=Default. This bit is set to one whenever the Base Address 
        ///         Register (BAR) is written. 
        /// 30      SMI on PCI Command  R/WC. 0=Default. This bit is set to one whenever the PCI 
        ///         Command Register is written. 
        /// 29      SMI on OS Ownership Change  R/WC. 0=Default. This bit is set to one whenever 
        ///         the HC OS Owned Semaphore bit in the USBLEGSUP register transitions from 1 to a 0 
        ///         or 0 to a 1 
        /// 28:22   Reserved.  These bits are reserved and should be zero. 
        /// 21      SMI on Async Advance  RO.  0=Default. Shadow bit of the Interrupt on Async 
        ///         Advance bit in the USBSTS register see Section 2.3.2 for definition.  
        ///         To set this bit to a zero, system software must write a one to the Interrupt on Async 
        ///         Advance bit in the USBSTS register. 
        /// 20      SMI on Host System Error  RO.  0=Default. Shadow bit of Host System Error bit in 
        ///         the USBSTS register, see Section 2.3.2 for definition and effects of the events 
        ///         associated with this bit being set to a one.  
        ///         To set this bit to a zero, system software must write a one to the Host System Error bit in 
        ///         the USBSTS register. 
        /// 19      SMI on Frame List Rollover  RO. 0=Default. Shadow bit of Frame List Rollover bit in 
        ///         the USBSTS register see Section 2.3.2 for definition.  
        ///         To set this bit to a zero, system software must write a one to the Frame List Rollover bit 
        ///         in the USBSTS register.  
        /// 18      SMI on Port Change Detect  RO.  0=Default. Shadow bit of Port Change Detect bit in 
        ///         the USBSTS register see Section 2.3.2 for definition.  
        ///         To set this bit to a zero, system software must write a one to the Port Change Detect bit 
        ///         in the USBSTS register. 
        /// 17      SMI on USB Error  RO.  0=Default. Shadow bit of USB Error Interrupt (USBERRINT) 
        ///         bit in the USBSTS register see Section 2.3.2 for definition.  
        ///         To set this bit to a zero, system software must write a one to the USB Error Interrupt bit 
        ///         in the USBSTS register.  
        /// 16      SMI on USB Complete  RO.  0=Default. Shadow bit of USB Interrupt (USBINT) bit in 
        ///         the USBSTS register see Section 2.3.2 for definition.  
        ///         To set this bit to a zero, system software must write a one to the USB Interrupt bit in the 
        ///         USBSTS register.  
        /// 15      SMI on BAR Enable  R/W. 0=Default. When this bit is one and SMI on BAR is one, 
        ///         then the host controller will issue an SMI. 
        /// 14      SMI on PCI Command Enable  R/W. 0=Default. When this bit is one and SMI on PCI 
        ///         Command is one, then the host controller will issue an SMI. 
        /// 13      SMI on OS Ownership Enable   R/W. 0=Default. When this bit is a one AND the OS 
        ///         Ownership Change bit is one, the host controller will issue an SMI. 
        /// 12:6    Reserved.  These bits are reserved and should be zero. 
        /// 5       SMI on Async Advance Enable  R/W. 0=Default. When this bit is a one, and the SMI 
        ///         on Async Advance bit (above) in this register is a one, the host controller will issue an 
        ///         SMI immediately. E 
        /// 4       SMI on Host System Error Enable  R/W. 0=Default. When this bit is a one, and the 
        ///         SMI on Host System Error bit (above) in this register is a one, the host controller will 
        ///         issue an SMI immediately. E 
        /// 3       SMI on Frame List Rollover Enable  R/W. 0=Default. When this bit is a one, and the 
        ///         SMI on Frame List Rollover bit (above) in this register is a one, the host controller will 
        ///         issue an SMI immediately. E 
        /// 2       SMI on Port Change Enable  R/W. 0=Default. When this bit is a one, and the SMI on 
        ///         Port Change Detect bit (above) in this register is a one, the host controller will issue an 
        ///         SMI immediately. E 
        /// 1       SMI on USB Error Enable  R/W. 0=Default. When this bit is a one, and the SMI on 
        ///         USB Error bit (above) in this register is a one, the host controller will issue an SMI 
        ///         immediately. E 
        /// 0       USB SMI Enable  R/W. 0=Default. When this bit is a one, and the SMI on USB 
        ///         Complete bit (above) in this register is a one, the host controller will issue an SMI 
        ///         immediately. E 
        ///         
        /// Notes:  
        /// A.
        ///   For all enable register bits, 1= Enabled, 0= Disabled 
        /// B.
        ///   SMI – System Management Interrupt 
        /// C.
        ///   BAR – Base Address Register 
        /// D.
        ///   MSE – Memory Space Enable 
        /// E.
        ///   SMI’s are independent of the interrupt threshold value
        ///</summary>
        ////private uint USBLEGCTLSTS;   //EECP+4h USB Legacy Support Control and Status Register 

        ////private Cosmos.Core.IOPort Port;

        ////private PCIDeviceNormal device;
        ////protected IOGroup io;

        ////private IOPortWrite ConfigAdress = new IOPortWrite(0xCF8);
        ////private IOPortRead ConfigData = new IOPortRead(0xCFF);

        ////protected class IOGroup : Cosmos.Core.IOGroup.IOGroup
        ////{
        ////    public readonly IOPort CommandAdress;
        ////    public readonly IOPort RegisterData;
        ////    public readonly IOPort BusData;
        ////    public readonly IOPortRead ClassCode;

        ////    public IOGroup(PCIDeviceNormal device)
        ////    {
        ////        var baseAdress = (ushort)device.BaseAddresses[0].BaseAddress();

        ////        CommandAdress = new IOPort(baseAdress, 0x04);
        ////        ////RegisterData = new IOPort(baseAdress, 0x10);
        ////        BusData = new IOPort(baseAdress, 0x1C);
        ////    }
        ////}

        public EHCI()
            : base()
        {
            ////device = (PCIDeviceNormal)Cosmos.Core.PCI.GetDevice(VendorId, DeviceId);

            ////if (device.ClassCode == (byte)0x0c)
            ////{
            ////    Console.Write("Serial bus controller ");
            ////    if (device.Subclass == (byte)0x03)
            ////    {
            ////        Console.Write("USB controller ");
            ////        switch ((byte)device.ProgIF)
            ////        {
            ////            case 0x00:
            ////                Console.WriteLine("UHCI");
            ////                break;
            ////            case 0x10:
            ////                Console.WriteLine("OHCI");
            ////                break;
            ////            case 0x20:
            ////                Console.WriteLine("EHCI");
            ////                break;
            ////            case 0x30:
            ////                Console.WriteLine("XHCI");
            ////                break;
            ////        }
            ////    }
            ////}

            ////////adress1 = (device.bus << 16) | (device.slot << 11) | (device.function << 8) | (0x80000000);
            ////////device.
            ////io = new IOGroup(device);
            ////device.EnableDevice();
            ////device.EnableMemory(true);
            //Console.WriteLine(pciConfigReadWord(4).ToHex());
            ReadCaps();
        }

        unsafe void ReadCaps()
        {
            var device = (PCIDevice) parent;
            Console.WriteLine(device.CapabilitiesPointer.ToHex());
            var caps = (CapabilityRegister*)device.BaseAddresses[0].BaseAddress();
            var mem = new MemoryBlock(device.BaseAddresses[0].BaseAddress(), 256).DWords;

            for (uint i = 0; i < 256; i += 4)
            {
                Console.Write(" ");

                if (i % 32 == 0)
                    Console.WriteLine();

                Console.Write(mem[i].ToHex());
            }

            Console.Write("Version: ");
            Console.WriteLine(caps->HCIVERSION.ToHex());
            Console.WriteLine(caps->CAPLENGTH.ToHex());
            Console.WriteLine(caps->HCSPARAMS.ToHex());


            CapabilityRegister* caps = (CapabilityRegister*)0;
        }

        ////private readonly uint adress1;

        ////ushort pciConfigReadWord(ushort offset)
        ////{
        ////    ConfigAdress.DWord = (uint)(adress1 | (offset & 0xfcU));
        ////    return (ushort)((ConfigData.DWord >> ((offset & 2) * 8)) & 0xffff);
        ////}

        public override bool Setup(IHardwareResources hardwareResources)
        {
            this.hardwareResources = hardwareResources;
            base.name = "VMWare.EHCI._0x" + hardwareResources.GetIOPortRegion(0).BaseIOPort.ToString("X");
            return true;
        }

        public override DeviceDriverStartStatus Start()
        {
            return DeviceDriverStartStatus.Started;
        }

        public override bool OnInterrupt()
        {
            return true;
        }
    }
}
