using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour {

    // Use this for initialization
    public string ipAddress1;
    void Start() {
        

        generateIP();

        
    }

    private void generateIP ()
    {
        //10.0.0.0 - 10.255.255.255                 172.16.0.0-172.31.255.255                   192.168.0.0 - 192.168.255.255
        string[] ip1add = "192|172|10".Split('|');
        
        System.Random rnd = new System.Random();
        string ip1 = ip1add[rnd.Next(ip1add.Length)];
        string ip2;
        string ip3;

        if (ip1 == "192")
        {
            ip2 = "168";
            ip3 = Random.Range(0, 255).ToString();
            ipAddress1 = ip1 + "." + ip2 + "." + ip3 + ".";
        }
        if (ip1 == "172")
        {
            ip2 = Random.Range(16, 31).ToString();
            ip3 = Random.Range(0, 255).ToString();
            ipAddress1 = ip1 + "." + ip2 + "." + ip3 + ".";
        }
        else if (ip1 == "10")
        {
            ip2 = Random.Range(0, 255).ToString();
            ip3 = Random.Range(0, 255).ToString();
            ipAddress1 = ip1 + "." + ip2 + "." + ip3 + ".";
        }


    }


    // Update is called once per frame
    void Update () {
		
	}
}
