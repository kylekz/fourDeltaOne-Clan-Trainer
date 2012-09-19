using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace fourDeltaOne_Trainer
{
    static class Program
    {
        public static Dictionary<string, string> Settings;
        public static string SettingsPath = "settings.txt";
        private static string ClanTag = "";
        private static string TitleText = "";
        private static int TitleNumber = 74;
        private static string Blank = "255";

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [STAThread]
        static void Main(string[] args)
        {
            ArgConsole(false);
            string a = "";
            for (int i = 0; i < args.Length; i++)
            {
                a = args[i];
            }

            switch (a)
            {
                case "-console":
                    ArgConsole(false);
                    break;
            }
        }

        private static void ArgConsole(Boolean val) {
            if (false)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                LoadConfig();

                SetClanTag();
                SetBlankTitle();
                SetTitleNumber();
                SetTitleText();
            }
        }

        private static byte[] GetBytes(string str)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(str);
            return bytes;
        }

        private static void SetClanTag()
        {
            var iw5m = Process.GetProcessesByName("iw5m.dat").FirstOrDefault();

            // Enable clan tag
            WriteMemory(iw5m, 0x01328D33, new byte[] { 255 });
            // Set tag
            byte[] tag = GetBytes(ClanTag);
            Console.WriteLine("Tag:" + BitConverter.ToString(tag).Replace("-", ""));
            WriteMemory(iw5m, 0x01328D54, tag);
        }

        private static void SetBlankTitle()
        {
            var iw5m = Process.GetProcessesByName("iw5m.dat").FirstOrDefault();

            // Set FNG title
            byte[] t = GetBytes(Blank);
            WriteMemory(iw5m, 0x01328D34, t);
        }

        private static void SetTitleNumber()
        {
            var iw5m = Process.GetProcessesByName("iw5m.dat").FirstOrDefault();

            // Set title number
            TitleNumber += 12;
            byte v;
            byte.TryParse(TitleNumber.ToString(), out v);
            byte[] value = new byte[] { v };
            Console.WriteLine("TitleNum:" + BitConverter.ToString(value).Replace("-", ""));
            WriteMemory(iw5m, 0x01328D50, value);
        }

        private static void SetTitleText()
        {
            var iw5m = Process.GetProcessesByName("iw5m.dat").FirstOrDefault();

            // Set title text
            byte[] text = GetBytes(TitleText);
            Console.WriteLine("TitleText:" + BitConverter.ToString(text).Replace("-", ""));
            WriteMemory(iw5m, 0x01328D35, text);
        }

        public static void WriteMemory(Process p, int address, byte[] val)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)p.Id);

            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr(address), val, (UInt32)val.LongLength, out wtf);

            CloseHandle(hProc);
        }

        private static string GetSetting(string setting, string settingValue)
        {
            try
            {
                string ret = Settings[setting];
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                TextWriter t = new StreamWriter(SettingsPath, true);
                t.WriteLine(setting + "=" + settingValue);
                t.Close();
                Settings.Add(setting, settingValue);
                return settingValue;
            }
        }

        public static void LoadConfig()
        {
            Settings = new Dictionary<string, string>();
            if (!File.Exists(SettingsPath))
            {
                TextWriter writer = new StreamWriter(SettingsPath);
                writer.WriteLine("clantag=swag");
                writer.WriteLine("titletext=omg yolo");
                writer.WriteLine("titlenumber=74");
                writer.Close();
            }

            foreach (string str2 in File.ReadAllLines(SettingsPath))
            {
                Settings.Add(str2.Split(new char[] { '=' })[0], str2.Split(new char[] { '=' })[1]);
            }
            try
            {
                ClanTag = GetSetting("clantag", "swag");
                TitleText = GetSetting("titletext", "omg yolo");
                TitleNumber = int.Parse(GetSetting("titlenumber", "74"));
            }
            catch (NullReferenceException exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }
    }
}
