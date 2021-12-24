using System;
using System.IO;
using Topshelf;
using Microsoft.CSharp.RuntimeBinder;

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

                x.SetServiceName(@"ICP61850-ICMP");
                x.SetDisplayName(@"ICMP-Service");
                x.SetDescription(@"ICMP Service as IEC61850 Server is Listening on Port 10103. The Project is for Heshun by ICPSI.");



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
