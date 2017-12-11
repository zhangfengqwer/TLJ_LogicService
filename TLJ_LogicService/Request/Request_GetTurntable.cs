using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class Request_GetTurntable
{
    public static void doRequest()
    {
        try
        {
            JObject respondJO = new JObject();

            respondJO.Add("tag", TLJCommon.Consts.Tag_GetTurntable);

            // 传给数据库服务器
            {
                if (!LogicService.m_mySqlServerUtil.sendMseeage(respondJO.ToString()))
                {
                    // 连接不上数据库服务器
                }
            }
        }
        catch (Exception ex)
        {
            // 客户端参数错误
            LogUtil.getInstance().addErrorLog("Request_GetTurntable.doRequest----" + ex.Message);
        }
    }

    public static void onMySqlRespond(string respondData)
    {
        try
        {
            GetTurntableDataScript.getInstance().initJson(respondData);
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addErrorLog("Request_GetTurntable.onMySqlRespond----" + ex.Message);

            // 客户端参数错误
            //respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_ParamError));

            // 发送给客户端
            //LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
        }
    }
}