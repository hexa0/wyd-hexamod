using System.IO;

public static class PathJoin
{
	public static string Join(params object[] paths)
	{
		string path = string.Empty;

		for (int i = 0; i < paths.Length; i++)
		{
			path = Path.Combine(path, paths[i].ToString());
		}

		return path;
	}
}