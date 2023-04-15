using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace unturned_hwid_unbanner
{
    internal class Program
    {
        private static Random random = new Random();
        static void Main(string[] args)
        {
            bool exceptionThrown = false;
            Console.WriteLine("epic unturned hwid unbanner software for bad boys\nby Coopyy#0001\n");
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("please run as administrator");
                Console.ReadKey();
            }
            try
            {
                Console.WriteLine("modifying registry guid...");
                var guidkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Cryptography", true);
                string oldg = guidkey.GetValue("MachineGuid") as string;
                string newg = GetNewGUID(oldg, out bool fits);
                if (!fits)
                    Console.Write("your current hwid is either zero'd or doesn't fit hwid pattern. result may be incorrect");
                guidkey.SetValue("MachineGuid", newg);
                Console.WriteLine($"{oldg} -> {newg}\ndone.");
            }
            catch (Exception ex) { Console.WriteLine("something blew up when trying to change guid\n" + ex.ToString()); exceptionThrown = true; }

            try
            {
                Console.WriteLine("\nmodifying registry game cloud hash...");
                var cloudkey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Smartly Dressed Games\\Unturned", true);
                byte[] oldc = cloudkey.GetValue("CloudStorageHash_h1878083263") as byte[];
                string asString = "";
                foreach (var item in oldc)
                {
                    if (item != 0) // is null terminated
                        asString += (char)item;
                }
                string newc = RandomString(asString.Length);
                List<byte> asBytes = new List<byte>();
                foreach (var item in newc)
                    asBytes.Add((byte)item);
                asBytes.Add(0); // null terminator
                cloudkey.SetValue("CloudStorageHash_h1878083263", asBytes.ToArray());
                Console.WriteLine($"{asString} -> {newc}\ndone.");
            }
            catch (Exception ex) { Console.WriteLine("something blew up when trying to change cloud registry\n" + ex.ToString()); exceptionThrown = true; }

            try
            {
                Console.WriteLine("\nmodifying game stored info...");
                bool didChangeAnything = false;
                string dir = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 304930", false).GetValue("InstallLocation") as string;
                if (File.Exists(dir + "\\Cloud\\ConvenientSavedata.json"))
                {
                    funkyclass c = JsonConvert.DeserializeObject<funkyclass>(File.ReadAllText(dir + "\\Cloud\\ConvenientSavedata.json"));
                    if (c.Strings.TryGetValue("ItemStoreCache", out string str))
                    {
                        string newcache = RandomString(str.Length);
                        c.Strings["ItemStoreCache"] = newcache;
                        File.WriteAllText(dir + "\\Cloud\\ConvenientSavedata.json", JsonConvert.SerializeObject(c, Formatting.Indented));
                        didChangeAnything = true;
                        Console.WriteLine($"{str} -> {newcache}\ndone.\n");
                    }
                }
                if (!didChangeAnything)
                    Console.WriteLine("no changes needed\ndone.\n");
            }
            catch (Exception ex) { Console.WriteLine("something blew up when trying to change json\nthis might is an issue if the file doesn't exist\n" + ex.ToString()); exceptionThrown = true; }
            if (!exceptionThrown)
                Console.WriteLine("youre all spoofed, now you just need a VPN and new account\npress any key or close");
            Console.ReadKey();
        }


        private static string GetNewGUID(string old, out bool fitsPattern)
        {
            fitsPattern = false;
            if (!String.IsNullOrEmpty(old))
            {
                string[] split = old.Split('-');
                if (split.Length > 0)
                {
                    split[0] = RandomString(split[0].Length);
                    fitsPattern = true;
                    return string.Join("-", split);
                }
            }
            return RandomString(36);
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdef0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class funkyclass
    {
        public Dictionary<string, string> Strings = new Dictionary<string, string>();

        public Dictionary<string, DateTime> DateTimes = new Dictionary<string, DateTime>();

        public Dictionary<string, bool> Booleans = new Dictionary<string, bool>();

        public Dictionary<string, long> Integers = new Dictionary<string, long>();

        public HashSet<string> Flags = new HashSet<string>();
    }
}
