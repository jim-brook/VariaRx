using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ANT_Managed_Library;
using AntPlus.Profiles.BikeRadar;
using AntPlus.Types;
namespace VariaRx
{
    internal partial class VariaRx : Form
    {        
        internal static readonly byte[] USER_NETWORK_KEY = {  };
        internal static readonly byte USER_NETWORK_NUM = 0;
        internal ANT_Device device0;
        internal ANT_Channel channel0;
        internal BikeRadarDisplay radarEquipmentDisplay;
        internal Network networkAntPlus = new Network(USER_NETWORK_NUM, USER_NETWORK_KEY, 57);
        internal ushort usValue = 0;
        internal struct APP_EXT
        {
            internal volatile bool AppIsClosing;
            internal volatile List<TargetVars> TargetQueue;
            internal volatile EventWaitHandle TargetQueueReadyWait;
            internal volatile Thread ProcessTargetQueueThread;
            internal volatile object TargetQueueLock;
            internal volatile TargetDisplay TargDispForm;
            internal volatile VariaRx MainForm;

        }
        internal APP_EXT VariaAppExt;
        internal VariaRx()
        {
            InitializeComponent();
            VariaAppExt = new APP_EXT();
            VariaAppExt.MainForm = this;
            VariaAppExt.TargDispForm = new TargetDisplay(this);
            VariaAppExt.AppIsClosing = false;
            VariaAppExt.TargetQueueLock = new object();
            VariaAppExt.TargetQueue = new List<TargetVars>();
            VariaAppExt.TargetQueueReadyWait = new EventWaitHandle(false, EventResetMode.AutoReset);
            VariaAppExt.ProcessTargetQueueThread = new Thread(VariaAppExt.TargDispForm.BufferTargs);
            VariaAppExt.ProcessTargetQueueThread.Start();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            bool res = Init();
            if (res)
            {
                ConfigureANT();
                VariaAppExt.TargDispForm.Show();
            }
        }
        private void stopBtn_Click(object sender, EventArgs e)
        {
            CloseAllChannles();
            VariaAppExt.TargDispForm.Hide();
        }
        private void CloseAllChannles()
        {
            if (channel0 != null)
            {
                Console.WriteLine("Closing Channel");                
                channel0.closeChannel();
                channel0.Dispose();
                channel0 = null;
            }
            if (device0 != null)
            {
                device0.Dispose();
                device0 = null;
            }
            //dispose time and wait object and semaphore
        }

