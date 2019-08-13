﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace CandidateTest.Threads
{
    class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static DateTime timeToBeCompleted;
        static bool completedByTime;
        static System.Timers.Timer aTimer;
        static FileLogger dataLogger = new FileLogger("..\\..\\Output\\data.txt");
        static FileLogger statisticLogger = new FileLogger("..\\..\\Output\\Statistics.txt", System.IO.FileMode.Open);
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 100;
            aTimer.Enabled = true;

            timeToBeCompleted = DateTime.Now.AddMinutes(5);
            Console.WriteLine("Should be automatically stopped at " + timeToBeCompleted.ToShortTimeString());
            Process currentProcess = Process.GetCurrentProcess();

            Console.WriteLine("Press ESCAPE to stop the process.");
            Console.WriteLine(string.Format("RAM used: {0} MB", currentProcess.WorkingSet64 / 1024 / 1024));

            

            for (int i = 1; i <= 200; i++)
            {
                WorkerProcess p = new WorkerProcess("P#" + i.ToString(), 200 + (200 / i * 2), cts.Token, dataLogger, statisticLogger);
                p.Start();
            }

            while (Console.ReadKey(true).Key == ConsoleKey.Escape && !completedByTime)
            {
                Process currentProcessExit = Process.GetCurrentProcess();
                Console.WriteLine("----------------------Terminating Process ESCAPE...");
                Console.WriteLine(string.Format("\r\nRAM used: {0} MB", currentProcessExit.WorkingSet64 / 1024 / 1024));

                cts.Cancel();

                dataLogger.Dispose();
                statisticLogger.Dispose();
            }
            Console.WriteLine("Press ANY KEY");
            Console.ReadKey();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            dataLogger.Dispose();
            statisticLogger.Dispose();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (timeToBeCompleted > DateTime.Now) return;

            Process currentProcessExit = Process.GetCurrentProcess();
            Console.WriteLine("----------------------Terminating Process by time...");
            Console.WriteLine(string.Format("\r\nRAM used: {0} MB", currentProcessExit.WorkingSet64 / 1024 / 1024));
            cts.Cancel();
            aTimer.Enabled = false;
            completedByTime = true;

            dataLogger.Dispose();
            statisticLogger.Dispose();
        }
    }
}
