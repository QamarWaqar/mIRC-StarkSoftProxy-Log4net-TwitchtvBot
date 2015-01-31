using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ReadingFromTextFile
{
    // This class will basically be reading 3 things for twitch tv bot //
    // 1. The Twitch tv channel name to Join //
    // 2. The Twitch tv bot name //
    // 3. The OAuth of the bot with chat scope Token //
    public class readTextFile
    {
        public static string readTxtFile()
        {
            StringBuilder sB = new StringBuilder();

            FileStream fS = File.OpenRead("textFileToReadFrom.txt");
            StreamReader sR = new StreamReader(fS);
            sB.Append(sR.ReadLine()); sB.Append(",");
            sB.Append(sR.ReadLine()); sB.Append(",");
            sB.Append(sR.ReadLine()); sB.Append(",");
            return sB.ToString();
        }
    }
}
