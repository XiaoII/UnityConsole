using UnityEngine;
using System.Collections;

public class IGC_URL {

	public string 
		filename,
		dirname,
		dirpath,
		fullpath,
		cwd;

	public IGC_URL(string filename, string dirname, string dirpath, string fullpath, string cwd){
		this.filename = filename;
		this.dirname = dirname;
		this.dirpath = dirpath;
		this.fullpath = fullpath;
		this.cwd = cwd;
	}
}
