using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;


class NetRespond_UserInfo
{
    public static string doAskCilentReq_UserInfo(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();

            respondJO.Add("tag", tag);
            

            //{
            //    respondJO.Add("code", (int)TLJCommon.Consts.Code.Code_OK);
            //    respondJO.Add("uid", uid);
            //    respondJO.Add("name", "name");
            //    respondJO.Add("gold", 10000);
            //    respondJO.Add("yuanbao", 100);
            //    respondJO.Add("phone", "");

            //    // 发送给客户端
            //    {
            //        LogicService.m_serverUtil.sendMessage((IntPtr)connId, respondJO.ToString());
            //    }
            //}

            // 传给数据库服务器
            {
                JObject temp = new JObject();
                temp.Add("tag", tag);
                temp.Add("connId", connId.ToInt32());

                temp.Add("uid", uid);

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

        //return respondJO.ToString();
        return "";
    }

    public static void onMySqlRespond(int connId, string respondData)
    {
        JObject respondJO = new JObject();
        try
        {
            JObject jo = JObject.Parse(respondData);

            string tag = jo.GetValue("tag").ToString();
            int code = (int)(jo.GetValue("code"));
            
            respondJO.Add("tag", tag);
            respondJO.Add("code", code);

            if (code == (int)TLJCommon.Consts.Code.Code_OK)
            {
                //respondJO.Add("uid", jo.GetValue("uid").ToString());
                respondJO.Add("name", jo.GetValue("name").ToString());
                respondJO.Add("gold", (int)(jo.GetValue("gold")));
                respondJO.Add("yuanbao", (int)(jo.GetValue("yuanbao")));
                respondJO.Add("phone", jo.GetValue("phone").ToString());
            }

            // 发送给客户端
            {
                LogicService.m_serverUtil.sendMessage((IntPtr)connId, respondJO.ToString());
            }
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog(ex.Message);

            // 客户端参数错误
            //respondJO.Add("code", Convert.ToInt32(TLJCommon.Consts.Code.Code_ParamError));

            // 发送给客户端
            //LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
        }
    }
}
