using HPSocketCS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TLJCommon;

public class HPServerUtil
{
    //TcpPackServer m_tcpServer = new TcpPackServer();
    TcpServer m_tcpServer = new TcpServer();

    // 数据包尾部标识
    char m_packEndFlag = (char)1;
    string m_endStr = "";

    int m_onlineCount = 0;
    List<IntPtr> m_allIntPtr = new List<IntPtr>();

    public HPServerUtil()
    {
        // 设置服务器事件
        m_tcpServer.OnPrepareListen += new TcpServerEvent.OnPrepareListenEventHandler(OnPrepareListen);
        m_tcpServer.OnAccept += new TcpServerEvent.OnAcceptEventHandler(OnAccept);
        m_tcpServer.OnSend += new TcpServerEvent.OnSendEventHandler(OnSend);
        // 两个数据到达事件的一种
        //server.OnPointerDataReceive += new TcpServerEvent.OnPointerDataReceiveEventHandler(OnPointerDataReceive);
        // 两个数据到达事件的一种
        m_tcpServer.OnReceive += new TcpServerEvent.OnReceiveEventHandler(OnReceive);

        m_tcpServer.OnClose += new TcpServerEvent.OnCloseEventHandler(OnClose);
        m_tcpServer.OnShutdown += new TcpServerEvent.OnShutdownEventHandler(OnShutdown);

        //// 设置包头标识,与对端设置保证一致性
        //m_tcpServer.PackHeaderFlag = 0xff;
        // 设置最大封包大小
        //m_tcpServer.MaxPackSize = 0x1000;
    }

    public int getOnlineCount()
    {
        return m_onlineCount;
    }

    public List<IntPtr> getAllIntPtr()
    {
        return m_allIntPtr;
    }

