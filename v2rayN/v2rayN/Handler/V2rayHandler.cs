﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using v2rayN.Mode;

namespace v2rayN.Handler
{

    /// <summary>
    /// 消息委托
    /// </summary>
    /// <param name="notify">是否显示在托盘区</param>
    /// <param name="msg">内容</param>
    public delegate void ProcessDelegate(bool notify, string msg);


    /// <summary>
    /// v2ray进程处理类
    /// </summary>
    class V2rayHandler
    {
        private static string v2rayConfigRes = Global.v2rayConfigFileName;
        private List<string> lstV2ray;
        public event ProcessDelegate ProcessEvent;

        public V2rayHandler()
        {
            lstV2ray = new List<string>();
            lstV2ray.Add("wv2ray");
            lstV2ray.Add("v2ray");
        }

        /// <summary>
        /// 载入V2ray
        /// </summary>
        public void LoadV2ray(Config config)
        {
            if (Global.reloadV2ray)
            {
                string msg = string.Empty;
                string fileName = Utils.GetPath(v2rayConfigRes);
                if (V2rayConfigHandler.GenerateClientConfig(config, fileName, out msg) != 0)
                {
                    ShowMsg(false, msg);
                }
                else
                {
                    ShowMsg(true, msg);
                    V2rayRestart();
                }
            }
        }

        /// <summary>
        /// V2ray重启
        /// </summary>
        private void V2rayRestart()
        {
            V2rayStop();
            V2rayStart();
        }

        /// <summary>
        /// V2ray停止
        /// </summary>
        public void V2rayStop()
        {
            try
            {
                foreach (string vName in lstV2ray)
                {
                    Process[] killPro = Process.GetProcessesByName(vName);
                    foreach (Process p in killPro)
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// V2ray启动
        /// </summary>
        private void V2rayStart()
        {
            ShowMsg(false, string.Format("启动服务({0})......", DateTime.Now.ToString()));

            try
            {
                //查找v2ray文件是否存在
                string fileName = string.Empty;
                for (int k = 0; k < lstV2ray.Count; k++)
                {
                    string vName = string.Format("{0}.exe", lstV2ray[k]);
                    vName = Utils.GetPath(vName);
                    if (File.Exists(vName))
                    {
                        fileName = vName;
                        break;
                    }
                }
                if (Utils.IsNullOrEmpty(fileName))
                {
                    string msg = "未找到v2ray文件";
                    ShowMsg(true, msg);
                    return;
                }

                Process p = new Process();
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        string msg = e.Data + Environment.NewLine;
                        //this.AppendText(e.Data + Environment.NewLine);
                        ShowMsg(false, msg);
                    }
                });
                p.Start();
                p.BeginOutputReadLine();
            }
            catch (Exception)
            {
                string msg = "未找到v2ray文件...";
                ShowMsg(true, msg);
            }
        }
        /// <summary>
        /// 消息委托
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="msg"></param>
        private void ShowMsg(bool notify, string msg)
        {
            if (ProcessEvent != null)
            {
                ProcessEvent(notify, msg);
            }
        }
    }
}
