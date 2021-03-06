﻿// Copyright (C) 2014 Victor Baybekov

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace QuikSharp {

    // Шаманство с обработкой закрытия может быть нужно, если кровь из носа следует 
    // почистить за собой перед выходом, например снять все заявки или сохранить
    // необработанные данные. Взято из:
    // http://stackoverflow.com/questions/474679/capture-console-exit-c-sharp?lq=1
    // http://stackoverflow.com/questions/1119841/net-console-application-exit-event

    static class Program {

        private static Quik _quik;

        static bool _exitSystem;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler _handler;

        enum CtrlType {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedMember.Local
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        internal static void ManualExitHandler(object sender, EventArgs e) { Handler(CtrlType.CTRL_CLOSE_EVENT); }

        private static bool Handler(CtrlType sig) {
            Trace.WriteLine("Exiting system due to manual close, external CTRL-C, or process kill, or shutdown");
            //do your cleanup here
            Cleanup();
            //allow main to run off
            _exitSystem = true;
            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);
            return true;
        }
        #endregion
        static void Main() {
            // Do not spam console when used as a dependency and use Trace
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());
            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            ServiceManager.StartServices();

            _quik = new Quik();
            //Console.WriteLine("Running in Quik? : " + _quik.Debug.IsQuik().Result);

            _quik.Events.OnAllTrade += Events_OnAllTrade;
            _quik.Events.OnQuote += Events_OnQuote;
            _quik.Events.OnOrder += Events_OnOrder;
            _quik.Events.OnStop += Events_OnStop;
            _quik.Events.OnClose += Events_OnClose;

            // hold the console so it doesn’t run off the end
            while (!_exitSystem) {
                Thread.Sleep(100);
            }
        }

        static void Events_OnOrder(DataStructures.Transaction.Order order) {
            Console.WriteLine("Events_OnOrder: " + order.ToJson());
        }

        static void Events_OnQuote(OrderBook ob) {
            Console.WriteLine("Events_OnQuote: " + ob.ToJson());
        }

        static void Events_OnAllTrade(DataStructures.AllTrade allTrade) {
            Console.WriteLine("Events_OnAllTrade: " + allTrade.ToJson());
        }

        static void Events_OnClose() {
            Console.WriteLine("Events_OnQuote: ");
        }

        static void Events_OnStop(int signal) {
            Console.WriteLine("Events_OnStop: " + signal);
        }

        static void Cleanup() {
#if DEBUG
            System.Windows.Forms.MessageBox.Show("Bye!");
#endif
            Console.WriteLine("Bye!");
            ServiceManager.StopServices();
        }

    }



}