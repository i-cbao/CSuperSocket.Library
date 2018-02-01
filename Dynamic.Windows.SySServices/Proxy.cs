using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Windows.SySServices
{
    class Proxy
    {

        public string FileName { get; protected set;  }
        public string Arguments { get; protected set; }

        public Process Process { get; protected set; }

        public bool IsExited { get; protected set; }

        public bool UseExitCommand { get; protected set; }

        private int retryCount = 0;

        public Proxy(string cmd, string args, bool useExitCommand)
        {
            FileName = cmd;
            Arguments = args;
            UseExitCommand = useExitCommand;
        }

        private System.Threading.ManualResetEvent waitExit = null;
        public void Exit()
        {
            IsExited = true;
            if(Process!=null)
            {
                if (UseExitCommand)
                {
                    waitExit = new System.Threading.ManualResetEvent(false);
                    Process.StandardInput.WriteLine("exit");
                    waitExit.WaitOne(10000);
                }else
                {
                    //退出
                    Process.Kill();

                }
                Process = null;
            }
        }

        public void WriteCommand(string command)
        {
            if(Process !=null && !Process.HasExited)
            {
                Process.StandardInput.WriteLine(command);
            }
        }

        public bool Start()
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = false,
                FileName = FileName,
                Arguments = Arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = System.IO.Path.GetDirectoryName(FileName),
                RedirectStandardOutput =true,
                RedirectStandardInput =true,
                UseShellExecute =false
            };
            p.EnableRaisingEvents = true;
            p.Exited += P_Exited;
            
            Process = p;
            bool isOk = p.Start();
            if(isOk)
            {
                IsExited = false;
                p.OutputDataReceived += P_OutputDataReceived;
                p.BeginOutputReadLine();
            }
          //  Logger.Debug("启动进程：{0} {1}", FileName ?? "", Arguments ?? "");
            return isOk;

        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            System.Console.WriteLine(e.Data);
        }

        private void P_Exited(object sender, EventArgs e)
        {
         //   Logger.Error("进程已经退出：{0}", FileName ?? "");

            if (!IsExited && Process.ExitCode !=0)
            {
                retryCount++;
                Start();
            }else
            {
                if (waitExit!=null)
                {
                    waitExit.Set();
                }
            }
        }
    }
}
