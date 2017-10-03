using UnityEngine;
using System.Collections;

public class ControlBridge : IGC_Command {
	
	public ControlBridge(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "bridge";
		this.usage = "usage: bridge <input_file>";
		this.help_text = "this command uses data from an input file to set the height of a bridge. \nthe file must contain only numbers seporated by commas. the number of numbers must equal the number of segments in the bridge.";
	}
	
	public override string command_function ()
	{
		if(argv.Length != 2){return malformed_error+"\n"+usage;}

		IGC_FileSystem fs = virtualSystem.fileSystem;
		IGC_URL url = fs.ParseURL (argv [1], issuer.cwd);
		IGC_File file = fs.GetFile (url.fullpath);

		if(file == null){return "input file does not exist";}

		Transform platforms = GameObject.Find("room/platforms").transform;
		string[] bridgeFormationData = IGC_Utils.SplitString (",", file.data);

		if(bridgeFormationData.Length != 10){return "incorrect number of comma seporated numbers in input file. must be 10.";}

		int i=0;
		foreach(string s in bridgeFormationData)
		{
			int num = int.Parse( s.Trim() );
			Transform t = platforms.Find(i.ToString());

			t.GetComponent<BridgeSegment>().SetTargetPosition(new Vector3(
				t.transform.localPosition.x,
				num,
				t.transform.localPosition.z
			));

			i++;
		}
		
		return "bridge formation reset";
	}
}