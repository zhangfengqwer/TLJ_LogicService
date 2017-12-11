using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLJ_LogicService;

class NetRespond_ProgressTask
{
    public static string doAskCilentReq_ProgressTask(IntPtr connId, string reqData)
    {
        try
        {
            JObject jo = JObject.Parse(reqData);
            string tag = jo.GetValue("tag").ToString();
            string uid = jo.GetValue("uid").ToString();
            int task_id = (int)jo.GetValue("task_id");

            Request_ProgressTask.doRequest(uid, task_id);
        }
        catch (Exception ex)
        {
            LogUtil.getInstance().addDebugLog("NetRespond_ProgressTask.doAskCilentReq_ProgressTask异常----" + ex.Message);
        }
        
        return "";
    }
}