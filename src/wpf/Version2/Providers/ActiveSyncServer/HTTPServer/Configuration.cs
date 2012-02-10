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
//
// You may contact the author at rmortega77@yahoo.es 
//
// Contributions can be from money to hardware spareparts (better), or 
// a bug fix (best), or printed bibliografy, or thanks... 
// just write me.

//*********************************
// This particular code is taken mostly from 
// Camera Vision
// Copyright © Andrew Kirillov, 2005-2006
// andrew.kirillov@gmail.com

using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Text;

namespace Synrc.HTTPServer
{

	/// <summary>
	/// Application configuration
	/// </summary>
	public class Configuration
	{
		// configuration file name
		private string settingsFile;

		// main window size and position
		public Point	mainWindowLocation = new Point(100, 50);

		// Main window start options
		public bool		startMinimized = false;
		public bool		startServing = false;
		public bool		startOnWindows = false;

		// MyServer settings 
		public string httpFolder = "C:\\www";
		public int httpPort = 80;
		
		// Constructor
		public Configuration(string path)
		{
			settingsFile = Path.Combine(path, "app.config");
		}

		// Save application settings
		public void SaveSettings()
		{
			// open file
			FileStream		fs = new FileStream(settingsFile, FileMode.Create);
			// create XML writer
			XmlTextWriter	xmlOut = new XmlTextWriter(fs, Encoding.UTF8); 

			// use indenting for readability
			xmlOut.Formatting = Formatting.Indented;

			// start document
			xmlOut.WriteStartDocument();
			xmlOut.WriteComment("CameraViewer configuration file");

			// main node
			xmlOut.WriteStartElement("CsHTTPServer");

			// main window node
			xmlOut.WriteStartElement("MainWindow");
			xmlOut.WriteAttributeString("x", mainWindowLocation.X.ToString());
			xmlOut.WriteAttributeString("y", mainWindowLocation.Y.ToString());
			xmlOut.WriteEndElement();

			// MtServer node
			xmlOut.WriteStartElement("MyServer");
			xmlOut.WriteAttributeString("startMinimized", this.startMinimized.ToString());
			xmlOut.WriteAttributeString("startServing", this.startServing.ToString());
			xmlOut.WriteAttributeString("startOnWindows", this.startOnWindows.ToString());
			//
			xmlOut.WriteAttributeString("httpFolder", this.httpFolder);
			xmlOut.WriteAttributeString("httpPort", this.httpPort.ToString());
			xmlOut.WriteEndElement();


			// end document
			xmlOut.WriteEndElement();

			// close file
			xmlOut.Close();
		}

		// Load application settings
		public bool LoadSettings()
		{
			bool	ret = false;

			// check file existance
			if (File.Exists(settingsFile))
			{
				FileStream		fs = null;
				XmlTextReader	xmlIn = null;

				try
				{
					// open file
					fs = new FileStream(settingsFile, FileMode.Open);
					// create XML reader
					xmlIn = new XmlTextReader(fs);

					xmlIn.WhitespaceHandling = WhitespaceHandling.None;
					xmlIn.MoveToContent();

					// check for main node
					if (xmlIn.Name != "CsHTTPServer")
						throw new ApplicationException("");

					// move to next node
					xmlIn.Read();
					if (xmlIn.NodeType == XmlNodeType.EndElement)
						xmlIn.Read();

					// check for main window node
					if (xmlIn.Name != "MainWindow")
						throw new ApplicationException("");

					// read main window position
					int		x = Convert.ToInt32(xmlIn.GetAttribute("x"));
					int		y = Convert.ToInt32(xmlIn.GetAttribute("y"));

					// move to next node
					xmlIn.Read();
					if (xmlIn.NodeType == XmlNodeType.EndElement)
						xmlIn.Read();

					// check for MyServer node
					if (xmlIn.Name != "MyServer")
						throw new ApplicationException("");

					startMinimized = Convert.ToBoolean(xmlIn.GetAttribute("startMinimized"));
					startServing = Convert.ToBoolean(xmlIn.GetAttribute("startServing"));
					startOnWindows = Convert.ToBoolean(xmlIn.GetAttribute("startOnWindows"));

					httpFolder = xmlIn.GetAttribute("httpFolder");
					httpPort = Convert.ToInt32(xmlIn.GetAttribute("httpPort"));

					mainWindowLocation = new Point(x, y);

					ret = true;
				}
					// catch any exceptions
				catch (Exception)
				{
				}
				finally
				{
					if (xmlIn != null)
						xmlIn.Close();
				}
			}
			return ret;
		}

	
	}
}
