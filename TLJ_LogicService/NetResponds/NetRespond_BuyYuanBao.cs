using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_BuyYuanBao
{
    public static string doAskCilentReq_BuyYuanBao(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();

            respondJO = jo;
            respondJO.Add("connId", connId.ToInt32());

            // 传给数据库服务器
            {
                if (!LogicService.m_mySqlServerUtil.sendMseeage(respondJO.ToString()))
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
            JObject jo = JObject.Parse(respondData);
            string uid = jo.GetValue("uid").ToString();

            // 发送给web服务器
            LogicService.m_serverUtil.sendMessage((IntPtr)connId, respondData);

            // 发送给玩家
            LogicService.m_serverUtil.sendMessage(ClientManager.getInstance().getClientByUID(uid).m_connId, respondData);
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog(ex.Message);
        }
    }
}