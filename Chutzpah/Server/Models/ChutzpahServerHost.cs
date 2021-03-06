﻿using Nancy.Hosting.Self;
using System;
using System.Threading;

namespace Chutzpah.Server.Models
{
    public interface IChutzpahWebServerHost : IDisposable
    {
        string RootPath { get; set; }

        int Port { get; set; }
    }

    public class ChutzpahWebServerHost : IChutzpahWebServerHost
    {
        static ChutzpahWebServerHost activeWebServer;

        public static ChutzpahWebServerHost Create(NancyHost nancyHost, string rootPath, int port)
        {
            var host = new ChutzpahWebServerHost(nancyHost, rootPath, port);
            ActiveWebServer = host;
            return host;
        }

        public static ChutzpahWebServerHost ActiveWebServer
        {
            get
            {
                return activeWebServer;
            }

            set
            {

                ChutzpahTracer.TraceInformation("Active server set to {0}", value.ToString());
                Interlocked.Exchange(ref activeWebServer, value);
            }
        }



        public NancyHost NancyHost { get; set; }
        public string RootPath { get; set; }

        public int Port { get; set; }

        private ChutzpahWebServerHost(NancyHost nancyHost, string rootPath, int port)
        {
            Port = port;
            RootPath = rootPath;
            NancyHost = nancyHost;
        }

        public void Dispose()
        {
            try
            {
                ChutzpahTracer.TraceInformation("Tearing down Web Server Host at path {0} and port {1}", RootPath, Port);
                NancyHost.Dispose();

            }
            catch (Exception e)
            {
                ChutzpahTracer.TraceError(e, "Error tearing down Web Server Host at path {0} and port {1}", RootPath, Port);
            }
            finally
            {
                // Set active server to null
                Interlocked.Exchange(ref activeWebServer, null);
            }

        }

        public override string ToString()
        {
            return string.Format("ChutzpahServerHost - Port: {0}, RootPath: {1}", Port, RootPath);
        }
    }
}
