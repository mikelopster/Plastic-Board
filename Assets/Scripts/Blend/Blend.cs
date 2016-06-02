using System;

[Serializable]
public struct Blend {
	public string name;
	public string display;
	public string obj;
	public string[] files;
}

[Serializable]
public struct Blends {
	public Blend[] blends;
}