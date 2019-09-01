using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AntPlus.Profiles.BikeRadar;
using AntPlus.Types;
using ANT_Managed_Library;

namespace VariaRx
{
    internal partial class TargetDisplay : Form 
    {
        internal struct DRAW_STRUCT
        {
            internal DateTime TimeStamp;
            internal int speed;
            internal int distance;
            internal string targetText;
            internal float targetTextXpos;
            internal float targetTextYpos;
            internal Font targetTextFont;
            internal SolidBrush targetTextColor;
            internal StringFormat targetTextFormat;
            internal SolidBrush targetColor;
            internal Point[] targetPosition;
        }
        internal volatile VariaRx Context;
        internal Dictionary<int, DRAW_STRUCT> TargetList;
        internal readonly int MAX_TARGETS = 8;
        internal readonly int NO_SPEED = 0;
        internal readonly int SLOW_SPEED = 7;
        internal readonly int MED_SPEED = 14;
        internal readonly int HIGH_SPEED = 21;
        internal readonly int TARGET_SIZE = 10;
        internal readonly int GRAY_OUT_TIME = 5;
        internal readonly int REMOVE_TIME = 8;
        internal readonly int DISP_FONT_SIZE = 7;
        internal SemaphoreSlim targetPointSem;
        internal System.Windows.Forms.Timer UpdateEntriesTimer;

        internal TargetDisplay(VariaRx cntxt)
        {
            InitializeComponent();
            targetPointSem = new SemaphoreSlim(1);
            Context = cntxt;
            TargetList = new Dictionary<int, DRAW_STRUCT>();
            UpdateEntriesTimer = new System.Windows.Forms.Timer();
            UpdateEntriesTimer.Interval = 1000; // specify interval time as you want
            UpdateEntriesTimer.Tick += new EventHandler(UpdateEntries_Tick);
            UpdateEntriesTimer.Start();

        }
       
        internal void UpdateEntries_Tick(object sender, EventArgs e)
        {
            if (targetPointSem.Wait(0) == false) return;
            DateTime currentTime = DateTime.Now;
            for (int counter = 0; counter < 8; counter++)
            {
                if (TargetList.TryGetValue(counter, out DRAW_STRUCT target))
                {                    
                    if (target.TimeStamp != null)
                    {
                        TimeSpan diff = currentTime - target.TimeStamp;
                        if ((diff.Seconds >= GRAY_OUT_TIME) && (diff.Seconds < REMOVE_TIME))
                        {
                            target.targetTextColor = new SolidBrush(Color.Gray);
                            target.targetColor = new SolidBrush(Color.Gray);
                            TargetList.Remove(counter);
                            TargetList.Add(counter, target);
                        }
                        else if (diff.Seconds > REMOVE_TIME)
                        {
                            //TargetList.Remove(counter); Already removed
                        }
                    }
                }
            }            
            targetPointSem.Release();
            this.Invalidate();
        }
        private void TargetDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (targetPointSem.Wait(0) == false) return;
            foreach(DRAW_STRUCT target in TargetList.Values)
            {
                e.Graphics.DrawString(target.targetText, target.targetTextFont, target.targetTextColor, target.targetTextXpos, target.targetTextYpos, target.targetTextFormat);
                e.Graphics.FillPolygon(target.targetColor, target.targetPosition);
            }
            targetPointSem.Release();
        }
        internal void BufferTargs()
        {
            while (Context.VariaAppExt.AppIsClosing == false)
            {
                bool wasUpdated = false;
                Context.VariaAppExt.TargetQueueReadyWait.WaitOne();
                if (Context.VariaAppExt.AppIsClosing == true) { return; }
                List<TargetVars> tempList;
                lock (Context.VariaAppExt.TargetQueueLock)
                {
                    tempList = Context.VariaAppExt.TargetQueue.ToList();
                    Context.VariaAppExt.TargetQueue.Clear();
                }
                targetPointSem.Wait();
                wasUpdated = CreatePoints(tempList);
                targetPointSem.Release();
                tempList.Clear();
                if(wasUpdated == true)  Invalidate();
            }
            
        }
        internal bool CreatePoints(List<TargetVars> vars)
        {
            bool wasUpdated = false;
            foreach (var valSet in vars)
            {                                
                if (CreateTargetPoint(valSet) == true)
                {
                    wasUpdated = true;
                }
            }
            
            return wasUpdated;
        } 
        internal bool CreateTargetPoint(TargetVars target)
        {
            bool updated = false;
            if (target.RangeTarget1 != 0)
            {
                if (TargetList.TryGetValue(1, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(1);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget1);
                targetDraw.distance = CovertMtoF(target.RangeTarget1);
                targetDraw.targetText = "[1] " + target.ThreatSideTarget1.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget1);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);          
                TargetList.Add(1, targetDraw);
                updated = true;
            }

            if (target.RangeTarget2 != 0)
            {
                if (TargetList.TryGetValue(2, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(2);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget2);
                targetDraw.distance = CovertMtoF(target.RangeTarget2);
                targetDraw.targetText = "[2] " + target.ThreatSideTarget2.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget2);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(2, targetDraw);
                updated = true;

            }

            if (target.RangeTarget3 != 0)
            {
                if (TargetList.TryGetValue(3, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(3);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget3);
                targetDraw.distance = CovertMtoF(target.RangeTarget3);
                targetDraw.targetText = "[3] " + target.ThreatSideTarget3.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget3);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(3, targetDraw);
                updated = true;

            }

            if (target.RangeTarget4 != 0)
            {
                if (TargetList.TryGetValue(4, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(4);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget4);
                targetDraw.distance = CovertMtoF(target.RangeTarget4);
                targetDraw.targetText = "[4] " + target.ThreatSideTarget4.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget4);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(4, targetDraw);
                updated = true;

            }

