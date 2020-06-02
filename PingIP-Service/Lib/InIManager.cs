using System.Text;
using System.Runtime.InteropServices;

namespace PingIP_Service
{
    class InIManager
    {
        private static StringBuilder lpReturnedString;
        private static string IniFilePath;
        private static int BufferSize;

        [DllImport(@"kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
               string key, string val, string filePath);
        [DllImport(@"kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal,
            int size, string filePath);

        public InIManager(string iniFilePath, int bufferSize)
        {
            IniFilePath = iniFilePath;
            BufferSize = bufferSize;
            lpReturnedString = new StringBuilder(BufferSize);
        }
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, $"{System.Environment.CurrentDirectory}\\{IniFilePath}");
        }

        public string IniReadValue(string Section, string Key)
        {
            lpReturnedString.Clear();
            int i = GetPrivateProfileString(Section, Key, "", lpReturnedString, BufferSize,
                $"{System.Environment.CurrentDirectory}\\{IniFilePath}");
            if (i.Equals(0))
            {
                return @"NotFound";
            }
            return lpReturnedString.ToString();
        }
    }
}
