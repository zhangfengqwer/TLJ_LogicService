using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GetTurntableDataScript
{
    static GetTurntableDataScript s_instance = null;

    List<TurntableData> m_dataList = new List<TurntableData>();
    public List<NoticeContent> m_noticeContentList = new List<NoticeContent>();

    public static GetTurntableDataScript getInstance()
    {
        if (s_instance == null)
        {
            s_instance = new GetTurntableDataScript();
        }

        return s_instance;
    }

    public void clear()
    {
        m_dataList.Clear();
    }

    public void initJson(string json)
    {
        m_dataList.Clear();

        {
            JObject jo = JObject.Parse(json);
            JArray turntable_list = (JArray)JsonConvert.DeserializeObject(jo.GetValue("turntable_list").ToString());

            for (int i = 0; i < turntable_list.Count; i++)
            {
                int id = (int)turntable_list[i]["id"];
                string reward = (string)turntable_list[i]["reward"];
                int probability = (int)turntable_list[i]["probability"];

                TurntableData temp = new TurntableData(id,reward, probability);
                m_dataList.Add(temp);
            }
        }

        {
            m_noticeContentList.Clear();

            for (int i = 0; i < 5; i++)
            {
                m_noticeContentList.Add(new NoticeContent(RandomName.GetName(), RandomUtil.getRandom(1,10)));
            }
        }
    }

    public void addNoticeContent(NoticeContent noticeContent)
    {
        if (m_noticeContentList.Count >= 5)
        {
            m_noticeContentList.RemoveAt(0);
        }

        m_noticeContentList.Add(noticeContent);
    }

    public List<TurntableData> getDataList()
    {
        return m_dataList;
    }

    public TurntableData getDataById(int id)
    {
        TurntableData temp = null;
        for (int i = 0; i < m_dataList.Count; i++)
        {
            if (m_dataList[i].m_id == id)
            {
                temp = m_dataList[i];
                break;
            }
        }

        return temp;
    }
}

public class TurntableData
{
    public int m_id;
    public string m_reward;
    public int m_probability;

    public TurntableData(int id, string reward, int probability)
    {
        m_id = id;
        m_reward = reward;
        m_probability = probability;
    }
}

public class NoticeContent
{
    public string m_name;
    public int m_reward_id;

    public NoticeContent(string name, int reward_id)
    {
        m_name = name;
        m_reward_id = reward_id;
    }
}