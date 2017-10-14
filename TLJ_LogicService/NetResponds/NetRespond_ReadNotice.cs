using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_ReadNotice
{
    public static string doAskCilentReq_ReadNotice(IntPtr connId, string reqData)
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
                temp.Add("notice_id", (int)jo.GetValue("notice_id"));

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
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog(ex.Message);
        }
    }
}