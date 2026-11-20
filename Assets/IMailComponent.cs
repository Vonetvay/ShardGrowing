using UnityEngine;

public interface IMailComponent
{
    GameObject[] TakeRequest();
    void GetRequest(GameObject[] objects);
}
