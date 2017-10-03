using UnityEngine;
using System.Collections;

public class IGC_Command_netscan : IGC_Command
{
    public IGC_Command_netscan(ref IGC_VirtualSystem virtualSystem)
    {
        this.virtualSystem = virtualSystem;
        this.name = "netscan";
        this.help_text = "the netscan command shows a list of IP addresses on the local network";
        this.description = "show network IP addresses";
    }

    public override string command_function()
    {
        GameObject[] Computers = GameObject.FindGameObjectsWithTag("computer");
        string ipAdd = "Computers on Network: \n";
        for (int i = 0; i < Computers.Length; i++)
        {       
           IGC_VirtualSystem virtualSystem = Computers[i].GetComponent<IGC_VirtualSystem>();
            ipAdd += " " + virtualSystem.name +  "\n";
        }
        return ipAdd;
    }
}
