using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ClientManager
{
    static ClientManager s_instance = null;

    List<ClientData> m_clientList = new List<ClientData>();

    public static ClientManager getInstance()
    {
        if (s_instance == null)
        {
            s_instance = new ClientManager();
        }

        return s_instance;
    }

    public void addClient(IntPtr connId)
    {
        for (int i = 0; i < m_clientList.Count; i++)
        {
            if (connId == m_clientList[i].m_connId)
            {
                return;
            }
        }

        m_clientList.Add(new ClientData(connId));
    }

    public ClientData getClientByConnId(IntPtr connId)
    {
        for (int i = 0; i < m_clientList.Count; i++)
        {
            if (connId == m_clientList[i].m_connId)
            {
                return m_clientList[i];
            }
        }

        return null;
    }

    public ClientData getClientByUID(string uid)
    {
        for (int i = 0; i < m_clientList.Count; i++)
        {
            if (uid.CompareTo(m_clientList[i].m_uid) == 0)
            {
                return m_clientList[i];
            }
        }

        return null;
    }

    public void deleteClientByConnId(IntPtr connId)
    {
        for (int i = 0; i < m_clientList.Count; i++)
        {
            if (connId == m_clientList[i].m_connId)
            {
                m_clientList.RemoveAt(i);
                return;
            }
        }
    }

    public void deleteClientByUID(string uid)
    {
        for (int i = 0; i < m_clientList.Count; i++)
        {
            if (uid.CompareTo(m_clientList[i].m_uid) == 0)
            {
                m_clientList.RemoveAt(i);
                return;
            }
        }
    }

    public void setClientUID(IntPtr connId, string uid)
    {
        ClientData temp = getClientByConnId(connId);
        if (temp != null)
        {
            temp.m_uid = uid;
        }
    }
}

public class ClientData
{
    public IntPtr m_connId;
    public string m_uid = "";

    public ClientData(IntPtr connId)
    {
        m_connId = connId;
    }
}