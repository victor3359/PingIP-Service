using System;
using System.IO;
using Topshelf;

namespace PingIP_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                x.Service<PingIP>(s =>
                {
                    s.ConstructUsing(PingIP => new PingIP());
                    s.WhenStarted(PingIP => PingIP.Start());
                    s.WhenStopped(PingIP => PingIP.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName(@"ICP61850");
                x.SetDisplayName(@"ICP61850-Service");
                x.SetDescription(@"Iec61850 Server is Listening on Port 10102. The Project is for Yantian by ICPSI.");



                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(TimeSpan.FromSeconds(5));
                    r.RestartService(TimeSpan.FromSeconds(5));
                    r.RestartComputer(TimeSpan.FromSeconds(30), @"Computer is restarting...");
                    r.SetResetPeriod(7);
                    r.OnCrashOnly();
                });
            });
        }
    }
}
