using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

public class Program
{
    private static char[] _characters = "abcdefghijklmnopqrstuvwxyz0123456789./?&:-_".ToCharArray();

    public static void Main()
    {
        Console.Title = "WordPressSiteChecker | Made by https://github.com/GabryB03/";

        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        ServicePointManager.MaxServicePoints = int.MaxValue;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        string host = "";

        while (TakeHostFromURI(FilterString(host)) == "")
        {
            Console.Write("Please, insert the host / URI to check here: ");
            host = Console.ReadLine();

            if (TakeHostFromURI(FilterString(host)) == "")
            {
                Console.WriteLine("Please, insert a valid host / URI.");
            }
        }

        host = FilterString(TakeHostFromURI(FilterString(host)));
        Console.WriteLine($"Checking your host / URI (\"{host}\"), please wait a while.");
        Console.WriteLine(IsWordPressSiteValid(host) ? "The specified host / URI is based on WordPress. VALID." : "The specified host / URI is NOT based on WordPress. INVALID.");
        Console.WriteLine("Press ENTER in order to exit from the program.");
        Console.ReadLine();
    }

    private static string FilterString(string str)
    {
        string result = "";
        str = str.ToLower();

        foreach (char c in str)
        {
            foreach (char s in _characters)
            {
                if (c.Equals(s))
                {
                    result += c;
                    break;
                }
            }
        }

        return result;
    }

    private static string TakeHostFromURI(string uri)
    {
        if (uri.StartsWith("http://"))
        {
            uri = uri.Substring("http://".Length);
        }

        if (uri.StartsWith("https://"))
        {
            uri = uri.Substring("https://".Length);
        }

        if (uri.StartsWith("www."))
        {
            uri = uri.Substring("www.".Length);
        }

        if (uri.Contains("/"))
        {
            return uri.Split('/')[0];
        }
        else if (uri.Contains("?"))
        {
            return uri.Split('?')[0];
        }
        else if (uri.Contains("&"))
        {
            return uri.Split('&')[0];
        }

        return uri;
    }

    private static bool IsWordPressSiteValid(string host)
    {
        try
        {
            var request1 = (HttpWebRequest)WebRequest.Create($"http://{host}/wp-json");

            request1.Proxy = null;
            request1.UseDefaultCredentials = false;
            request1.AllowAutoRedirect = false;
            request1.Timeout = 70000;

            var field1 = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic);

            request1.Method = "GET";

            var headers1 = new CustomWebHeaderCollection(new Dictionary<string, string>
            {
                ["Host"] = $"{host}"
            });

            field1.SetValue(request1, headers1);

            var response1 = request1.GetResponse();
            string content1 = Encoding.UTF8.GetString(ReadFully(response1.GetResponseStream()));

            string headerLocation = "";

            try
            {
                headerLocation = response1.Headers["Location"].ToString();
            }
            catch
            {

            }

            response1.Close();
            response1.Dispose();

            if (headerLocation == "" && content1.Contains("wp-site-health"))
            {
                return true;
            }

            string composedURI = "", composedHost = "";

            if (headerLocation.StartsWith("http://"))
            {
                composedURI += "http://";
                headerLocation = headerLocation.Substring("http://".Length);
            }
            else if (headerLocation.StartsWith("https://"))
            {
                composedURI += "https://";
                headerLocation = headerLocation.Substring("https://".Length);
            }

            if (headerLocation.StartsWith("www."))
            {
                composedURI += "www.";
                composedHost += "www.";
                headerLocation = headerLocation.Substring("www.".Length);
            }

            composedURI += $"{host}/wp-json";
            composedHost += host;

            var request2 = (HttpWebRequest)WebRequest.Create(composedURI);

            request2.Proxy = null;
            request2.UseDefaultCredentials = false;
            request2.AllowAutoRedirect = false;
            request2.Timeout = 70000;

            var field2 = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic);

            request2.Method = "GET";

            var headers2 = new CustomWebHeaderCollection(new Dictionary<string, string>
            {
                ["Host"] = composedHost
            });

            field2.SetValue(request2, headers2);

            var response2 = request2.GetResponse();
            string content2 = Encoding.UTF8.GetString(ReadFully(response2.GetResponseStream()));

            response2.Close();
            response2.Dispose();

            return content2.Contains("wp-site-health");
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}