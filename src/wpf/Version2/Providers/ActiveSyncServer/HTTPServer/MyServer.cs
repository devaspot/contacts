// CsHTTPServer
//
// rmortega77@yahoo.es
// The use of this software is subject to the following agreement
//
// 1. Don't use it to kill.
// 2. Don't use to lie.
// 3. If you learned something give it back.
// 4. If you make money with it, consider sharing with the author.
// 5. If you do not complies with 1 to 5, you may not use this software.
//
// If you have money to spare, and found useful, or funny, or anything 
// worth on this software, and want to contribute with future free 
// software development.
// You may contact the author at rmortega77@yahoo.es 
// Contributions can be from money to hardware spareparts (better), or 
// a bug fix (best), or printed bibliografy, or thanks... 
// just write me.

using System;
using System.Text;

using System.IO;
using System.Web;

using Microsoft.Win32;


namespace Synrc.HTTPServer
{
	/// <summary>
	/// Summary description for MyServer.
	/// </summary>
	public class MyServer : CsHTTPServer 
	{
		public string Folder;
		
		public MyServer(): base() 
		{
			this.Folder = "c:\\www";
		}
	
		public MyServer(int thePort, string theFolder): base(thePort) 
		{
			this.Folder = theFolder;
		}

		public override void OnResponse(ref HTTPRequestStruct rq, ref HTTPResponseStruct rp)
		{
			if (rq.Headers["MS-ASProtocolVersion"] != null)
			{
				rp.Headers[""] = "";
			}
			if (rq.URL.Equals("/Microsoft-Server-ActiveSync"))
			{
				if (rq.Method == "OPTIONS")
				{
					rp.Headers["MS-Server-ActiveSync"] = "6.5.7638.1";
					rp.Headers["MS-ASProtocolVersions"] = "1.0,2.0,2.1,2.5";
					rp.Headers["MS-ASProtocolCommands"] = "Sync,Provision,Search,Ping";
					rp.Headers["Content-Length"] = "0";
					rp.Headers["Allow"] = "OPTIONS,POST";
					rp.Headers["Public"] = "OPTIONS,POST";
					rp.Headers["Connection"] = "close";
					rp.Headers["Pragma"] = "no-cache";
					rp.Headers["X-Powered-By"] = "ASP.NET";

				}
			}

			return;

			string path = this.Folder + "\\" + rq.URL.Replace("/","\\");

			if (Directory.Exists(path))
			{
				if (File.Exists(path + "default.htm"))
					path += "\\default.htm";
				else
				{
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path);

					string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
					bodyStr += "<HTML><HEAD>\n";
					bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
					bodyStr += "</HEAD>\n";
					bodyStr += "<BODY><p>Folder listing, to do not see this add a 'default.htm' document\n<p>\n";
					for ( int i = 0; i<dirs.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName ( dirs[i] ) + "/\">[" +  Path.GetFileName( dirs[i] ) + "]</a>\n"; 
					for ( int i = 0; i<files.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName( files[i] ) + "\">" +  Path.GetFileName( files[i] ) + "</a>\n"; 
					bodyStr += "</BODY></HTML>\n";

					rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);
					return;
				}
			}

			if (File.Exists(path))
			{
				RegistryKey rk = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(path), true);

				// Get the data from a specified item in the key.
				String s = (String)rk.GetValue("Content Type");

				// Open the stream and read it back.
				rp.fs = File.Open(path, FileMode.Open);
				if ( s != "")
					rp.Headers["Content-type"] = s;
			}
			else
			{
                
				rp.status = (int)RespState.NOT_FOUND;

				string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
				bodyStr += "<HTML><HEAD>\n";
				bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
				bodyStr += "</HEAD>\n";
				bodyStr += "<BODY>File not found!!</BODY></HTML>\n";

				rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);

			}

		}
	}
}
