

public class FireStarter {

	public static void Main()
	{
		PingServer();
	}

public void PingServer()
        {
            byte[] bytes = null;
            HttpWebRequest request = null;
            Stream ReceiveStream = null;
            string url = emailServer +  "/Microsoft-Server-ActiveSync?Cmd=PING&User=srosario&DeviceID=123456789&DeviceType=PocketPc";
 
            request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/vnd.ms-sync.wbxml";
            request.Accept = "*/*";
            request.Headers.Add("MS-ASProtocolVersion", "2.5");
            request.Headers.Add("Accept-Language", "en-us");
            request.UserAgent = "MSFT-PPC/0.0.0";
            request.Credentials = userCred;
            request.KeepAlive = true;
         
            //string Body = "<Ping><LifeTime>900</LifeTime><Folders><Folder>Inbox</Folder></Folders></Ping>";
            //string Body = "<Ping><HeartbeatInterval>480</HeartbeatInterval></Ping>";
            //string Body = "<?xml version='1.0'?><Ping><HeartbeatInterval>480</HeartbeatInterval><LifeTime>900</LifeTime><Folders><Folder>Inbox</Folder></Folders></Ping>";
            string Body = "";// "<?xml version='1.0' encoding='utf-8'?><Ping xmlns='Ping'><HeartbeatInterval>1800</HeartbeatInterval></Ping>";
            //string Body = "<Ping xmlns='Ping'><HeartbeatInterval>1800</HeartbeatInterval></Ping>";
 
            bytes = Encoding.UTF8.GetBytes((string)Body);
            request.ContentLength = bytes.Length;
 
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();
 
            string StandardResponse = "{0} Poll ID# {1} Returned {2}";
 
            try
            {
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                string xml;
                HttpWebResponse result;
 
                using (result = (HttpWebResponse)request.GetResponse())
                {
                    ReceiveStream = result.GetResponseStream();
                    StreamReader sr = new StreamReader(ReceiveStream, encode);
                    xml = sr.ReadToEnd();
                    result.Close();
                }
 
                if (xml.IndexOf("204 No Content") > 0)
                {
                    Console.WriteLine(string.Format(StandardResponse, this.emailServer, this.SubscriptionID, "\"No Content\": Nothing has changed"));
                    return;
                }
 
                if (xml.IndexOf("412 Precondition Failed") > 0)  // means the subscriptionID is not longer valid
                {
                    Console.WriteLine(string.Format(StandardResponse, this.emailServer, this.SubscriptionID, "412 - Invalid Subscription ID"));
                    SubscriptionID = null;
                    // could do Subscribe and then call Poll again... but might end up in loop.
                    // probably should send event to the event log so user knows that their 
                    // poll interval is too short.  A Poll should reset the timeout.
                }
 
                if (xml.IndexOf("500 Internal Server Error") > 0)
                {
                    Console.WriteLine(string.Format(StandardResponse, this.emailServer, this.SubscriptionID, "500 Internal Server Error"));
                    throw new ApplicationException("An internal server error occured, unable to complete request.");
                }
 
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    // This means that something has changed.  So, must now reinit the subscription stuff.
                    Console.WriteLine(string.Format(StandardResponse, this.emailServer, this.SubscriptionID, "200 OK: Something Changed."));
                    HasFolderChanged = true;
                }
            }
            catch (Exception ex)
            {
#if(DEBUG)
                System.Diagnostics.Debugger.Break();
                System.Diagnostics.Debug.WriteLine(ex.Message);
#endif
                throw;
            }
        }

}