        private void VariaRx_FormClosing(object sender, FormClosingEventArgs e)
        {
            VariaAppExt.AppIsClosing = true;
            VariaAppExt.TargDispForm.UpdateEntriesTimer.Stop();
            VariaAppExt.TargetQueueReadyWait.Set();
            VariaAppExt.ProcessTargetQueueThread.Join(250);
            CloseAllChannles();
            VariaAppExt.TargDispForm.targetPointSem.Dispose();
            VariaAppExt.TargDispForm.Close();
        }
        internal bool Init()
        {
            bool result = true;
            try
            {
                Console.WriteLine("Attempting to connect to an ANT USB device 0...");
                device0 = new ANT_Device();
                device0.deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);
                channel0 = device0.getChannel(0);               
                channel0.channelResponse += new dChannelResponseHandler(ChannelResponse0);
                Console.WriteLine("Initialization 0 was successful!");
 
            }
            catch (Exception ex)
            {
                result = false;
                if (device0 == null)
                {
                    Console.WriteLine("Could not connect to device 0.\n" + "Details: \n   " + ex.Message);
                }
                else
                {
                    Console.WriteLine("Error connecting to ANT: " + ex.Message);
                }
            }
            return result;
        }
        internal void ConfigureANT()
        {
            Console.WriteLine("Resetting module 0 ...");
            device0.ResetSystem();
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("Setting network key..."); 
            if (device0.setNetworkKey(USER_NETWORK_NUM, USER_NETWORK_KEY, 500))
                Console.WriteLine("Network key set");
            else
                throw new Exception("Error configuring network key");

            Console.WriteLine("Setting Channel ID...");
            channel0.setChannelSearchTimeout((byte)100, 100);
            if (channel0.setChannelID(0, false, 40, 0, 8192))
            {
                
                Console.WriteLine("Channel ID set");
            }
            else
                Console.WriteLine("Error configuring Channel ID");

            radarEquipmentDisplay = new BikeRadarDisplay(channel0, networkAntPlus);
     
            radarEquipmentDisplay.DataPageReceived += radarEquipment_DataPageReceived;
            radarEquipmentDisplay.RadarSensorFound += radarEquipment_Found;
            radarEquipmentDisplay.RadarTargetsAPageReceived += radarEquipment_RadarTargetsAPageReceived;
            radarEquipmentDisplay.RadarTargetsBPageReceived += radarEquipment_RadarTargetsBPageReceived;
            radarEquipmentDisplay.TurnOn();

        }
        private void radarEquipment_RadarTargetsAPageReceived(RadarTargetsA targs)
        {
            //Console.WriteLine(targs.RangeTarget1.ToString());
            var targVars = new TargetVars();
            targVars.SpeedTarget1 = targs.SpeedTarget1;
            targVars.SpeedTarget2 = targs.SpeedTarget2;
            targVars.SpeedTarget3 = targs.SpeedTarget3;
            targVars.SpeedTarget4 = targs.SpeedTarget4;
            targVars.RangeTarget1 = targs.RangeTarget1;
            targVars.RangeTarget2 = targs.RangeTarget2;
            targVars.RangeTarget3 = targs.RangeTarget3;
            targVars.RangeTarget4 = targs.RangeTarget4;
            targVars.ThreatLevelTarget1 = (TargetVars.ThreatLevelBitField) targs.ThreatLevelTarget1;
            targVars.ThreatLevelTarget2 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget2;
            targVars.ThreatLevelTarget3 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget3;
            targVars.ThreatLevelTarget4 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget4;
            targVars.ThreatSideTarget1 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget1;
            targVars.ThreatSideTarget2 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget2;
            targVars.ThreatSideTarget3 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget3;
            targVars.ThreatSideTarget4 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget4;
            //Console.WriteLine(targVars.SpeedTarget1.ToString() + ":" + targVars.RangeTarget1.ToString());
            lock (VariaAppExt.TargetQueueLock)
            {
                VariaAppExt.TargetQueue.Add(targVars);
            }
            VariaAppExt.TargetQueueReadyWait.Set();
        }
        private void radarEquipment_RadarTargetsBPageReceived(RadarTargetsB targs)
        {
            //Console.WriteLine(targs.RangeTarget5.ToString());
            var targVars = new TargetVars();
            targVars.SpeedTarget5 = targs.SpeedTarget5;
            targVars.SpeedTarget6 = targs.SpeedTarget6;
            targVars.SpeedTarget7 = targs.SpeedTarget7;
            targVars.SpeedTarget8 = targs.SpeedTarget8;
            targVars.RangeTarget5 = targs.RangeTarget5;
            targVars.RangeTarget6 = targs.RangeTarget6;
            targVars.RangeTarget7 = targs.RangeTarget7;
            targVars.RangeTarget8 = targs.RangeTarget8;
            targVars.ThreatLevelTarget5 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget5;
            targVars.ThreatLevelTarget6 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget6;
            targVars.ThreatLevelTarget7 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget7;
            targVars.ThreatLevelTarget8 = (TargetVars.ThreatLevelBitField)targs.ThreatLevelTarget8;
            targVars.ThreatSideTarget5 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget5;
            targVars.ThreatSideTarget6 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget6;
            targVars.ThreatSideTarget7 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget7;
            targVars.ThreatSideTarget8 = (TargetVars.ThreatSideBitField)targs.ThreatSideTarget8;
            lock (VariaAppExt.TargetQueueLock)
            {
                VariaAppExt.TargetQueue.Add(targVars);
            }
            VariaAppExt.TargetQueueReadyWait.Set();
        }

        /*                                                                                                                   */
        /* ANT boilerplate code transport level stuff from here down                                                                            */
        /*                                                                                                                   */



        private static void radarEquipment_DataPageReceived(AntPlus.Profiles.Components.DataPage action)
        {
            //Console.WriteLine(action.DataPageNumber.ToString());
        }
        private static void radarEquipment_Found(ushort x, byte y)
        {
            //Console.WriteLine(x.ToString() + ":" + y.ToString());
        }

