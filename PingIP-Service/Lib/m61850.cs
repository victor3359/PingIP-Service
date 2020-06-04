using System;
using System.IO;
using System.Collections.Generic;

using IEC61850.Client;
using IEC61850.Common;

namespace PingIP_Service
{
    class m61850
    {
		private string _host;
		private IedConnection _conn;
		private List<string> fileList = new List<string>();


		public m61850(string host)
        {
			_host = host;
			_conn = new IedConnection();
			Console.WriteLine("Connect to " + _host);

			try
			{
				IedConnect();

				Console.WriteLine("Files in server root directory:");
				List<string> serverDirectory = _conn.GetServerDirectory(true);

				foreach (string entry in serverDirectory)
				{
					Console.WriteLine(entry);
				}
				
				Console.WriteLine();
				Console.WriteLine("File directory tree at server:");
				printFiles(_conn, "", "");
				Console.WriteLine();

				if (!Directory.Exists(@"COMTRADE"))
				{
					Directory.CreateDirectory(@"COMTRADE");
				}
				DirectoryInfo di = new DirectoryInfo(@"COMTRADE");
				foreach (var fi in di.GetFiles())
				{
					fileList.Add($"/COMTRADE/{fi.Name}");
				}
				_conn.Abort();
			}
			catch (IedConnectionException e)
			{
				Console.WriteLine(e.Message);
			}

			//_conn.Dispose();
		}
		private void IedConnect() => _conn.Connect(_host, 102);
		public IedConnectionState GetState()
		{
			return _conn.GetState();
		}
		public void UpdateComtradeFiles()
		{
			IedConnect();
			List<string> serverDirectory = _conn.GetServerDirectory(true);

			foreach (string entry in serverDirectory)
			{
				string tmp = entry.Replace("/", "\\");
				if (!fileList.Contains(entry))
				{
					string filename = $"{AppDomain.CurrentDomain.BaseDirectory}{tmp}";
					Console.WriteLine($"Download file {entry}");
					FileStream fs = new FileStream(filename, FileMode.Create);
					BinaryWriter w = new BinaryWriter(fs);
					fileList.Add(entry);
					_conn.GetFile(entry, new IedConnection.GetFileHandler(getFileHandler), w);
					fs.Close();
				}
			}
			_conn.Abort();
		}

		public static void printFiles(IedConnection con, string prefix, string parent)
		{
			bool moreFollows = false;

			List<FileDirectoryEntry> files = con.GetFileDirectoryEx(parent, null, out moreFollows);

			foreach (FileDirectoryEntry file in files)
			{
				Console.WriteLine(prefix + file.GetFileName() + "\t" + file.GetFileSize() + "\t" +
				MmsValue.MsTimeToDateTimeOffset(file.GetLastModified()));

				if (file.GetFileName().EndsWith("/"))
				{
					printFiles(con, prefix + "  ", parent + file.GetFileName());
				}
			}

			if (moreFollows)
				Console.WriteLine("-- MORE FILES AVAILABLE --");
		}

		static bool getFileHandler(object parameter, byte[] data)
		{
			Console.WriteLine("Received " + data.Length + " bytes");

			BinaryWriter binWriter = (BinaryWriter)parameter;
			binWriter.Write(data);

			return true;
		}
	}
}
