using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;


public class IGC_Utils : MonoBehaviour {

	/*public static string Base64Encode(string data)
	{
		var b = new BinaryFormatter();
		var m = new MemoryStream();
		
		b.Serialize(m, data);
		
		return Convert.ToBase64String(m.GetBuffer());
	}
	
	public static Dictionary<string, IGC_User> Base64Decode(string base64)
	{
		if(!string.IsNullOrEmpty(base64)){
			var b = new BinaryFormatter();
			var m = new MemoryStream(Convert.FromBase64String(base64));
			
			return (string)b.Deserialize(m);
		}
		return null;
	}*/

	public static string[] ArrayShift(string[] target)
	{
		string[] shifted = new string[target.Length - 1];

		for(int i=1; i<target.Length; i++){
			shifted[i-1] = target[i];
		}

		return shifted;
	}

	public static string ColorString(Color c)
	{
		return c.r.ToString () + "," + c.g.ToString () + "," + c.b.ToString ();
	}

	public static Color ColorFromString(string s)
	{
		string [] channels = SplitString (",", s);

		return new Color(
			float.Parse(channels[0]),
			float.Parse(channels[1]),
			float.Parse(channels[2])
		);
	}

	public static string[] SplitString(string seporator, string str)
	{
		return str.Split(new string[1]{seporator}, System.StringSplitOptions.RemoveEmptyEntries);
	}

	public static void LogStringArray(string[] message)
	{
		LogStringArray(", ", message);
	}

	public static void LogStringArray(string seporator, string[] message)
	{
		Debug.Log (string.Join(seporator, message));
	}


	public static string EscapeForSave(string data)
	{
		return data.Replace(":","%c%l%").Replace("~","%t%l%").Replace("\n", "%n%l%");
	}

	public static string UnescapeSaved(string data)
	{
		return data.Replace("%c%l%", ":").Replace("%t%l%", "~").Replace("%n%l%", "\n");
	}

	public static string EscapeURL(string data)
	{

		//"[~|!|,|<|>|\"|'|:|@|%|*|(|)|{|}]"
		Regex r = new Regex(@"[^\w|\d|.|-|_|/]"); 
		return r.Replace(data, "");
	}

	public static string EscapeUserOrGroup(string data)
	{
		
		//"[~|!|,|<|>|\"|'|:|@|%|*|(|)|{|}]"
		Regex r = new Regex(@"[^\w|\d|-|_]"); 
		return r.Replace(data, "");
	}

	public static string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
}