        internal void ChannelResponse0(ANT_Response response)
        {
            Random rnd = new Random();

            try
            {
                switch ((ANT_ReferenceLibrary.ANTMessageID)response.responseID)
                {
                    case ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40:
                        {
                            switch (response.getChannelEventCode())
                            {
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TX_0x03:
                                    {
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_SEARCH_TIMEOUT_0x01:
                                    {
                                        Console.WriteLine("Search Timeout");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_0x02:
                                    {
                                        Console.WriteLine("Rx Fail");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_RX_FAILED_0x04:
                                    {
                                        Console.WriteLine("Burst receive has failed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_COMPLETED_0x05:
                                    {
                                        Console.WriteLine("Transfer Completed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_FAILED_0x06:
                                    {
                                        Console.WriteLine("Transfer Failed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_CLOSED_0x07:
                                    {
                                        Console.WriteLine("Channel Closed");
                                        Console.WriteLine("Unassigning Channel...");
                                        if (channel0.unassignChannel(500))
                                        {
                                            Console.WriteLine("Unassigned Channel");
                                            Console.WriteLine("Press enter to exit");   
                                        }
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_GO_TO_SEARCH_0x08:
                                    {
                                        Console.WriteLine("Go to Search");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_COLLISION_0x09:
                                    {
                                        Console.WriteLine("Channel Collision");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_START_0x0A:
                                    {
                                        Console.WriteLine("Burst Started");
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("Unhandled Channel Event " + response.getChannelEventCode());
                                        break;
                                    }
                            }
                            break;
                        }
                    case ANT_ReferenceLibrary.ANTMessageID.BROADCAST_DATA_0x4E:
                    case ANT_ReferenceLibrary.ANTMessageID.ACKNOWLEDGED_DATA_0x4F:
                    case ANT_ReferenceLibrary.ANTMessageID.BURST_DATA_0x50:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_BROADCAST_DATA_0x5D:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_ACKNOWLEDGED_DATA_0x5E:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_BURST_DATA_0x5F:
                        {
                            // Process received messages here
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("Unknown Message " + response.responseID);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Channel response processing failed with exception: " + ex.Message);
            }
        }
        internal void DeviceResponse(ANT_Response response)
        {
            switch ((ANT_ReferenceLibrary.ANTMessageID)response.responseID)
            {
                case ANT_ReferenceLibrary.ANTMessageID.STARTUP_MESG_0x6F:
                    {
                        Console.Write("RESET Complete, reason: ");

                        byte ucReason = response.messageContents[0];

                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_POR_0x00)
                            Console.WriteLine("RESET_POR");
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_RST_0x01)
                            Console.WriteLine("RESET_RST");
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_WDT_0x02)
                            Console.WriteLine("RESET_WDT");
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_CMD_0x20)
                            Console.WriteLine("RESET_CMD");
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_SYNC_0x40)
                            Console.WriteLine("RESET_SYNC");
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_SUSPEND_0x80)
                            Console.WriteLine("RESET_SUSPEND");
                        break;
                    }
                case ANT_ReferenceLibrary.ANTMessageID.VERSION_0x3E:
                    {
                        Console.WriteLine("VERSION: " + new ASCIIEncoding().GetString(response.messageContents));
                        break;
                    }
                case ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40:
                    {
                        switch (response.getMessageID())
                        {
                            case ANT_ReferenceLibrary.ANTMessageID.CLOSE_CHANNEL_0x4C:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.CHANNEL_IN_WRONG_STATE_0x15)
                                    {
                                        Console.WriteLine("Channel is already closed");   
                                    }
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.NETWORK_KEY_0x46:
                            case ANT_ReferenceLibrary.ANTMessageID.ASSIGN_CHANNEL_0x42:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_ID_0x51:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_RADIO_FREQ_0x45:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_MESG_PERIOD_0x43:
                            case ANT_ReferenceLibrary.ANTMessageID.OPEN_CHANNEL_0x4B:
                            case ANT_ReferenceLibrary.ANTMessageID.UNASSIGN_CHANNEL_0x41:
                                {
                                    if (response.getChannelEventCode() != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                                    {
                                        Console.WriteLine(String.Format("Error {0} configuring {1}", response.getChannelEventCode(), response.getMessageID()));
                                    }
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.RX_EXT_MESGS_ENABLE_0x66:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.INVALID_MESSAGE_0x28)
                                    {
                                        Console.WriteLine("Extended messages not supported in this ANT product");
                                        break;
                                    }
                                    else if (response.getChannelEventCode() != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                                    {
                                        Console.WriteLine(String.Format("Error {0} configuring {1}", response.getChannelEventCode(), response.getMessageID()));
                                        break;
                                    }
                                    Console.WriteLine("Extended messages enabled");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.REQUEST_0x4D:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.INVALID_MESSAGE_0x28)
                                    {
                                        Console.WriteLine("Requested message not supported in this ANT product");
                                        break;
                                    }
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unhandled response " + response.getChannelEventCode() + " to message " + response.getMessageID()); break;
                                }
                        }
                        break;
                    }
            }
        }


    }
}