    // 启动服务
    public void startTCPService()
    {
        try
        {
            //m_tcpServer.IpAddress = NetConfig.s_logicService_ip;
            m_tcpServer.IpAddress = "0.0.0.0";
            m_tcpServer.Port = (ushort)NetConfig.s_logicService_port;

            // 启动服务
            if (m_tcpServer.Start())
            {
                LogUtil.getInstance().addDebugLog("TCP服务启动成功");
            }
            else
            {
                LogUtil.getInstance().addDebugLog("TCP服务启动失败");
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog("TCP服务启动异常:" + ex.Message);
        }
    }

    // 停止服务
    public void stopTCPService()
    {
        if (m_tcpServer.Stop())
        {
            LogUtil.getInstance().writeLogToLocalNow("TCP服务停止成功");
        }
        else
        {
            LogUtil.getInstance().writeLogToLocalNow("TCP服务停止失败");
        }
    }

    // 发送消息
    public void sendMessage(IntPtr connId, string text)
    {
        // 增加数据包尾部标识
        //byte[] bytes = new byte[1024 * 10];
        //bytes = Encoding.UTF8.GetBytes(text + m_packEndFlag);

        byte[] bytes = Encoding.UTF8.GetBytes(text + m_packEndFlag);

        if (m_tcpServer.Send(connId, bytes, bytes.Length))
        {
            LogUtil.getInstance().addDebugLog("发送消息给客户端：" + text);
        }
        else
        {
            Debug.WriteLine("发送给客户端失败:" + text);
        }
    }

    HandleResult OnPrepareListen(IntPtr soListen)
    {
        return HandleResult.Ok;
    }

    // 客户进入了
    HandleResult OnAccept(IntPtr connId, IntPtr pClient)
    {
        ++m_onlineCount;
        m_allIntPtr.Add(connId);
        ClientManager.getInstance().addClient(connId);

        // 获取客户端ip和端口
        string ip = string.Empty;
        ushort port = 0;
        if (m_tcpServer.GetRemoteAddress(connId, ref ip, ref port))
        {
            LogUtil.getInstance().addDebugLog("有客户端连接--connId=" + (int)connId + "--ip=" + ip.ToString() + "--port=" + port);
        }
        else
        {
            LogUtil.getInstance().addDebugLog("获取客户端ip地址出错");
        }

        return HandleResult.Ok;
    }

    HandleResult OnSend(IntPtr connId, byte[] bytes)
    {
        //string text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

        //LogUtil.getInstance().addDebugLog("发送消息给客户端：" + text);

        return HandleResult.Ok;
    }


    //HandleResult OnPointerDataReceive(IntPtr connId, IntPtr pData, int length)
    //{
    //    // 数据到达了
    //    try
    //    {
    //        if (m_tcpServer.Send(connId, pData, length))
    //        {
    //            return HandleResult.Ok;
    //        }

    //        return HandleResult.Error;
    //    }
    //    catch (Exception)
    //    {
    //        return HandleResult.Ignore;
    //    }
    //}

    HandleResult OnReceive(IntPtr connId, byte[] bytes)
    {
        try
        {
            string text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            LogUtil.getInstance().addDebugLog("收到客户端原始消息：" + text);

            {
                text = m_endStr + text;
                text = text.Replace("\r\n", "");

                List<string> list = new List<string>();
                bool b = CommonUtil.splitStrIsPerfect(text, list, m_packEndFlag);

                if (b)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        ReceiveObj obj = new ReceiveObj(connId, list[i]);
                        //Thread thread = new Thread(doAskCilentReq);
                        //thread.Start(obj);

                        Task t = new Task(() => { doAskCilentReq(obj); });
                        t.Start();
                    }

                    //text = "";
                    m_endStr = "";
                }
                else
                {
                    for (int i = 0; i < list.Count - 1; i++)
                    {
                        ReceiveObj obj = new ReceiveObj(connId, list[i]);
                        //Thread thread = new Thread(doAskCilentReq);
                        //thread.Start(obj);

                        Task t = new Task(() => { doAskCilentReq(obj); });
                        t.Start();
                    }

                    m_endStr = list[list.Count - 1];
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addErrorLog("OnReceive:" + ex.Message);
        }

        return HandleResult.Ok;
    }

    HandleResult OnClose(IntPtr connId, SocketOperation enOperation, int errorCode)
    {
        if (m_onlineCount > 0)
        {
            --m_onlineCount;
            m_allIntPtr.Remove(connId);
        }

        ClientManager.getInstance().deleteClientByConnId(connId);

        LogUtil.getInstance().addDebugLog("与客户端断开:" + connId);

        return HandleResult.Ok;
    }

    // 服务关闭
    HandleResult OnShutdown()
    {
        //调用关闭的地方已经记录日志了，这里不需要重复记录
        //Debug.WriteLine("OnShutdown");
        //LogUtil.getInstance().addDebugLog("TCP服务关闭成功");

        return HandleResult.Ok;
    }

    // 处理客户端的请求
    void doAskCilentReq(object obj)
    {
        try
        {
            // 模拟耗时操作，比如数据库操作，IO操作
            // Thread.Sleep(3000);

            ReceiveObj receiveObj = (ReceiveObj)obj;
            string text = receiveObj.m_data;

            LogUtil.getInstance().addDebugLog("收到客户端消息：" + text);

            JObject jo;
            try
            {
                jo = JObject.Parse(text);
            }
            catch (JsonReaderException ex)
            {
                // 传过来的数据不是json格式的，一律不理
                LogUtil.getInstance().addDebugLog("客户端传来非json数据：" + text);

                m_endStr = "";

                return;
            }

            if (jo.GetValue("tag") != null)
            {
                string tag = jo.GetValue("tag").ToString();

                // 请求签到数据
                if (tag.CompareTo(Consts.Tag_GetSignRecord) == 0)
                {
                    NetRespond_GetSignRecord.doAskCilentReq_GetSignRecord(receiveObj.m_connId, text);
                }
                //签到
                else if (tag.CompareTo(Consts.Tag_Sign) == 0)
                {
                    NetRespond_Sign.doAskCilentReq_Sign(receiveObj.m_connId, text);
                }
                // 请求服务器在线玩家信息
                else if (tag.CompareTo(Consts.Tag_OnlineInfo) == 0)
                {
                    NetRespond_OnlineInfo.doAskCilentReq_OnlineInfo(receiveObj.m_connId, text);
                }
                // 获取用户信息
                else if (tag.CompareTo(Consts.Tag_UserInfo) == 0)
                {
                    NetRespond_UserInfo.doAskCilentReq_UserInfo(receiveObj.m_connId, text);
                }
                // 获取用户邮箱数据
                else if (tag.CompareTo(Consts.Tag_GetMail) == 0)
                {
                    NetRespond_GetMail.doAskCilentReq_GetMail(receiveObj.m_connId, text);
                }
                // 阅读邮件
                else if (tag.CompareTo(Consts.Tag_ReadMail) == 0)
                {
                    NetRespond_ReadMail.doAskCilentReq_ReadMail(receiveObj.m_connId, text);
                }
                // 删除邮件
                else if (tag.CompareTo(Consts.Tag_DeleteMail) == 0)
                {
                    NetRespond_DeleteMail.doAskCilentReq_DeleteMail(receiveObj.m_connId, text);
                }
                // 一键读取所有邮件
                else if (tag.CompareTo(Consts.Tag_OneKeyReadMail) == 0)
                {
                    NetRespond_OneKeyReadMail.doAskCilentReq_OneKeyReadMail(receiveObj.m_connId, text);
                }
                // 一键删除所有邮件
                else if (tag.CompareTo(Consts.Tag_OneKeyDeleteMail) == 0)
                {
                    NetRespond_OneKeyDeleteMail.doAskCilentReq_OneKeyDeleteMail(receiveObj.m_connId, text);
                }
                // 获取背包数据
                else if (tag.CompareTo(Consts.Tag_GetBag) == 0)
                {
                    NetRespond_GetBag.doAskCilentReq_GetBag(receiveObj.m_connId, text);
                }
                // 使用道具
                else if (tag.CompareTo(Consts.Tag_UseProp) == 0)
                {
                    NetRespond_UseProp.doAskCilentReq_UseProp(receiveObj.m_connId, text);
                }
                // 获取公告活动数据
                else if (tag.CompareTo(Consts.Tag_GetNotice) == 0)
                {
                    NetRespond_GetNotice.doAskCilentReq_GetNotice(receiveObj.m_connId, text);
                }
                // 阅读公告活动
                else if (tag.CompareTo(Consts.Tag_ReadNotice) == 0)
                {
                    NetRespond_ReadNotice.doAskCilentReq_ReadNotice(receiveObj.m_connId, text);
                }
                // 获取商店数据
                else if (tag.CompareTo(Consts.Tag_GetShop) == 0)
                {
                    NetRespond_GetShop.doAskCilentReq_GetShop(receiveObj.m_connId, text);
                }
                // 购买物品
                else if (tag.CompareTo(Consts.Tag_BuyGoods) == 0)
                {
                    NetRespond_BuyGoods.doAskCilentReq_BuyGoods(receiveObj.m_connId, text);
                }
                // 获取任务数据
                else if (tag.CompareTo(Consts.Tag_GetTask) == 0)
                {
                    NetRespond_GetTask.doAskCilentReq_GetTask(receiveObj.m_connId, text);
                }
                // 完成任务
                else if (tag.CompareTo(Consts.Tag_CompleteTask) == 0)
                {
                    NetRespond_CompleteTask.doAskCilentReq_CompleteTask(receiveObj.m_connId, text);
                }
                // 使用喇叭
                else if (tag.CompareTo(Consts.Tag_UseLaBa) == 0)
                {
                    NetRespond_UseLaBa.doAskCilentReq_UseLaBa(receiveObj.m_connId, text);
                }
                // 兑换话费
                else if (tag.CompareTo(Consts.Tag_UseHuaFei) == 0)
                {
                    NetRespond_UseHuaFei.doAskCilentReq_UseHuaFei(receiveObj.m_connId, text);
                }
                // 实名认证
                else if (tag.CompareTo(Consts.Tag_RealName) == 0)
                {
                    NetRespond_RealName.doAskCilentReq_RealName(receiveObj.m_connId, text);
                }
                // 发送验证码
                else if (tag.CompareTo(Consts.Tag_SendSMS) == 0)
                {
                    NetRespond_SendSMS.doAskCilentReq_SendSMS(receiveObj.m_connId, text);
                }
                // 校验验证码
                else if (tag.CompareTo(Consts.Tag_CheckSMS) == 0)
                {
                    NetRespond_CheckSMS.doAskCilentReq_CheckSMS(receiveObj.m_connId, text);
                }
                // 获取pvp场次数据
                else if (tag.CompareTo(Consts.Tag_GetPVPGameRoom) == 0)
                {
                    NetRespond_GetPVPGameRoom.doAskCilentReq_GetPVPGameRoom(receiveObj.m_connId, text);
                }
                // 获取排行榜数据
                else if (tag.CompareTo(Consts.Tag_GetRank) == 0)
                {
                    NetRespond_GetRank.doAskCilentReq_GetRank(receiveObj.m_connId, text);
                }
                // 检查是否有人已经登录这个账号
                else if (tag.CompareTo(Consts.Tag_CheckRepeatLogin) == 0)
                {
                    NetRespond_CheckRepeatLogin.doAskCilentReq_CheckRepeatLogin(receiveObj.m_connId, text);
                }
                // 改变玩家财富
                else if (tag.CompareTo(Consts.Tag_ChangeUserWealth) == 0)
                {
                    NetRespond_ChangeUserWealth.doAskCilentReq_ChangeUserWealth(receiveObj.m_connId, text);
                }
                // 微信公众号获取玩家数据
                else if (tag.CompareTo(Consts.Tag_WeChat_UserInfo) == 0)
                {
                    NetRespond_WeChat_UserInfo.doAskCilentReq_WeChat_UserInfo(receiveObj.m_connId, text);
                }
                // 购买元宝
                else if (tag.CompareTo(Consts.Tag_BuyYuanBao) == 0)
                {
                    NetRespond_BuyYuanBao.doAskCilentReq_BuyYuanBao(receiveObj.m_connId, text);
                }
                // 设置二级密码（徽章密码）
                else if (tag.CompareTo(Consts.Tag_SetSecondPSW) == 0)
                {
                    NetRespond_SetSecondPSW.doAskCilentReq_SetSecondPSW(receiveObj.m_connId, text);
                }
                // 获取转盘数据
                else if (tag.CompareTo(Consts.Tag_GetTurntable) == 0)
                {
                    NetRespond_GetTurntable.doAskCilentReq_GetTurntable(receiveObj.m_connId, text);
                }
                // 使用转盘
                else if (tag.CompareTo(Consts.Tag_UseTurntable) == 0)
                {
                    NetRespond_UseTurntable.doAskCilentReq_UseTurntable(receiveObj.m_connId, text);
                }
                // 校验徽章密码(与Login接口共用)
                else if (tag.CompareTo(Consts.Tag_Login) == 0)
                {
                    NetRespond_CheckSecondPSW.doAskCilentReq_CheckSecondPSW(receiveObj.m_connId, text);
                }
                // 客户端提交完成任务id
                else if (tag.CompareTo(Consts.Tag_ProgressTask) == 0)
                {
                    NetRespond_ProgressTask.doAskCilentReq_ProgressTask(receiveObj.m_connId, text);
                }
                // 修改头像
                else if (tag.CompareTo(Consts.Tag_ChangeHead) == 0)
                {
                    NetRespond_ChangeHead.doAskCilentReq_ChangeHead(receiveObj.m_connId, text);
                }
                else
                {
                    LogUtil.getInstance().addDebugLog("未知Tag：" + tag);
                }
            }
            else
            {
                // 传过来的数据没有tag字段的，一律不理
                LogUtil.getInstance().addDebugLog("客户端传来的数据没有Tag：" + text);
                return;
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addErrorLog("doAskCilentReq:" + ex.Message);
        }
    }
}

class ReceiveObj
{
    public IntPtr m_connId;
    public string m_data = "";

    public ReceiveObj(IntPtr connId, string data)
    {
        m_connId = connId;
        m_data = data;
    }
};
