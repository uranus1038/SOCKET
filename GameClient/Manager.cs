using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Manager : MonoBehaviour 
{
    public static Manager instance;
    public GameObject Button;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log($"Destroy");
            Destroy(this);
        }
    }

    public void connect()
    {
        Button.SetActive(false);
        Client.instance.connect_to_Server();
    }

}



