using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class MailSystem
{
    private static Dictionary<int, IMailComponent> mails = new Dictionary<int, IMailComponent>();
    private static int s_lastMailAddress = 0;

    public static int RegisterNewMail(IMailComponent mail) 
    {
        mails[s_lastMailAddress] = mail;
        Debug.Log(s_lastMailAddress);
        s_lastMailAddress++;
        return s_lastMailAddress - 1;
    }

    public static void RemoveMail(int address) 
    {
        if (mails.ContainsKey(address))
        {
            mails.Remove(address);
        }
        else 
        {
            Debug.LogError($"Не существует почтового адреса {address}");
        }
    }

    public static void Request(int from, int to) 
    {
        if (mails.ContainsKey(to))
        {
            if (mails.ContainsKey(from))
            {
                mails[to].GetRequest(mails[from].TakeRequest());
            }
            else 
            {
                Debug.LogError($"Адреса {from} получателя не существует");
            }
        }
        else 
        {
            Debug.LogError($"Адреса {to} отправителя не существует");
        }
    }
}
