﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_UseTurntable
{
    public static string doAskCilentReq_UseTurntable(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();

            respondJO.Add("tag", tag);

            // 传给数据库服务器
            {
                JObject temp = new JObject();
                temp.Add("tag", tag);
                temp.Add("connId", connId.ToInt32());

                temp.Add("uid", uid);
                temp.Add("type", (int)jo.GetValue("type"));

                if (!LogicService.m_mySqlServerUtil.sendMseeage(temp.ToString()))
                {
                    // 连接不上数据库服务器，通知客户端
                    {
                        respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_MySqlError));

                        // 发送给客户端
                        LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 客户端参数错误
            respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_ParamError));

            // 发送给客户端
            LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
        }
        
        return "";
    }

    public static void onMySqlRespond(int connId, string respondData)
    {
        try
        {
            // 发送给客户端
            {
                LogicService.m_serverUtil.sendMessage((IntPtr)connId, respondData);

                // 提交任务
                {
                    JObject jo = JObject.Parse(respondData);
                    int code = (int)jo.GetValue("code");
                    if (code == (int)TLJCommon.Consts.Code.Code_OK)
                    {
                        string uid = jo.GetValue("uid").ToString();
                        Request_ProgressTask.doRequest(uid, 217);
                    }
                }
            }

            // 广播给所有客户端
            {
                JObject jo = JObject.Parse(respondData);
                int code = (int)jo.GetValue("code");
                if (code == (int)TLJCommon.Consts.Code.Code_OK)
                {
                    string uid = (string)jo.GetValue("uid");
                    string name = (string)jo.GetValue("name");
                    int reward_id = (int)jo.GetValue("reward_id");
                    int type = (int)jo.GetValue("type");

                    // 提交任务
                    if (type == 2)
                    {
                        Request_ProgressTask.doRequest(uid, 220);
                    }

                    JObject respondJO = new JObject();
                    respondJO.Add("tag",TLJCommon.Consts.Tag_TurntableBroadcast);
                    respondJO.Add("name", name);
                    respondJO.Add("reward_id", reward_id);
                    respondJO.Add("canShowSelf", 0);
                    
                    for (int i = 0; i < LogicService.m_serverUtil.getAllIntPtr().Count; i++)
                    {
                        LogicService.m_serverUtil.sendMessage(LogicService.m_serverUtil.getAllIntPtr()[i], respondJO.ToString());
                    }

                    GetTurntableDataScript.getInstance().addNoticeContent(new NoticeContent(name, reward_id));
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog(ex.Message);
        }
    }
}