using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_SupplyGold
{
    public static void onMySqlRespond(string respondData)
    {
        try
        {
            JObject jo = JObject.Parse(respondData);
            string uid = jo.GetValue("uid").ToString();

            // 发送给客户端
            {
                ClientData clientData = ClientManager.getInstance().getClientByUID(uid);
                if (clientData != null)
                {
                    LogicService.m_serverUtil.sendMessage(clientData.m_connId, respondData);
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog(ex.Message);
        }
    }
}