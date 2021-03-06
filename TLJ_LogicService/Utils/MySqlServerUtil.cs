﻿using HPSocketCS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TLJCommon;

public class MySqlServerUtil
{
    TcpPackClient m_tcpClient = new TcpPackClient();

    bool m_isConnecting = false;

    public MySqlServerUtil()
    {
        // 设置client事件
        m_tcpClient.OnPrepareConnect += new TcpClientEvent.OnPrepareConnectEventHandler(OnPrepareConnect);
        m_tcpClient.OnConnect += new TcpClientEvent.OnConnectEventHandler(OnConnect);
        m_tcpClient.OnSend += new TcpClientEvent.OnSendEventHandler(OnSend);
        m_tcpClient.OnReceive += new TcpClientEvent.OnReceiveEventHandler(OnReceive);
        m_tcpClient.OnClose += new TcpClientEvent.OnCloseEventHandler(OnClose);

        // 设置包头标识,与对端设置保证一致性
        m_tcpClient.PackHeaderFlag = 0xff;
        // 设置最大封包大小
        m_tcpClient.MaxPackSize = TLJCommon.Consts.MaxPackSize;
    }

    public void start()
    {
        //Thread thread = new Thread(connectinInThread);
        //thread.Start();

        Task t = new Task(() => { connectinInThread(); });
        t.Start();
    }

    void connectinInThread()
    {
        while (!m_tcpClient.Connect(NetConfig.s_mySqlService_ip, (ushort)NetConfig.s_mySqlService_port, false))
        {
            // 连接数据库服务器失败的话会一直尝试连接
            LogUtil.getInstance().addDebugLog("连接数据库服务器失败");
            Thread.Sleep(1000);
        }

        m_isConnecting = true;
        LogUtil.getInstance().addDebugLog("连接数据库服务器成功");

        {
            // 数据清空
            {
                GetTurntableDataScript.getInstance().clear();
            }

            {
                // 获取转盘数据
                Request_GetTurntable.doRequest();
            }
        }

        return;
    }

    public void stop()
    {
        if (m_tcpClient.Stop())
        {
            LogUtil.getInstance().writeLogToLocalNow("与数据库服务器断开成功");
            m_isConnecting = false;
        }
        else
        {
            LogUtil.getInstance().writeLogToLocalNow("与数据库服务器断开失败");
        }
    }

