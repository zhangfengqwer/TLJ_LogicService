using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace TLJ_LogicService
{
    public partial class LogicService : ServiceBase
    {
        public static HPServerUtil m_serverUtil;
        public static MySqlServerUtil m_mySqlServerUtil;

        public LogicService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!OtherConfig.init())
            {
                return;
            }

            LogUtil.getInstance().start(OtherConfig.s_logPath + "TLJ_LogicServiceLog");

            if (!NetConfig.init())
            {
                return;
            }

            m_serverUtil = new HPServerUtil();
            m_mySqlServerUtil = new MySqlServerUtil();
            LogUtil.getInstance().addDebugLog("服务开启");

            // 开启TCP服务组件与客户端通信
            m_serverUtil.startTCPService();

            // 开启TCP客户端组件与数据库服务器通信
            m_mySqlServerUtil.start();
        }

        protected override void OnStop()
        {
            m_serverUtil.stopTCPService();
            m_mySqlServerUtil.stop();

            LogUtil.getInstance().writeLogToLocalNow("服务关闭");
        }
    }
}
