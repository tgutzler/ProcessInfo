using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProcessToolsLib
{
    public class ProcessToMonitor
    {
        public ProcessToMonitor(string processName)
        {
            _proc = (from p in Process.GetProcessesByName(processName)
                     orderby p.StartTime descending
                     select p).FirstOrDefault();

            if (_proc == null)
            {
                // Maybe they've given us a PID?
                try
                {
                    _proc = Process.GetProcessById(Convert.ToInt32(processName));
                }
                catch (Exception)
                {
                    // leave it as is
                }
            }
        }

        private Process _proc;

        public bool IsValid {
            get { return _proc != null; }
        }

        public ProcessInfo GetInfo()
        {
            return new ProcessInfo(_proc);
        }
    }
    

    [DataContract]
    public class ProcessInfo
    {
        [DllImport("User32")]
        extern public static int GetGuiResources(IntPtr hProcess, int uiFlags);

        private DateTime _timestamp;

        public ProcessInfo(Process process)
        {
            TimeStamp = DateTime.Now;
            ProcessName = process.ProcessName;
            ProcessId = process.Id;
            Responding = (byte)(process.Responding ? 1 : 0);
            ProcessorTimeMs = process.TotalProcessorTime.TotalMilliseconds;
            _timestamp = DateTime.Now;
            WorkingSet = process.WorkingSet64;
            PeakWorkingSet = process.PeakWorkingSet64;
            PrivateBytes = process.PrivateMemorySize64;
            ThreadCount = process.Threads.Count;
            HandleCount = process.HandleCount;
            UserHandles = GetGuiResources(process.Handle, 1);
            Thread.Sleep(200);
            ProcessorUsage = (process.TotalProcessorTime.TotalMilliseconds - ProcessorTimeMs)
                / (DateTime.Now.Subtract(_timestamp).TotalMilliseconds)
                * 100 / Environment.ProcessorCount;
        }

        public int ProcessId { get; private set; }
        [DataMember]
        public DateTime TimeStamp { get; private set; }
        [DataMember]
        public string ProcessName { get; private set; }
        [DataMember]
        public byte Responding { get; private set; }
        [DataMember]
        public double ProcessorTimeMs { get; private set; }
        [DataMember]
        public double ProcessorUsage { get; private set; }
        [DataMember]
        public long WorkingSet { get; private set; }
        [DataMember]
        public long PeakWorkingSet { get; private set; }
        [DataMember]
        public long PrivateBytes { get; private set; }
        [DataMember]
        public int ThreadCount { get; private set; }
        [DataMember]
        public int HandleCount { get; private set; }
        [DataMember]
        public int UserHandles { get; private set; }


        public string ToJSON()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ProcessInfo));
            ser.WriteObject(stream, this);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