    public bool sendMseeage(string str)
    {
        if (!m_isConnecting)
        {
            return false;
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            // 发送
            if (m_tcpClient.Send(bytes, bytes.Length))
            {
                LogUtil.getInstance().addDebugLog("发送给数据库服务器成功:" + str);
                return true;
            }
            else
            {
                LogUtil.getInstance().addDebugLog("发送给数据库服务器失败:" + str);
                return false;
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog("发送给数据库服务器异常:" + ex.Message);
            return false;
        }
    }

    HandleResult OnPrepareConnect(TcpClient sender, IntPtr socket)
    {
        return HandleResult.Ok;
    }

    HandleResult OnConnect(TcpClient sender)
    {
        return HandleResult.Ok;
    }

    HandleResult OnSend(TcpClient sender, byte[] bytes)
    {
        //string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        //LogUtil.getInstance().addDebugLog("发送给数据库服务器:" + str);

        return HandleResult.Ok;
    }

    HandleResult OnReceive(TcpClient sender, byte[] bytes)
    {
        try
        {
            string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            LogUtil.getInstance().addDebugLog("收到数据库服务器消息:" + str);

            JObject jo = JObject.Parse(str);
            string tag = jo.GetValue("tag").ToString();
            int connId = Convert.ToInt32(jo.GetValue("connId"));

            // 请求签到数据接口
            if (tag.CompareTo(Consts.Tag_GetSignRecord) == 0)
            {
                NetRespond_GetSignRecord.onMySqlRespond(connId, str);
            }
            // 签到接口
            else if (tag.CompareTo(Consts.Tag_Sign) == 0)
            {
                NetRespond_Sign.onMySqlRespond(connId, str);
            }
            // 获取用户信息接口
            else if (tag.CompareTo(Consts.Tag_UserInfo) == 0)
            {
                NetRespond_UserInfo.onMySqlRespond(connId, str);
            }
            // 获取用户邮箱数据
            else if (tag.CompareTo(Consts.Tag_GetMail) == 0)
            {
                NetRespond_GetMail.onMySqlRespond(connId, str);
            }
            // 阅读邮件
            else if (tag.CompareTo(Consts.Tag_ReadMail) == 0)
            {
                NetRespond_ReadMail.onMySqlRespond(connId, str);
            }
            // 删除邮件
            else if (tag.CompareTo(Consts.Tag_DeleteMail) == 0)
            {
                NetRespond_DeleteMail.onMySqlRespond(connId, str);
            }
            // 一键读取所有邮件
            else if (tag.CompareTo(Consts.Tag_OneKeyReadMail) == 0)
            {
                NetRespond_OneKeyReadMail.onMySqlRespond(connId, str);
            }
            // 一键删除所有邮件
            else if (tag.CompareTo(Consts.Tag_OneKeyDeleteMail) == 0)
            {
                NetRespond_OneKeyDeleteMail.onMySqlRespond(connId, str);
            }
            // 获取背包数据
            else if (tag.CompareTo(Consts.Tag_GetBag) == 0)
            {
                NetRespond_GetBag.onMySqlRespond(connId, str);
            }
            // 使用道具
            else if (tag.CompareTo(Consts.Tag_UseProp) == 0)
            {
                NetRespond_UseProp.onMySqlRespond(connId, str);
            }
            // 获取公告活动数据
            else if (tag.CompareTo(Consts.Tag_GetNotice) == 0)
            {
                NetRespond_GetNotice.onMySqlRespond(connId, str);
            }
            // 阅读公告活动
            else if (tag.CompareTo(Consts.Tag_ReadNotice) == 0)
            {
                NetRespond_ReadNotice.onMySqlRespond(connId, str);
            }
            // 获取商店数据
            else if (tag.CompareTo(Consts.Tag_GetShop) == 0)
            {
                NetRespond_GetShop.onMySqlRespond(connId, str);
            }
            // 购买物品
            else if (tag.CompareTo(Consts.Tag_BuyGoods) == 0)
            {
                NetRespond_BuyGoods.onMySqlRespond(connId, str);
            }
            // 获取任务数据
            else if (tag.CompareTo(Consts.Tag_GetTask) == 0)
            {
                NetRespond_GetTask.onMySqlRespond(connId, str);
            }
            // 完成任务
            else if (tag.CompareTo(Consts.Tag_CompleteTask) == 0)
            {
                NetRespond_CompleteTask.onMySqlRespond(connId, str);
            }
            // 使用喇叭
            else if (tag.CompareTo(Consts.Tag_UseLaBa) == 0)
            {
                NetRespond_UseLaBa.onMySqlRespond(connId, str);
            }
            // 兑换话费
            else if (tag.CompareTo(Consts.Tag_UseHuaFei) == 0)
            {
                NetRespond_UseHuaFei.onMySqlRespond(connId, str);
            }
            // 实名认证
            else if (tag.CompareTo(Consts.Tag_RealName) == 0)
            {
                NetRespond_RealName.onMySqlRespond(connId, str);
            }
            // 发送验证码
            else if (tag.CompareTo(Consts.Tag_SendSMS) == 0)
            {
                NetRespond_SendSMS.onMySqlRespond(connId, str);
            }
            // 校验验证码
            else if (tag.CompareTo(Consts.Tag_CheckSMS) == 0)
            {
                NetRespond_CheckSMS.onMySqlRespond(connId, str);
            }
            // 获取pvp场次数据
            else if (tag.CompareTo(Consts.Tag_GetPVPGameRoom) == 0)
            {
                NetRespond_GetPVPGameRoom.onMySqlRespond(connId, str);
            }
            // 获取排行榜数据
            else if (tag.CompareTo(Consts.Tag_GetRank) == 0)
            {
                NetRespond_GetRank.onMySqlRespond(connId, str);
            }
            // 改变玩家财富
            else if (tag.CompareTo(Consts.Tag_ChangeUserWealth) == 0)
            {
                NetRespond_ChangeUserWealth.onMySqlRespond(connId, str);
            }
            // 微信公众号获取玩家数据
            else if (tag.CompareTo(Consts.Tag_WeChat_UserInfo) == 0)
            {
                NetRespond_WeChat_UserInfo.onMySqlRespond(connId, str);
            }
            // 购买元宝
            else if (tag.CompareTo(Consts.Tag_BuyYuanBao) == 0)
            {
                NetRespond_BuyYuanBao.onMySqlRespond(connId, str);
            }
            // 设置二级密码（徽章密码）
            else if (tag.CompareTo(Consts.Tag_SetSecondPSW) == 0)
            {
                NetRespond_SetSecondPSW.onMySqlRespond(connId, str);
            }
            // 获取转盘数据
            else if (tag.CompareTo(Consts.Tag_GetTurntable) == 0)
            {
                Request_GetTurntable.onMySqlRespond(str);
            }
            // 使用转盘
            else if (tag.CompareTo(Consts.Tag_UseTurntable) == 0)
            {
                NetRespond_UseTurntable.onMySqlRespond(connId, str);
            }
            // 校验徽章密码(与Login接口共用)
            else if (tag.CompareTo(Consts.Tag_Login) == 0)
            {
                NetRespond_CheckSecondPSW.onMySqlRespond(connId, str);
            }
            // 修改头像
            else if (tag.CompareTo(Consts.Tag_ChangeHead) == 0)
            {
                NetRespond_ChangeHead.onMySqlRespond(connId, str);
            }
            // 发放救济金
            else if (tag.CompareTo(Consts.Tag_SupplyGold) == 0)
            {
                NetRespond_SupplyGold.onMySqlRespond(str);
            }
            // ios支付
            else if (tag.CompareTo(Consts.Tag_IOS_Pay) == 0)
            {
                NetRespond_IOS_Pay.onMySqlRespond(connId, str);
            }
            // 老用户绑定
            else if (tag.CompareTo(Consts.Tag_OldPlayerBind) == 0)
            {
                NetRespond_OldPlayerBind.onMySqlRespond(connId, str);
            }
            // 获取30天签到数据
            else if (tag.CompareTo(Consts.Tag_GetSignReward_30) == 0)
            {
                NetRespond_GetSignReward_30.onMySqlRespond(connId, str);
            }
            // 获取30天签到记录
            else if (tag.CompareTo(Consts.Tag_GetSignRecord_30) == 0)
            {
                NetRespond_GetSignRecord_30.onMySqlRespond(connId, str);
            }
            // 请求签到（月签的那种）
            else if (tag.CompareTo(Consts.Tag_Sign_30) == 0)
            {
                NetRespond_Sign_30.onMySqlRespond(connId, str);
            }
            else
            {
                LogUtil.getInstance().addDebugLog("未知Tag：" + tag);
            }

            return HandleResult.Ok;
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog("MySqlServerUtil.OnReceive异常:" + ex.Message);
        }

        return HandleResult.Ok;
    }

    HandleResult OnClose(TcpClient sender, SocketOperation enOperation, int errorCode)
    {
        LogUtil.getInstance().addDebugLog("与数据库服务器断开");

        m_isConnecting = false;

        // 尝试重新连接数据库服务器
        start();

        return HandleResult.Ok;
    }
}
