using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Diagnostics;

namespace Launcher
{
    public class Launcher
    {
        #region Win32 Wrappers
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallback Adress, int y);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        List<string> tabTitles = new List<string>();
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        public static extern bool BringWindowToTop(IntPtr hWnd);



        [DllImport("USER32.DLL")]
        public static extern bool SetWindowPos(
              IntPtr hWnd,
              IntPtr hWndInsertAfter,
              int X,
              int Y,
              int cx,
              int cy,
              uint uFlags
            );
        #endregion


        public List<Process> selectedProcs;
        public bool winFound = false;
        public int searchTry = 0;
        public List<string> items = new List<string>();


        public void launchTCS()
        {
            items.Add("Launching TCS DeskHelp");
            string strCmdText = "\"" + @"C:\Program Files (x86)\Google\Chrome\Application\chrome_proxy.exe" + "\"";
            strCmdText = strCmdText + " --profile-directory=Default --app-id=andoncijbhadleiidjpjdcdjhnainali";
            Console.WriteLine("Launching process...");
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + strCmdText);
            Process process = new Process();
            process.StartInfo = procStartInfo;
            process.Start();

        }

        public bool ActivateSelectedWindow()
        {
            bool result = false;
            items.Add("========Found TCS Windows========================");
            if (selectedProcs.Count == 0)
            {
                launchTCS();
                return true;
            }
            foreach (var sp in selectedProcs)
            {
                try
                {
                    if (sp.MainWindowTitle.ToLower().Contains("tcs") &&
                        !sp.MainWindowTitle.ToLower().Contains("google chrome"))
                    {
                        BringWindowToTop(sp.MainWindowHandle);
                        items.Add(sp.MainWindowTitle + " " + sp.MainWindowHandle.ToString());
                        result = true;
                        break;
                    }
                    else
                    {
                        //kill process
                        //sp.Kill();
                        result = false;
                        launchTCS();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string s = "";
                    result = false;
                }

            }
            return result;
        }

        public void GetByAutomation()
        {
            searchTry = 0;
            selectedProcs = new List<Process>();
            winFound = false;
            items.Clear();
            showTabTitles();
        }
        public void btnShowTabTitles_Click(object sender, EventArgs e)
        {
            searchTry = 0;
            selectedProcs = new List<Process>();
            winFound = false;
            items.Clear();
            showTabTitles();

        }

        public void showTabTitles()
        {
            //Grab all the Chrome processes
            Process[] chromeProcesses = Process.GetProcessesByName("chrome");

            //Chrome process not found
            if ((chromeProcesses.Length == 0))
            {
                items.Add("Chrome isn\'t open?");
                return;
            }

            //Clear our array of tab titles
            tabTitles = new List<string>();

            //Kick off our search for chrome tab titles
            EnumWindowsCallback callBackFn = new EnumWindowsCallback(Enumerator);
            EnumWindows(callBackFn, 0);

            //Add to our listbox
            items.AddRange(tabTitles.ToArray());

            if (searchTry == 0 && !ActivateSelectedWindow())
            {
                btnShowTabTitles_Click(null, null);
            }
            searchTry++;

        }

        //Enums through all visible windows - gets each chrome handle
        public bool Enumerator(IntPtr hwnd, int lParam)
        {

            if (IsWindowVisible(hwnd))
            {
                StringBuilder sClassName = new StringBuilder(256);
                uint processID = 0;
                GetWindowThreadProcessId(hwnd, out processID);
                Process processFromID = Process.GetProcessById((int)processID);
                GetClassName(hwnd, sClassName, sClassName.Capacity);

                //Only want visible chrome windows (not any electron type apps that have chrome embedded!)
                //if (((sClassName.ToString() == "Chrome_WidgetWin_1") && (processFromID.ProcessName == "chrome")))
                if (processFromID.ProcessName == "chrome")
                {
                    //if (!FindChromeTabs(hwnd) && processFromID.MainWindowTitle.ToLower().Contains("tcs"))
                    if (!FindChromeTabs(hwnd))
                    {
                        items.Add("Possible candidate window: [" + processFromID.MainWindowTitle + "] " + processFromID.MainWindowHandle.ToString());
                        selectedProcs.Add(processFromID);
                    }

                }
                string s = processFromID.MainWindowTitle;

            }

            return true;
        }

        //Takes chrome window handle, searches for tabstrip then gets tab titles
        public bool FindChromeTabs(IntPtr hwnd)
        {
            //To find the tabs we first need to locate something reliable - the 'New Tab' button
            AutomationElement rootElement = AutomationElement.FromHandle(hwnd);


            Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, "New Tab");

            //Find the 'new tab' button
            AutomationElement elemNewTab = rootElement.FindFirst(TreeScope.Descendants, condNewTab);

            //No tabstrip found
            if ((elemNewTab == null))
            {
                //tabTitles.Add(hwnd.ToString() + " Possible TCS handle");
                AutomationElement elm = AutomationElement.FromHandle(hwnd);
                AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.NameProperty, "Address and searchbar"));

                // if it can be found, get the value from the URL bar
                if (elmUrlBar != null)
                {
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                    if (patterns.Length > 0)
                    {
                        ValuePattern val =
                        (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
                        Console.WriteLine("Chrome URL found: " + val.Current.Value);
                    }
                }
                return false;
            }


            //Get the tabstrip by getting the parent of the 'new tab' button
            TreeWalker tWalker = TreeWalker.ControlViewWalker;
            AutomationElement elemTabStrip = tWalker.GetParent(elemNewTab);

            //Loop through all the tabs and get the names which is the page title
            Condition tabItemCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
            foreach (AutomationElement tabItem in elemTabStrip.FindAll(TreeScope.Children, tabItemCondition))
            {
                tabTitles.Add(tabItem.Current.Name.ToString());
            }
            return true;
        }

        public void getWinTitles()
        {
            items.Clear();
            selectedProcs = new List<Process>();

            Process[] chromeProcesses = Process.GetProcessesByName("chrome");
            foreach (var p in chromeProcesses)
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    items.Add(p.MainWindowTitle + " " + p.MainWindowHandle.ToString());

            selectedProcs = chromeProcesses.Where(x => x.MainWindowTitle.ToLower().Contains("tcs")).ToList();


            if (searchTry == 0 && !ActivateSelectedWindow())
            {
                searchTry++;
                getWinTitles();
            }
            if (selectedProcs.Count == 0) items.Add("NO TCS DeskHelp windows found");
        }


        public void GetWinTitles()
        {
            searchTry = 0;
            getWinTitles();
        }
        public void btnGetWinTitles_Click(object sender, EventArgs e)
        {
            searchTry = 0;
            getWinTitles();
        }


        public Launcher()
        {

        }


    }
}