            if (target.RangeTarget5 != 0)
            {
                if (TargetList.TryGetValue(5, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(5);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget5);
                targetDraw.distance = CovertMtoF(target.RangeTarget5);
                targetDraw.targetText = "[5] " + target.ThreatSideTarget5.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget5);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(5, targetDraw);
                updated = true;

            }

            if (target.RangeTarget6 != 0)
            {
                if (TargetList.TryGetValue(6, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(6);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget6);
                targetDraw.distance = CovertMtoF(target.RangeTarget6);
                targetDraw.targetText = "[6] " + target.ThreatSideTarget6.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget6);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(6, targetDraw);
                updated = true;

            }

            if (target.RangeTarget7 != 0)
            {
                if (TargetList.TryGetValue(7, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(7);
                }
                targetDraw.targetPosition = new Point[3];
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget7);
                targetDraw.distance = CovertMtoF(target.RangeTarget7);
                targetDraw.targetText = "[7] " + target.ThreatSideTarget7.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget7);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(7, targetDraw);
                updated = true;

            }

            if (target.RangeTarget8 != 0)
            {
                if (TargetList.TryGetValue(8, out DRAW_STRUCT targetDraw))
                {
                    TargetList.Remove(8);
                }
                targetDraw.TimeStamp = DateTime.Now;
                targetDraw.targetPosition = new Point[3];
                targetDraw.speed = ConvertMetersSecToMPH(target.SpeedTarget8);
                targetDraw.distance = CovertMtoF(target.RangeTarget8);
                targetDraw.targetText = "[8] " + target.ThreatSideTarget8.ToString() + "\n" + targetDraw.speed.ToString() + " MPH Closing\n" + targetDraw.distance.ToString() + " FT Distance";
                targetDraw.targetColor = GetColorByTargetSpeed(targetDraw.speed);
                targetDraw.targetPosition = PlotTargetPosition(targetDraw.distance, target.ThreatSideTarget8);
                targetDraw.targetTextFont = new System.Drawing.Font("Arial", DISP_FONT_SIZE);
                targetDraw.targetTextFormat = new System.Drawing.StringFormat();
                targetDraw.targetTextColor = targetDraw.targetColor;
                targetDraw.targetTextXpos = Convert.ToSingle(targetDraw.targetPosition[2].X + TARGET_SIZE + 10);//distance of text from triangle
                targetDraw.targetTextYpos = Convert.ToSingle(targetDraw.targetPosition[2].Y);
                TargetList.Add(8, targetDraw);
                updated = true;

            }
            return updated;
        }
        internal Point[] PlotTargetPosition(int range, TargetVars.ThreatSideBitField targetSide)
        {            
            double targetRange = Convert.ToDouble(range);
            double scalerY = Convert.ToDouble(this.Height) / 645.75f; //645.75 ft is max range of garmin unit Page 20 ANT+ Managed Network Document – Bike Radar Device Profile, Rev 1.0
            int scalerX = this.Width;
            int TARGET_SIDE_LEFT = TARGET_SIZE;
            int TARGET_SIDE_MIDDLE = (scalerX/2) - 45;
            int TARGET_SIDE_RIGHT = scalerX - (TARGET_SIZE + 100);

            int x3 = 0;

            if (targetSide == TargetVars.ThreatSideBitField.NoSide)
            {
                x3 = TARGET_SIDE_MIDDLE;
            }
            else if (targetSide == TargetVars.ThreatSideBitField.Left)
            {
                x3 = TARGET_SIDE_LEFT;
            }
            else if (targetSide == TargetVars.ThreatSideBitField.Right)
            {
                x3 = TARGET_SIDE_RIGHT;
            }
            int x2 = x3 + TARGET_SIZE;
            int x1 = x3 - TARGET_SIZE;

            int y3 = Convert.ToInt32(scalerY * targetRange);
            int y2 = y3 + TARGET_SIZE;
            int y1 = y3 + TARGET_SIZE;
            Point[] points = { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
            return points;
        }
        internal SolidBrush GetColorByTargetSpeed(int spd)
        {

            if (spd <= NO_SPEED)
            {
                return new SolidBrush(Color.Gray);
            }
            else if (spd > NO_SPEED && spd <= SLOW_SPEED)
            {
                return new SolidBrush(Color.Green);
            }
            else if (spd > SLOW_SPEED && spd <= MED_SPEED)
            {
                return new SolidBrush(Color.DarkRed);
            }
            else if (spd > MED_SPEED)
            {
                return new SolidBrush(Color.Red);
            }
            else
            {
                return new SolidBrush(Color.Yellow); //Unknown speed
            }

        }
        internal int ConvertMetersSecToMPH(byte unitsMetersPerSeconds)
        {
            //1 unit = 3.04m/s PG 20 ANT+ Managed Network Document – Bike Radar Device Profile, Rev 1.0
            double units = Convert.ToDouble(unitsMetersPerSeconds);
            double speedPerUnits = 3.04f;
            double speedInMetersPerSecond = speedPerUnits * units;
            double speedInMPH = 2.23694f * speedInMetersPerSecond;
            return Convert.ToInt32(speedInMPH);
        }
        internal int CovertMtoF(byte unitMeters)
        {
            //1 unit = 3.125m PG 20 ANT+ Managed Network Document – Bike Radar Device Profile, Rev 1.0

            double units = Convert.ToDouble(unitMeters);
            double distancePerUnit = 3.125f;
            double distanceMeters = distancePerUnit * units;
            double feetPerMeter = 3.28084;
            double distanceFeet = distanceMeters * feetPerMeter;
            return Convert.ToInt32(distanceFeet);
        }


    }
}
