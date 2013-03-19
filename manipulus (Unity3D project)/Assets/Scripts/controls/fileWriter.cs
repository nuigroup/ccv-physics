using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


public class fileWriter : MonoBehaviour {
	
	private const string FILE_NAME = "Test.txt";
	
	//writing
	public void writeData(string inputString)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("===========");
		sb.AppendLine("LEVEL: " + Application.loadedLevel.ToString());
		sb.AppendLine("TIME");
		sb.AppendLine(inputString);
		sb.AppendLine("===========");
		
		if(File.Exists(FILE_NAME))
		{
			Debug.Log("File excists already.. appending message to:" + FILE_NAME);
			using(StreamWriter outfile = File.AppendText(FILE_NAME))
			{
				outfile.Write(sb.ToString());
			}
			
		}else{
			
			using(StreamWriter outfile = File.CreateText(FILE_NAME))
			{
				outfile.Write(sb.ToString());
			}
		}
	}
}
