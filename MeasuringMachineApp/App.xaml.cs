﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DataBase;
using PLCInterface;
using ToolOffset;

namespace MeasuringMachineApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // DatabaseScanTimer
        System.Threading.Timer TimerDB1;
        System.Threading.Timer TimerDB2;
        // Static Class definition
        public static PLCInterface.Interface PlcInterface;
        public static MainWindow mwHandle;
        public static MyDatabase Database;
        public static MyCorrectionDatabase CorrectionDatabaseM1;
        public static MyCorrectionDatabase CorrectionDatabaseM2;

        // This is made static to access from another GUI class
        public static MeasurmentCalculation MeasurmentCalculationM1;
        public static MeasurmentCalculation MeasurmentCalculationM2;
        public static MeasurementData MeasurementDataM1;
        public static MeasurementData MeasurementDataM2;
        public static CNCInterface.Interface CncInterfaceM1;
        public static CNCInterface.Interface CncInterfaceM2;

        // SslMode=none - If local host does not support SSL
        static string MySQLconnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=mjernastanica;SslMode=none";
        private bool _oneCallFlagSaveM1;
        private bool _oneCallFlagSaveM2;
 

        public App()
        {
            PlcInterface = new PLCInterface.Interface();
            Database = new MyDatabase();
            CorrectionDatabaseM1 = new MyCorrectionDatabase();
            CorrectionDatabaseM2 = new MyCorrectionDatabase();
            MeasurmentCalculationM1 = new MeasurmentCalculation();
            MeasurmentCalculationM2 = new MeasurmentCalculation();
            MeasurementDataM1 = new MeasurementData();
            MeasurementDataM2 = new MeasurementData();
            CncInterfaceM1 = new CNCInterface.Interface();
            CncInterfaceM2 = new CNCInterface.Interface();
            PlcInterface.StartCyclic(); // Possible system null reference
            PlcInterface.Update_Online_Flag += new Interface.OnlineMarker(PLCInterface_PLCOnlineChanged);
            PlcInterface.Update_100_ms += new Interface.UpdateHandler(PLC_Update_100_ms);
            // Database check Machine1
            MeasurmentCalculationM1.DatabaseChanged += OnDatabaseChangedM1;
            TimerDB1 = new System.Threading.Timer(TickDB1, null, 8000, 5000);
            // Database check Machine2
            MeasurmentCalculationM2.DatabaseChanged += OnDatabaseChangedM2;
            TimerDB2 = new System.Threading.Timer(TickDB2, null, 8000, 5000);
        }

        // New worker thread
        public void TickDB1(object info1)
        {
            MeasurmentCalculationM1.CompareWorkOrder(MySQLconnectionString, "stroj1");
            MeasurmentCalculationM1.DatabaseCount(MySQLconnectionString, "stroj1");
        }
        
        // New worker thread
        public void TickDB2(object info2)
        {
            MeasurmentCalculationM2.CompareWorkOrder(MySQLconnectionString, "stroj2");
            MeasurmentCalculationM2.DatabaseCount(MySQLconnectionString, "stroj2");
        }
        
        // Calculate correction when we have results for M2
        public void OnDatabaseChangedM1(object source, EventArgs e)
        {
            // Corection value for diameter C
            if (MeasurmentCalculationM1.CAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM1.CAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.CAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.CAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.CAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionCno1 = MeasurmentCalculationM1.CAverageValueMeas1 - MeasurementDataM1.cNominal;
                MeasurementDataM1.CorrectionCno2 = MeasurmentCalculationM1.CAverageValueMeas2 - MeasurementDataM1.cNominal;
                MeasurementDataM1.CorrectionCno3 = MeasurmentCalculationM1.CAverageValueMeas3 - MeasurementDataM1.cNominal;
                MeasurementDataM1.CorrectionCno4 = MeasurmentCalculationM1.CAverageValueMeas4 - MeasurementDataM1.cNominal;
                MeasurementDataM1.CorrectionCno5 = MeasurmentCalculationM1.CAverageValueMeas5 - MeasurementDataM1.cNominal;
                MeasurementDataM1.CorrectionCforMachine = ((MeasurementDataM1.CorrectionCno1 + MeasurementDataM1.CorrectionCno2 +
                                                            MeasurementDataM1.CorrectionCno3 + MeasurementDataM1.CorrectionCno4 +
                                                            MeasurementDataM1.CorrectionCno5) / 5);

                MeasurementDataM1.CorrectionCforMachine = (float) Math.Round(MeasurementDataM1.CorrectionCforMachine,3);
            }
            else
            {
                MeasurementDataM1.CorrectionCforMachine = 0;
            }
            
            // Corection value for diameter A (Two Point)
            if (MeasurmentCalculationM1.AtwoPointAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM1.AtwoPointAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.AtwoPointAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.AtwoPointAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.AtwoPointAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionA2no1 = MeasurmentCalculationM1.AtwoPointAverageValueMeas1 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA2no2 = MeasurmentCalculationM1.AtwoPointAverageValueMeas2 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA2no3 = MeasurmentCalculationM1.AtwoPointAverageValueMeas3 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA2no4 = MeasurmentCalculationM1.AtwoPointAverageValueMeas4 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA2no5 = MeasurmentCalculationM1.AtwoPointAverageValueMeas5 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA2forMachine = ((MeasurementDataM1.CorrectionA2no1 + MeasurementDataM1.CorrectionA2no2 +
                                                             MeasurementDataM1.CorrectionA2no3 + MeasurementDataM1.CorrectionA2no4 +
                                                             MeasurementDataM1.CorrectionA2no5) / 5);

                MeasurementDataM1.CorrectionA2forMachine = (float) Math.Round(MeasurementDataM1.CorrectionA2forMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionA2forMachine = 0;
            }
            
            // Corection value for diameter A (One Point)
            if (MeasurmentCalculationM1.AonePointAverageValueMeas1 !=0.0 &
                MeasurmentCalculationM1.AonePointAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.AonePointAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.AonePointAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.AonePointAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionA1no1 = MeasurmentCalculationM1.AonePointAverageValueMeas1 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA1no2 = MeasurmentCalculationM1.AonePointAverageValueMeas2 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA1no3 = MeasurmentCalculationM1.AonePointAverageValueMeas3 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA1no4 = MeasurmentCalculationM1.AonePointAverageValueMeas4 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA1no5 = MeasurmentCalculationM1.AonePointAverageValueMeas5 - MeasurementDataM1.aNominal;
                MeasurementDataM1.CorrectionA1forMachine = ((MeasurementDataM1.CorrectionA1no1 + MeasurementDataM1.CorrectionA1no2 +
                                                             MeasurementDataM1.CorrectionA1no3 + MeasurementDataM1.CorrectionA1no4 +
                                                             MeasurementDataM1.CorrectionA1no5) / 5);

                MeasurementDataM1.CorrectionA1forMachine = (float) Math.Round(MeasurementDataM1.CorrectionA1forMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionA1forMachine = 0;
            }
            
            // Corection value for diameter B
            if (MeasurmentCalculationM1.BAverageValueMeas1 !=0.0 &
                MeasurmentCalculationM1.BAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.BAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.BAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.BAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionBno1 = MeasurmentCalculationM1.BAverageValueMeas1 - MeasurementDataM1.bNominal;
                MeasurementDataM1.CorrectionBno2 = MeasurmentCalculationM1.BAverageValueMeas2 - MeasurementDataM1.bNominal;
                MeasurementDataM1.CorrectionBno3 = MeasurmentCalculationM1.BAverageValueMeas3 - MeasurementDataM1.bNominal;
                MeasurementDataM1.CorrectionBno4 = MeasurmentCalculationM1.BAverageValueMeas4 - MeasurementDataM1.bNominal;
                MeasurementDataM1.CorrectionBno5 = MeasurmentCalculationM1.BAverageValueMeas5 - MeasurementDataM1.bNominal;
                MeasurementDataM1.CorrectionBforMachine = ((MeasurementDataM1.CorrectionBno1 + MeasurementDataM1.CorrectionBno2 +
                                                            MeasurementDataM1.CorrectionBno3 + MeasurementDataM1.CorrectionBno4 +
                                                            MeasurementDataM1.CorrectionBno5) / 5);

                MeasurementDataM1.CorrectionBforMachine = (float) Math.Round(MeasurementDataM1.CorrectionBforMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionBforMachine = 0;
            }
            
            // Corection value for diameter J - Add new Measurement
            if (MeasurmentCalculationM1.JAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM1.JAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.JAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.JAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.JAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionJno1 = MeasurmentCalculationM1.JAverageValueMeas1 - MeasurementDataM1.jNominal;
                MeasurementDataM1.CorrectionJno2 = MeasurmentCalculationM1.JAverageValueMeas2 - MeasurementDataM1.jNominal;
                MeasurementDataM1.CorrectionJno3 = MeasurmentCalculationM1.JAverageValueMeas3 - MeasurementDataM1.jNominal;
                MeasurementDataM1.CorrectionJno4 = MeasurmentCalculationM1.JAverageValueMeas4 - MeasurementDataM1.jNominal;
                MeasurementDataM1.CorrectionJno5 = MeasurmentCalculationM1.JAverageValueMeas5 - MeasurementDataM1.jNominal;
                MeasurementDataM1.CorrectionJforMachine = ((MeasurementDataM1.CorrectionJno1 + MeasurementDataM1.CorrectionJno2 +
                                                            MeasurementDataM1.CorrectionJno3 + MeasurementDataM1.CorrectionJno4 +
                                                            MeasurementDataM1.CorrectionJno5) / 5);

                MeasurementDataM1.CorrectionJforMachine = (float) Math.Round(MeasurementDataM1.CorrectionJforMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionJforMachine = 0;
            }
            

            // Corection value for diameter F
            if (MeasurmentCalculationM1.FAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM1.FAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.FAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.FAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.FAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionFno1 = MeasurmentCalculationM1.FAverageValueMeas1 - MeasurementDataM1.fNominal;
                MeasurementDataM1.CorrectionFno2 = MeasurmentCalculationM1.FAverageValueMeas2 - MeasurementDataM1.fNominal;
                MeasurementDataM1.CorrectionFno3 = MeasurmentCalculationM1.FAverageValueMeas3 - MeasurementDataM1.fNominal;
                MeasurementDataM1.CorrectionFno4 = MeasurmentCalculationM1.FAverageValueMeas4 - MeasurementDataM1.fNominal;
                MeasurementDataM1.CorrectionFno5 = MeasurmentCalculationM1.FAverageValueMeas5 - MeasurementDataM1.fNominal;
                MeasurementDataM1.CorrectionFforMachine = ((MeasurementDataM1.CorrectionFno1 + MeasurementDataM1.CorrectionFno2 +
                                                            MeasurementDataM1.CorrectionFno3 + MeasurementDataM1.CorrectionFno4 +
                                                            MeasurementDataM1.CorrectionFno5) / 5);

                MeasurementDataM1.CorrectionFforMachine = (float) Math.Round(MeasurementDataM1.CorrectionFforMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionFforMachine = 0;
            }
            
            // Corection value for diameter E
            if (MeasurmentCalculationM1.EAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM1.EAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM1.EAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM1.EAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM1.EAverageValueMeas5 != 0.0 )
            {
                MeasurementDataM1.CorrectionEno1 = MeasurmentCalculationM1.EAverageValueMeas1 - MeasurementDataM1.eNominal;
                MeasurementDataM1.CorrectionEno2 = MeasurmentCalculationM1.EAverageValueMeas2 - MeasurementDataM1.eNominal;
                MeasurementDataM1.CorrectionEno3 = MeasurmentCalculationM1.EAverageValueMeas3 - MeasurementDataM1.eNominal;
                MeasurementDataM1.CorrectionEno4 = MeasurmentCalculationM1.EAverageValueMeas4 - MeasurementDataM1.eNominal;
                MeasurementDataM1.CorrectionEno5 = MeasurmentCalculationM1.EAverageValueMeas5 - MeasurementDataM1.eNominal;
                MeasurementDataM1.CorrectionEforMachine = ((MeasurementDataM1.CorrectionEno1 + MeasurementDataM1.CorrectionEno2 +
                                                            MeasurementDataM1.CorrectionEno3 + MeasurementDataM1.CorrectionEno4 +
                                                            MeasurementDataM1.CorrectionEno5) / 5);

                MeasurementDataM1.CorrectionEforMachine = (float) Math.Round(MeasurementDataM1.CorrectionEforMachine, 3);
            }
            else
            {
                MeasurementDataM1.CorrectionEforMachine = 0;
            }
            
            // Corection value for diameter D
            //MeasurementDataM1.CorrectionDno1 = MeasurmentCalculationM2.DAverageValueMeas1 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno2 = MeasurmentCalculationM2.DAverageValueMeas2 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno3 = MeasurmentCalculationM2.DAverageValueMeas3 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno4 = MeasurmentCalculationM2.DAverageValueMeas4 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno5 = MeasurmentCalculationM2.DAverageValueMeas5 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDforMachine = (MeasurementDataM1.CorrectionDno1 + MeasurementDataM1.CorrectionDno2 +
            //                                         MeasurementDataM1.CorrectionDno3 + MeasurementDataM1.CorrectionDno4 +
            //                                         MeasurementDataM1.CorrectionDno5);

            // Set Workorder number
            CorrectionDatabaseM1.RadniNalog = MeasurmentCalculationM1.LastWorkOrder;
            CorrectionDatabaseM1.CorrectionCforMachine = MeasurementDataM1.CorrectionCforMachine;
            CorrectionDatabaseM1.CorrectionA2forMachine = MeasurementDataM1.CorrectionA2forMachine;
            CorrectionDatabaseM1.CorrectionA1forMachine = MeasurementDataM1.CorrectionA1forMachine;
            CorrectionDatabaseM1.CorrectionBforMachine = MeasurementDataM1.CorrectionBforMachine;
            CorrectionDatabaseM1.CorrectionJforMachine = MeasurementDataM1.CorrectionJforMachine;
            CorrectionDatabaseM1.CorrectionFforMachine = MeasurementDataM1.CorrectionFforMachine;
            CorrectionDatabaseM1.CorrectionEforMachine = MeasurementDataM1.CorrectionEforMachine;
            CorrectionDatabaseM1.ModifyDb(MySQLconnectionString, "korekcijestroj1");
        }

        // Calculate correction when we have results for M2
        public void OnDatabaseChangedM2(object source, EventArgs e)
        {
            // Corection value for diameter C
            if (MeasurmentCalculationM2.CAverageValueMeas1 != 0.0 & 
                MeasurmentCalculationM2.CAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.CAverageValueMeas3 != 0.0 & 
                MeasurmentCalculationM2.CAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.CAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionCno1 = MeasurmentCalculationM2.CAverageValueMeas1 - MeasurementDataM2.cNominal;
                MeasurementDataM2.CorrectionCno2 = MeasurmentCalculationM2.CAverageValueMeas2 - MeasurementDataM2.cNominal;
                MeasurementDataM2.CorrectionCno3 = MeasurmentCalculationM2.CAverageValueMeas3 - MeasurementDataM2.cNominal;
                MeasurementDataM2.CorrectionCno4 = MeasurmentCalculationM2.CAverageValueMeas4 - MeasurementDataM2.cNominal;
                MeasurementDataM2.CorrectionCno5 = MeasurmentCalculationM2.CAverageValueMeas5 - MeasurementDataM2.cNominal;
                MeasurementDataM2.CorrectionCforMachine = ((MeasurementDataM2.CorrectionCno1 + MeasurementDataM2.CorrectionCno2 +
                                                            MeasurementDataM2.CorrectionCno3 + MeasurementDataM2.CorrectionCno4 +
                                                            MeasurementDataM2.CorrectionCno5) / 5);
                // Round with 3 decimal places
                MeasurementDataM2.CorrectionCforMachine =(float) Math.Round(MeasurementDataM2.CorrectionCforMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionCforMachine = 0;
            }

            // Corection value for diameter A (Two Point)
            if (MeasurmentCalculationM2.AtwoPointAverageValueMeas1 != 0.0 & 
                MeasurmentCalculationM2.AtwoPointAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.AtwoPointAverageValueMeas3 != 0.0 & 
                MeasurmentCalculationM2.AtwoPointAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.AtwoPointAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionA2no1 = MeasurmentCalculationM2.AtwoPointAverageValueMeas1 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA2no2 = MeasurmentCalculationM2.AtwoPointAverageValueMeas2 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA2no3 = MeasurmentCalculationM2.AtwoPointAverageValueMeas3 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA2no4 = MeasurmentCalculationM2.AtwoPointAverageValueMeas4 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA2no5 = MeasurmentCalculationM2.AtwoPointAverageValueMeas5 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA2forMachine = ((MeasurementDataM2.CorrectionA2no1 + MeasurementDataM2.CorrectionA2no2 +
                                                             MeasurementDataM2.CorrectionA2no3 + MeasurementDataM2.CorrectionA2no4 +
                                                             MeasurementDataM2.CorrectionA2no5) / 5);

                MeasurementDataM2.CorrectionA2forMachine = (float) Math.Round(MeasurementDataM2.CorrectionA2forMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionA2forMachine = 0;
            }
            
            // Corection value for diameter A (One Point)
            if (MeasurmentCalculationM2.AonePointAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM2.AonePointAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.AonePointAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM2.AonePointAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.AonePointAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionA1no1 = MeasurmentCalculationM2.AonePointAverageValueMeas1 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA1no2 = MeasurmentCalculationM2.AonePointAverageValueMeas2 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA1no3 = MeasurmentCalculationM2.AonePointAverageValueMeas3 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA1no4 = MeasurmentCalculationM2.AonePointAverageValueMeas4 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA1no5 = MeasurmentCalculationM2.AonePointAverageValueMeas5 - MeasurementDataM2.aNominal;
                MeasurementDataM2.CorrectionA1forMachine = ((MeasurementDataM2.CorrectionA1no1 + MeasurementDataM2.CorrectionA1no2 +
                                                             MeasurementDataM2.CorrectionA1no3 + MeasurementDataM2.CorrectionA1no4 +
                                                             MeasurementDataM2.CorrectionA1no5) / 5);

                MeasurementDataM2.CorrectionA1forMachine = (float) Math.Round(MeasurementDataM2.CorrectionA1forMachine, 3);
            }
            
            // Corection value for diameter B
            if (MeasurmentCalculationM2.BAverageValueMeas1 != 0.0 & 
                MeasurmentCalculationM2.BAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.BAverageValueMeas3 != 0.0 & 
                MeasurmentCalculationM2.BAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.BAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionBno1 = MeasurmentCalculationM2.BAverageValueMeas1 - MeasurementDataM2.bNominal;
                MeasurementDataM2.CorrectionBno2 = MeasurmentCalculationM2.BAverageValueMeas2 - MeasurementDataM2.bNominal;
                MeasurementDataM2.CorrectionBno3 = MeasurmentCalculationM2.BAverageValueMeas3 - MeasurementDataM2.bNominal;
                MeasurementDataM2.CorrectionBno4 = MeasurmentCalculationM2.BAverageValueMeas4 - MeasurementDataM2.bNominal;
                MeasurementDataM2.CorrectionBno5 = MeasurmentCalculationM2.BAverageValueMeas5 - MeasurementDataM2.bNominal;
                MeasurementDataM2.CorrectionBforMachine = ((MeasurementDataM2.CorrectionBno1 + MeasurementDataM2.CorrectionBno2 +
                                                            MeasurementDataM2.CorrectionBno3 + MeasurementDataM2.CorrectionBno4 +
                                                            MeasurementDataM2.CorrectionBno5) / 5);

                MeasurementDataM2.CorrectionBforMachine = (float)Math.Round(MeasurementDataM2.CorrectionBforMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionBforMachine = 0;
            }
            
            // Corection value for diameter J - Add new Measurement
            if (MeasurmentCalculationM2.JAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM2.JAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.JAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM2.JAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.JAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionJno1 = MeasurmentCalculationM2.JAverageValueMeas1 - MeasurementDataM2.jNominal;
                MeasurementDataM2.CorrectionJno2 = MeasurmentCalculationM2.JAverageValueMeas2 - MeasurementDataM2.jNominal;
                MeasurementDataM2.CorrectionJno3 = MeasurmentCalculationM2.JAverageValueMeas3 - MeasurementDataM2.jNominal;
                MeasurementDataM2.CorrectionJno4 = MeasurmentCalculationM2.JAverageValueMeas4 - MeasurementDataM2.jNominal;
                MeasurementDataM2.CorrectionJno5 = MeasurmentCalculationM2.JAverageValueMeas5 - MeasurementDataM2.jNominal;
                MeasurementDataM2.CorrectionJforMachine = ((MeasurementDataM2.CorrectionJno1 + MeasurementDataM2.CorrectionJno2 +
                                                            MeasurementDataM2.CorrectionJno3 + MeasurementDataM2.CorrectionJno4 +
                                                            MeasurementDataM2.CorrectionJno5) / 5);

                MeasurementDataM2.CorrectionJforMachine = (float)Math.Round(MeasurementDataM2.CorrectionJforMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionJforMachine = 0;
            }

            // Corection value for diameter F
            if (MeasurmentCalculationM2.FAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM2.FAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.FAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM2.FAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.FAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionFno1 = MeasurmentCalculationM2.FAverageValueMeas1 - MeasurementDataM2.fNominal;
                MeasurementDataM2.CorrectionFno2 = MeasurmentCalculationM2.FAverageValueMeas2 - MeasurementDataM2.fNominal;
                MeasurementDataM2.CorrectionFno3 = MeasurmentCalculationM2.FAverageValueMeas3 - MeasurementDataM2.fNominal;
                MeasurementDataM2.CorrectionFno4 = MeasurmentCalculationM2.FAverageValueMeas4 - MeasurementDataM2.fNominal;
                MeasurementDataM2.CorrectionFno5 = MeasurmentCalculationM2.FAverageValueMeas5 - MeasurementDataM2.fNominal;
                MeasurementDataM2.CorrectionFforMachine = ((MeasurementDataM2.CorrectionFno1 + MeasurementDataM2.CorrectionFno2 +
                                                            MeasurementDataM2.CorrectionFno3 + MeasurementDataM2.CorrectionFno4 +
                                                            MeasurementDataM2.CorrectionFno5) / 5);

                MeasurementDataM2.CorrectionFforMachine = (float) Math.Round(MeasurementDataM2.CorrectionFforMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionFforMachine = 0;
            }
            
            // Corection value for diameter E
            if (MeasurmentCalculationM2.EAverageValueMeas1 != 0.0 &
                MeasurmentCalculationM2.EAverageValueMeas2 != 0.0 &
                MeasurmentCalculationM2.EAverageValueMeas3 != 0.0 &
                MeasurmentCalculationM2.EAverageValueMeas4 != 0.0 &
                MeasurmentCalculationM2.EAverageValueMeas5 != 0.0)
            {
                MeasurementDataM2.CorrectionEno1 = MeasurmentCalculationM2.EAverageValueMeas1 - MeasurementDataM2.eNominal;
                MeasurementDataM2.CorrectionEno2 = MeasurmentCalculationM2.EAverageValueMeas2 - MeasurementDataM2.eNominal;
                MeasurementDataM2.CorrectionEno3 = MeasurmentCalculationM2.EAverageValueMeas3 - MeasurementDataM2.eNominal;
                MeasurementDataM2.CorrectionEno4 = MeasurmentCalculationM2.EAverageValueMeas4 - MeasurementDataM2.eNominal;
                MeasurementDataM2.CorrectionEno5 = MeasurmentCalculationM2.EAverageValueMeas5 - MeasurementDataM2.eNominal;
                MeasurementDataM2.CorrectionEforMachine = ((MeasurementDataM2.CorrectionEno1 + MeasurementDataM2.CorrectionEno2 +
                                                            MeasurementDataM2.CorrectionEno3 + MeasurementDataM2.CorrectionEno4 +
                                                            MeasurementDataM2.CorrectionEno5) / 5);

                MeasurementDataM2.CorrectionEforMachine = (float) Math.Round(MeasurementDataM2.CorrectionEforMachine, 3);
            }
            else
            {
                MeasurementDataM2.CorrectionEforMachine = 0;
            }
            
            // Corection value for diameter D
            //MeasurementDataM1.CorrectionDno1 = MeasurmentCalculationM2.DAverageValueMeas1 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno2 = MeasurmentCalculationM2.DAverageValueMeas2 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno3 = MeasurmentCalculationM2.DAverageValueMeas3 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno4 = MeasurmentCalculationM2.DAverageValueMeas4 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDno5 = MeasurmentCalculationM2.DAverageValueMeas5 - MeasurementDataM1.dNominalM2;
            //MeasurementDataM1.CorrectionDforMachine = (MeasurementDataM1.CorrectionDno1 + MeasurementDataM1.CorrectionDno2 +
            //                                         MeasurementDataM1.CorrectionDno3 + MeasurementDataM1.CorrectionDno4 +
            //                                         MeasurementDataM1.CorrectionDno5);

            // Set Workorder number
            CorrectionDatabaseM2.RadniNalog = MeasurmentCalculationM2.LastWorkOrder;
            CorrectionDatabaseM2.CorrectionCforMachine = MeasurementDataM2.CorrectionCforMachine;
            CorrectionDatabaseM2.CorrectionA2forMachine = MeasurementDataM2.CorrectionA2forMachine;
            CorrectionDatabaseM2.CorrectionA1forMachine = MeasurementDataM2.CorrectionA1forMachine;
            CorrectionDatabaseM2.CorrectionBforMachine = MeasurementDataM2.CorrectionBforMachine;
            CorrectionDatabaseM2.CorrectionJforMachine = MeasurementDataM2.CorrectionJforMachine;
            CorrectionDatabaseM2.CorrectionFforMachine = MeasurementDataM2.CorrectionFforMachine;
            CorrectionDatabaseM2.CorrectionEforMachine = MeasurementDataM2.CorrectionEforMachine;
            CorrectionDatabaseM2.ModifyDb(MySQLconnectionString, "korekcijestroj2");
        }

        private void PLC_Update_100_ms(Interface sender, InterfaceEventArgs e)
        {
            String msg = "SISTEM SPREMAN";
            // Workpiece Nominal Values from Machine 1
            MeasurementDataM1.cNominal = (float)e.StatusData.Machine1Data.C.Value;
            MeasurementDataM1.aNominal = (float)e.StatusData.Machine1Data.A.Value;
            MeasurementDataM1.bNominal = (float)e.StatusData.Machine1Data.B.Value;
            MeasurementDataM1.jNominal = (float)e.StatusData.Machine1Data.J.Value;
            MeasurementDataM1.fNominal = (float)e.StatusData.Machine1Data.F.Value;
            MeasurementDataM1.eNominal = (float)e.StatusData.Machine1Data.E.Value;
            MeasurementDataM1.dNominal = (float)e.StatusData.Machine1Data.D.Value;
            MeasurementDataM1.gNominal = (float)e.StatusData.Machine1Data.G.Value;
            // Workpiece Nominal Values from Machine 2
            MeasurementDataM2.cNominal = (float)e.StatusData.Machine2Data.C.Value;
            MeasurementDataM2.aNominal = (float)e.StatusData.Machine2Data.A.Value;
            MeasurementDataM2.bNominal = (float)e.StatusData.Machine2Data.B.Value;
            MeasurementDataM2.jNominal = (float)e.StatusData.Machine2Data.J.Value;
            MeasurementDataM2.fNominal = (float)e.StatusData.Machine2Data.F.Value;
            MeasurementDataM2.eNominal = (float)e.StatusData.Machine2Data.E.Value;
            MeasurementDataM2.dNominal = (float)e.StatusData.Machine2Data.D.Value;
            MeasurementDataM2.gNominal = (float)e.StatusData.Machine2Data.G.Value;

            // Signal to fill SQL Database for Machine1
            if ((bool)e.StatusData.Savedata.M1.Value && _oneCallFlagSaveM1)
            {
                _oneCallFlagSaveM1 = false;
                // Value setting
                // C
                Database.KotaCPoz1 = (float)e.StatusData.MeasuredPos1.C.Value;
                Database.KotaCPoz2 = (float)e.StatusData.MeasuredPos2.C.Value;
                // A1.2
                Database.KotaA12Poz1 = (float)e.StatusData.MeasuredPos1.A12.Value;
                Database.KotaA12Poz2 = (float)e.StatusData.MeasuredPos2.A12.Value;
                // A1.1
                Database.KotaA11Poz1 = (float)e.StatusData.MeasuredPos1.A11.Value;
                Database.KotaA11Poz2 = (float)e.StatusData.MeasuredPos2.A11.Value;
                // A
                Database.KotaAPoz1 = (float)e.StatusData.MeasuredPos1.A.Value;
                Database.KotaAPoz2 = (float)e.StatusData.MeasuredPos2.A.Value;
                // B
                Database.KotaBPoz1 = (float)e.StatusData.MeasuredPos1.B.Value;
                Database.KotaBPoz2 = (float)e.StatusData.MeasuredPos2.B.Value;
                // J
                Database.KotaJPoz1 = (float)e.StatusData.MeasuredPos1.J.Value;
                Database.KotaJPoz2 = (float)e.StatusData.MeasuredPos2.J.Value;
                // F1 AND F2 POS 1
                Database.KotaF1LG2Poz1 = (float)e.StatusData.MeasuredPos1.F1LG2.Value;
                Database.KotaF2LG2Poz1 = (float)e.StatusData.MeasuredPos1.F2LG2.Value;
                Database.KotaF1LG3Poz1 = (float)e.StatusData.MeasuredPos1.F1LG3.Value;
                Database.KotaF2LG3Poz1 = (float)e.StatusData.MeasuredPos1.F2LG3.Value;
                // F1 AND F2 POS 2
                Database.KotaF1LG2Poz2 = (float)e.StatusData.MeasuredPos2.F1LG2.Value;
                Database.KotaF2LG2Poz2 = (float)e.StatusData.MeasuredPos2.F2LG2.Value;
                Database.KotaF1LG3Poz2 = (float)e.StatusData.MeasuredPos2.F1LG3.Value;
                Database.KotaF2LG3Poz2 = (float)e.StatusData.MeasuredPos2.F2LG3.Value;
                // E
                Database.KotaEPoz1 = (float)e.StatusData.MeasuredPos1.E.Value;
                Database.KotaEPoz2 = (float)e.StatusData.MeasuredPos2.E.Value;
                // D
                Database.KotaDPoz1 = (float)e.StatusData.MeasuredPos1.D.Value;
                Database.KotaDPoz2 = (float)e.StatusData.MeasuredPos2.D.Value;
                // H1
                Database.KotaH1Poz1 = (float)e.StatusData.MeasuredPos1.H1.Value;
                Database.KotaH1Poz2 = (float)e.StatusData.MeasuredPos2.H1.Value;
                // K
                Database.KotaKPoz1 = (float)e.StatusData.MeasuredPos1.K.Value;
                Database.KotaKPoz2 = (float)e.StatusData.MeasuredPos2.K.Value;

                // Workpiecedata
                Database.RadniNalog = (string)e.StatusData.Workpiecedata.RadniNalog.Value;

                // Fill SQL base for Machine 1
                // New thread
                Thread WriteToDatabaseM1 = new Thread(() => Database.ModifyDb(MySQLconnectionString, "stroj1"));
                WriteToDatabaseM1.Name = "WriteToDatabaseM1";
                WriteToDatabaseM1.Start();
            }
            else if (!(bool)e.StatusData.Savedata.M1.Value)
            {
                _oneCallFlagSaveM1 = true;
            }

            // Signal to fill SQL Database for Machine2
            if ((bool)e.StatusData.Savedata.M2.Value && _oneCallFlagSaveM2)
            {
                _oneCallFlagSaveM2 = false;
                // Value setting
                // C
                Database.KotaCPoz1 = (float)e.StatusData.MeasuredPos1.C.Value;
                Database.KotaCPoz2 = (float)e.StatusData.MeasuredPos2.C.Value;
                // A1.2
                Database.KotaA12Poz1 = (float)e.StatusData.MeasuredPos1.A12.Value;
                Database.KotaA12Poz2 = (float)e.StatusData.MeasuredPos2.A12.Value;
                // A1.1
                Database.KotaA11Poz1 = (float)e.StatusData.MeasuredPos1.A11.Value;
                Database.KotaA11Poz2 = (float)e.StatusData.MeasuredPos2.A11.Value;
                // A
                Database.KotaAPoz1 = (float)e.StatusData.MeasuredPos1.A.Value;
                Database.KotaAPoz2 = (float)e.StatusData.MeasuredPos2.A.Value;
                // B
                Database.KotaBPoz1 = (float)e.StatusData.MeasuredPos1.B.Value;
                Database.KotaBPoz2 = (float)e.StatusData.MeasuredPos2.B.Value;
                // J
                Database.KotaJPoz1 = (float)e.StatusData.MeasuredPos1.J.Value;
                Database.KotaJPoz2 = (float)e.StatusData.MeasuredPos2.J.Value;
                // F1 AND F2 POS 1
                Database.KotaF1LG2Poz1 = (float)e.StatusData.MeasuredPos1.F1LG2.Value;
                Database.KotaF2LG2Poz1 = (float)e.StatusData.MeasuredPos1.F2LG2.Value;
                Database.KotaF1LG3Poz1 = (float)e.StatusData.MeasuredPos1.F1LG3.Value;
                Database.KotaF2LG3Poz1 = (float)e.StatusData.MeasuredPos1.F2LG3.Value;
                // F1 AND F2 POS 2
                Database.KotaF1LG2Poz2 = (float)e.StatusData.MeasuredPos2.F1LG2.Value;
                Database.KotaF2LG2Poz2 = (float)e.StatusData.MeasuredPos2.F2LG2.Value;
                Database.KotaF1LG3Poz2 = (float)e.StatusData.MeasuredPos2.F1LG3.Value;
                Database.KotaF2LG3Poz2 = (float)e.StatusData.MeasuredPos2.F2LG3.Value;
                // E
                Database.KotaEPoz1 = (float)e.StatusData.MeasuredPos1.E.Value;
                Database.KotaEPoz2 = (float)e.StatusData.MeasuredPos2.E.Value;
                // D
                Database.KotaDPoz1 = (float)e.StatusData.MeasuredPos1.D.Value;
                Database.KotaDPoz2 = (float)e.StatusData.MeasuredPos2.D.Value;
                // H1
                Database.KotaH1Poz1 = (float)e.StatusData.MeasuredPos1.H1.Value;
                Database.KotaH1Poz2 = (float)e.StatusData.MeasuredPos2.H1.Value;
                // K
                Database.KotaKPoz1 = (float)e.StatusData.MeasuredPos1.K.Value;
                Database.KotaKPoz2 = (float)e.StatusData.MeasuredPos2.K.Value;

                // Workpiecedata
                Database.RadniNalog = (string)e.StatusData.Workpiecedata.RadniNalog.Value;

                // Fill SQL base for Machine 1
                // New thread
                Thread WriteToDatabaseM2 = new Thread(() => Database.ModifyDb(MySQLconnectionString, "stroj2"));
                WriteToDatabaseM2.Name = "WriteToDatabaseM2";
                WriteToDatabaseM2.Start();
            }
            else if (!(bool)e.StatusData.Savedata.M2.Value)
            {
                _oneCallFlagSaveM2 = true;
            }

            if (mwHandle != null)
            {
                mwHandle.TbStatusMessage.Dispatcher.BeginInvoke((Action)(() => { mwHandle.TbStatusMessage.Text = msg; }));
            }

        }

        private void PLCInterface_PLCOnlineChanged(object sender, OnlineMarkerEventArgs e)
        {
            if (e.OnlineMark)
            {
                mwHandle.onlineFlag.Dispatcher.BeginInvoke((Action)(() => { mwHandle.onlineFlag.Fill = new LinearGradientBrush(Colors.Green, Colors.White, 0.0); }));
                mwHandle.TbConnectionStatus.Dispatcher.BeginInvoke((Action)(() => { mwHandle.TbConnectionStatus.Text = "PLC Status: Online"; }));
            }
            else
            {
                mwHandle.onlineFlag.Dispatcher.BeginInvoke((Action)(() => { mwHandle.onlineFlag.Fill = new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#FF979797"), Colors.White, 0.0); }));
                mwHandle.TbConnectionStatus.Dispatcher.BeginInvoke((Action)(() => { mwHandle.TbConnectionStatus.Text = "PLC Status: Offline"; }));
            }
        }
    }
}
