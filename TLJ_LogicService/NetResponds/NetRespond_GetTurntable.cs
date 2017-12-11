using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_GetTurntable
{
    public static string doAskCilentReq_GetTurntable(IntPtr connId, string reqData)
    {
        JObject respondJO = new JObject();

        try
        {
            JObject jo = JObject.Parse(reqData);

            respondJO.Add("tag", jo.GetValue("tag").ToString());

            JArray ja = new JArray();

            for (int i = 0; i < GetTurntableDataScript.getInstance().getDataList().Count; i++)
            {
                JObject temp = new JObject();

                temp.Add("id", GetTurntableDataScript.getInstance().getDataList()[i].m_id);
                temp.Add("reward", GetTurntableDataScript.getInstance().getDataList()[i].m_reward);
                temp.Add("probability", GetTurntableDataScript.getInstance().getDataList()[i].m_probability);

                ja.Add(temp);
            }

            respondJO.Add("turntable_list",ja);

            // 发送给客户端
            LogicService.m_serverUtil.sendMessage(connId, respondJO.ToString());
            
            {
                for (int i = 0; i < GetTurntableDataScript.getInstance().m_noticeContentList.Count; i++)
                {
                    string name = GetTurntableDataScript.getInstance().m_noticeContentList[i].m_name;
                    int reward_id = GetTurntableDataScript.getInstance().m_noticeContentList[i].m_reward_id;

                    JObject jo2 = new JObject();
                    jo2.Add("tag", TLJCommon.Consts.Tag_TurntableBroadcast);
                    jo2.Add("name", name);
                    jo2.Add("reward_id", reward_id);
                    jo2.Add("canShowSelf", 1);

                    LogicService.m_serverUtil.sendMessage(connId, jo2.ToString());
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
}