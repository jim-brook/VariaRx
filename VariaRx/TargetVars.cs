using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace VariaRx
{
    // Ripped from ANT+ProfileLib
    public class TargetVars
    {
        // Summary:
        //     Threat Level
        public enum ThreatLevelBitField : byte
        {
            //
            // Summary:
            //     No threat
            NoThreat = 0,
            //
            // Summary:
            //     Vehicle approaching
            VehicleApproaching = 1,
            //
            // Summary:
            //     Vehicle Fast approach
            FastApproach = 2
        }

        //
        // Summary:
        //     Threat Side
        public enum ThreatSideBitField : byte
        {
            //
            // Summary:
            //     No side
            NoSide = 0,
            //
            // Summary:
            //     Right
            Right = 1,
            //
            // Summary:
            //     Left
            Left = 2
        }
        //
        // Summary:
        //     Speed Target 1
        [DisplayName("Speed Target 1 (3.04 m/s)")]
        public byte SpeedTarget1 { get; set; }

        //
        // Summary:
        //     Speed Target 2
        [DisplayName("Speed Target 2 (3.04 m/s)")]
        public byte SpeedTarget2 { get; set; }

        //
        // Summary:
        //     Speed Target 3
        [DisplayName("Speed Target 3 (3.04 m/s)")]
        public byte SpeedTarget3 { get; set; }

        //
        // Summary:
        //     Speed Target 4
        [DisplayName("Speed Target 4 (3.04 m/s)")]
        public byte SpeedTarget4 { get; set; }

        //
        // Summary:
        //     Speed Target 5
        [DisplayName("Speed Target 5 (3.04 m/s)")]
        public byte SpeedTarget5 { get; set; }

        //
        // Summary:
        //     Speed Target 6
        [DisplayName("Speed Target 6 (3.04 m/s)")]
        public byte SpeedTarget6 { get; set; }

        //
        // Summary:
        //     Speed Target 7
        [DisplayName("Speed Target 7 (3.04 m/s)")]
        public byte SpeedTarget7 { get; set; }

        //
        // Summary:
        //     Speed Target 8
        [DisplayName("Speed Target 8 (3.04 m/s)")]
        public byte SpeedTarget8 { get; set; }


        //
        // Summary:
        //     Range Target 1
        [DisplayName("Range Target 1 (3.125m)")]
        public byte RangeTarget1 { get; set; }

        //
        // Summary:
        //     Range Target 2
        [DisplayName("Range Target 2 (3.125m)")]
        public byte RangeTarget2 { get; set; }

        //
        // Summary:
        //     Range Target 3
        [DisplayName("Range Target 3 (3.125m)")]
        public byte RangeTarget3 { get; set; }

        //
        // Summary:
        //     Range Target 4
        [DisplayName("Range Target 4 (3.125m)")]
        public byte RangeTarget4 { get; set; }
        //
        // Summary:
        //     Range Target 5
        [DisplayName("Range Target 5 (3.125m)")]
        public byte RangeTarget5 { get; set; }

        //
        // Summary:
        //     Range Target 6
        [DisplayName("Range Target 6 (3.125m)")]
        public byte RangeTarget6 { get; set; }

        //
        // Summary:
        //     Range Target 7
        [DisplayName("Range Target 7 (3.125m)")]
        public byte RangeTarget7 { get; set; }

        //
        // Summary:
        //     Range Target 8
        [DisplayName("Range Target 8 (3.125m)")]
        public byte RangeTarget8 { get; set; }





        public ThreatLevelBitField ThreatLevelTarget1 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget2 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget3 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget4 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget5 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget6 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget7 { get; set; }
        public ThreatLevelBitField ThreatLevelTarget8 { get; set; }

        public ThreatSideBitField ThreatSideTarget1 { get; set; }
        public ThreatSideBitField ThreatSideTarget2 { get; set; }
        public ThreatSideBitField ThreatSideTarget3 { get; set; }
        public ThreatSideBitField ThreatSideTarget4 { get; set; }
        public ThreatSideBitField ThreatSideTarget5 { get; set; }
        public ThreatSideBitField ThreatSideTarget6 { get; set; }
        public ThreatSideBitField ThreatSideTarget7 { get; set; }
        public ThreatSideBitField ThreatSideTarget8 { get; set; }
    }
}
