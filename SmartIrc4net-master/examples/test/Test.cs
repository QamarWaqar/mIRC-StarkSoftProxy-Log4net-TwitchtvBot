/**
 * $Id$
 * $Revision$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 * This is a simple test client for the library.
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Meebey.SmartIrc4net;
using System.Text;
// this is my reading a text from text file //
using ReadingFromTextFile;

// This is an VERY basic example how your IRC application could be written
// its mainly for showing how to use the API, this program just connects sends
// a few message to a channel and waits for commands on the console
// (raw RFC commands though! it's later explained).
// There are also a few commands the IRC bot/client allows via private message.
public class Test
{
    // this is my string that will be filled with text from file with commas //
    public static string channelName;
    public static string botName;
    public static string botOAuth;
    // -------------------------------------------------------------------- //
    public static string twitterAccessToken;
    public static string twitterAccessTokenSecret;
    public static string twitterConsumerKey;
    public static string twitterConsumerSecret;
    // this is my array which will store the list of moderators of the channel including the broadcaster //
    public static int modArrayInt = 0;
    public static string[] modArray = new string[100];

    // this is my method to fill the static string //
    // channelName, botName, botOAuth //
    public static void fillTextFileString()
    {
        // this will return the contents of the file with commas //
        string fileContentsWithCommas = ReadingFromTextFile.readTextFile.readTxtFile();
        // now filling channelName and botName and botOAuth //
        string[] strArr = fileContentsWithCommas.Split(',');
        channelName = strArr[0];
        botName = strArr[1];
        botOAuth = strArr[2];
    }
    public static void fillTwitterCredentiasFromFile()
    {
        string twitterCredentialsFromFileWithCommas = ReadingFromTextFile.readTextFile.readTwitterCredentials();
        string[] strArr = twitterCredentialsFromFileWithCommas.Split(',');
        twitterAccessToken = strArr[0];
        twitterAccessTokenSecret = strArr[1];
        twitterConsumerKey = strArr[2];
        twitterConsumerSecret = strArr[3];
    }

    // make an instance of the high-level API
    public static IrcClient irc = new IrcClient();

    // this method we will use to analyse queries (also known as private messages)
    public static void OnQueryMessage(object sender, IrcEventArgs e)
    {
        switch (e.Data.MessageArray[0])
        {
            // debug stuff
            case "dump_channel":
                string requested_channel = e.Data.MessageArray[1];
                // getting the channel (via channel sync feature)
                Channel channel = irc.GetChannel(requested_channel);

                // here we send messages
                irc.SendMessage(SendType.Message, e.Data.Nick, "<channel '" + requested_channel + "'>");

                irc.SendMessage(SendType.Message, e.Data.Nick, "Name: '" + channel.Name + "'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Topic: '" + channel.Topic + "'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Mode: '" + channel.Mode + "'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Key: '" + channel.Key + "'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '" + channel.UserLimit + "'");

                // here we go through all users of the channel and show their
                // hashtable key and nickname 
                string nickname_list = "";
                nickname_list += "Users: ";
                foreach (DictionaryEntry de in channel.Users)
                {
                    string key = (string)de.Key;
                    ChannelUser channeluser = (ChannelUser)de.Value;
                    nickname_list += "(";
                    if (channeluser.IsOp)
                    {
                        nickname_list += "@";
                    }
                    if (channeluser.IsVoice)
                    {
                        nickname_list += "+";
                    }
                    nickname_list += ")" + key + " => " + channeluser.Nick + ", ";
                }
                irc.SendMessage(SendType.Message, e.Data.Nick, nickname_list);

                irc.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
                break;
            case "gc":
                GC.Collect();
                break;
            // typical commands
            case "join":
                irc.RfcJoin(e.Data.MessageArray[1]);
                break;
            case "part":
                irc.RfcPart(e.Data.MessageArray[1]);
                break;
            case "die":
                Exit();
                break;
        }
    }

    // this method handles when we receive "ERROR" from the IRC server
    public static void OnError(object sender, ErrorEventArgs e)
    {
        System.Console.WriteLine("Error: " + e.ErrorMessage);
        Exit();
    }

    // this method will get all IRC messages
    public static void OnRawMessage(object sender, IrcEventArgs e)
    {
        System.Console.WriteLine("Received: " + e.Data.RawMessage);

        // Entering my Logic to intercept the bot Commands //
        //irc.SendMessage(SendType.Message, "#Cloakers", "Hello World!");
        //string channel = Test.channelName;
        if (e.Data.RawMessage.Contains("#time"))
        {
            irc.SendMessage(SendType.Message, Test.channelName, DateTime.Now.ToString());
        }
        if (e.Data.RawMessage.Contains("#help"))
        {
            string helpText = readTextFile.readHelpFile();
            irc.SendMessage(SendType.Message, Test.channelName, helpText);
        }
        if (e.Data.RawMessage.Contains(":jtv") && !(e.Data.RawMessage.Contains("!")) && e.Data.RawMessage.Contains("+o"))
        {
            // the pattern string that we are looking for is //
            // Received: :jtv MODE #cloakers -o cloakers //
            // now we have to extract the string after the # but do not include the space after it //
            int index = e.Data.RawMessage.IndexOf("#");
            // index # = 20 // always // add in it channel name lenght, plus space and +o and a space //
            //                                                          +1 +2 +1                      //
            // so index2+1 will be the starting index of the operators name till the end of string //
            int index2 = index + Test.channelName.Length + 1 + 2 + 1;
            StringBuilder sB = new StringBuilder();
            for (int i = index2; i < e.Data.RawMessage.Length; i++)
            {
                //if (e.Data.RawMessage[i] == ' ') { break; }
                sB.Append(e.Data.RawMessage[i]);
            }
            modArray[modArrayInt++] = sB.ToString();
        }
        if (e.Data.RawMessage.Contains("#L"))
        {
            /*int index = e.Data.RawMessage.IndexOf('#');
            int index2 = e.Data.RawMessage.IndexOf('#', index + 1);
            StringBuilder sB = new StringBuilder();
            for (int i = (index2 + 2); i < e.Data.RawMessage.Length; i++)
            {
                if (e.Data.RawMessage[i] == ' ') { break; }
                sB.Append(e.Data.RawMessage[i]);
            }
            sB.Insert(0, "<3"); sB.Append("<3", 0, 2);
            irc.SendMessage(SendType.Message, Test.channelName, sB.ToString());*/

            /*StringBuilder sB = new StringBuilder();
            for (int i = 0; i <= (modArrayInt - 1); i++)
            {
                sB.Append(modArray[i]);
            }
            irc.SendMessage(SendType.Message, Test.channelName, sB.ToString());*/
        }
        /*if (e.Data.RawMessage.Contains("#H"))
        {
            int index = e.Data.RawMessage.IndexOf('#');
            int index2 = e.Data.RawMessage.IndexOf('#', index + 1);
            StringBuilder sB = new StringBuilder();
            for (int i = (index2 + 2); i < e.Data.RawMessage.Length; i++)
            {
                if (e.Data.RawMessage[i] == ' ') { break; }
                sB.Append(e.Data.RawMessage[i]);
            }
            sB.Insert(0, "Dirt "); sB.Append(" Bag", 0, 4);
            irc.SendMessage(SendType.Message, Test.channelName, sB.ToString());
        }*/
        if (e.Data.RawMessage.Contains("#T"))
        {
            bool checkMod = false;
            for (int i = 0; i <= (modArrayInt - 1); i++)
            {
                if (e.Data.Ident == modArray[i]) { checkMod = true; }
            }
            /*foreach (var item in modArray)
            {
                if (e.Data.RawMessage.Contains(item)) { checkMod = true; }
            }*/
            if (checkMod)
            {
                int index = e.Data.RawMessage.IndexOf('#');
                int index2 = e.Data.RawMessage.IndexOf('#', index + 1);
                StringBuilder sB = new StringBuilder();
                for (int i = (index2 + 2); i < e.Data.RawMessage.Length; i++)
                {
                    sB.Append(e.Data.RawMessage[i]);
                }
                // And now tweet  the sB out //
                string pubOrNot = TwitterTweetAPI.TwitterTweet.tweet(twitterAccessToken, twitterAccessTokenSecret, twitterConsumerKey, twitterConsumerSecret, sB.ToString());
                if (pubOrNot == "True") { irc.SendMessage(SendType.Message, Test.channelName, "Your Tweet was Sent Successfully"); } else { irc.SendMessage(SendType.Message, Test.channelName, "Your Tweet was Not Sent Successfully"); }
            }
        }
    }
    public static void Main(string[] args)
    {
        // this is my code filling the static variables //
        fillTextFileString();
        fillTwitterCredentiasFromFile();

        Thread.CurrentThread.Name = "Main";

        // UTF-8 test
        irc.Encoding = System.Text.Encoding.UTF8;

        // wait time between messages, we can set this lower on own irc servers
        irc.SendDelay = 200;

        // we use channel sync, means we can use irc.GetChannel() and so on
        irc.ActiveChannelSyncing = true;

        // here we connect the events of the API to our written methods
        // most have own event handler types, because they ship different data
        irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
        irc.OnError += new ErrorEventHandler(OnError);
        irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

        string[] serverlist;
        // the server we want to connect to, could be also a simple string
        serverlist = new string[] { "irc.twitch.tv" }; // change from "irc.freenode.org" to "irc.twitch.tv" //
        int port = 6667;
        string channel = Test.channelName; // change from "#smartirc-test" to "#cloakers" //
        try
        {
            // here we try to connect to the server and exceptions get handled
            irc.Connect(serverlist, port);
        }
        catch (ConnectionException e)
        {
            // something went wrong, the reason will be shown
            System.Console.WriteLine("couldn't connect! Reason: " + e.Message);
            Exit();
        }

        try
        {
            // here we logon and register our nickname and so on 
            irc.Login(Test.botName, Test.botName, 0, Test.botName, Test.botOAuth); // change from ["SmartIRC","SmartIrc4net Test Bot"] to ["cloakers","xhlhmno8l1savabauqbzxmvwx3y11l"]
            // join the channel
            irc.RfcJoin(channel);

            /*for (int i = 0; i < 3; i++) {
                // here we send just 3 different types of messages, 3 times for
                // testing the delay and flood protection (messagebuffer work)
                irc.SendMessage(SendType.Message, channel, "test message ("+i.ToString()+")");
                irc.SendMessage(SendType.Action, channel, "thinks this is cool ("+i.ToString()+")");
                irc.SendMessage(SendType.Notice, channel, "SmartIrc4net rocks ("+i.ToString()+")");
            }*/

            // spawn a new thread to read the stdin of the console, this we use
            // for reading IRC commands from the keyboard while the IRC connection
            // stays in its own thread
            new Thread(new ThreadStart(ReadCommands)).Start();

            // here we tell the IRC API to go into a receive mode, all events
            // will be triggered by _this_ thread (main thread in this case)
            // Listen() blocks by default, you can also use ListenOnce() if you
            // need that does one IRC operation and then returns, so you need then 
            // an own loop 
            irc.Listen();

            // when Listen() returns our IRC session is over, to be sure we call
            // disconnect manually
            irc.Disconnect();
        }
        catch (ConnectionException)
        {
            // this exception is handled because Disconnect() can throw a not
            // connected exception
            Exit();
        }
        catch (Exception e)
        {
            // this should not happen by just in case we handle it nicely
            System.Console.WriteLine("Error occurred! Message: " + e.Message);
            System.Console.WriteLine("Exception: " + e.StackTrace);
            Exit();
        }
    }

    public static void ReadCommands()
    {
        // here we read the commands from the stdin and send it to the IRC API
        // WARNING, it uses WriteLine() means you need to enter RFC commands
        // like "JOIN #test" and then "PRIVMSG #test :hello to you"
        while (true)
        {
            string cmd = System.Console.ReadLine();
            if (cmd.StartsWith("/list"))
            {
                int pos = cmd.IndexOf(" ");
                string channel = null;
                if (pos != -1)
                {
                    channel = cmd.Substring(pos + 1);
                }

                IList<ChannelInfo> channelInfos = irc.GetChannelList(channel);
                Console.WriteLine("channel count: {0}", channelInfos.Count);
                foreach (ChannelInfo channelInfo in channelInfos)
                {
                    Console.WriteLine("channel: {0} user count: {1} topic: {2}",
                                      channelInfo.Channel,
                                      channelInfo.UserCount,
                                      channelInfo.Topic);
                }
            }
            else
            {
                irc.WriteLine(cmd);
            }
        }
    }

    public static void Exit()
    {
        // we are done, lets exit...
        System.Console.WriteLine("Exiting...");
        System.Environment.Exit(0);
    }
}
