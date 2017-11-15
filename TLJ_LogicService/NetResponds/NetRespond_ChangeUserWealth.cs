using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_ChangeUserWealth
{
    public static string doAskCilentReq_ChangeUserWealth(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();

            string account = jo.GetValue("account").ToString();
            string password = jo.GetValue("password").ToString();

            string uid = jo.GetValue("uid").ToString();
            int reward_id = (int)jo.GetValue("reward_id");
            int reward_num = (int)jo.GetValue("reward_num");

            Request_ChangeUserWealth.doRequest(connId,account, password, uid, reward_id, reward_num);
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