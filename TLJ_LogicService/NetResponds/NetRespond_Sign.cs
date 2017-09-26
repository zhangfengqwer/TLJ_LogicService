using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TLJ_LogicService;

class NetRespond_Sign
{
    public static string doAskCilentReq_Sign(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();
            respondJO.Add("tag", tag);

            JObject temp = new JObject();
            temp.Add("tag", tag);
            temp.Add("connId", connId.ToInt32());
            temp.Add("uid", uid);
            //发送给数据库服务器
            if (!LogicService.m_mySqlServerUtil.sendMseeage(temp.ToString()))
            {
                // 连接不上数据库服务器，通知客户端
                respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_MySqlError));
                // 发送给客户端
                LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
            }
        }
        catch (Exception e)
        {
            // 客户端参数错误
            respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_ParamError));

            // 发送给客户端
            LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
        }

        //return respondJO.ToString();
        return "";
    }

    public static void onMySqlRespond(int connId, string respondData)
    {
        // 发送给客户端
        LogicService.m_serverUtil.sendMessage((IntPtr)connId, respondData);
    }
}
