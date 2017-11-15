using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_CheckRepeatLogin
{
    public static string doAskCilentReq_CheckRepeatLogin(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();

            {
                try
                {
                    ClientData temp = ClientManager.getInstance().getClientByUID(uid);
                    if (temp != null)
                    {
                        JObject jo2 = new JObject();
                        jo2.Add("tag", TLJCommon.Consts.Tag_ForceOffline);

                        // 发送给客户端
                        LogicService.m_serverUtil.sendMessage(temp.m_connId, jo2.ToString());

                        ClientManager.getInstance().deleteClientByConnId(temp.m_connId);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        catch (Exception ex)
        {
            // 客户端参数错误
            respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_ParamError));
        }

        //return respondJO.ToString();
        return "";
    }
}