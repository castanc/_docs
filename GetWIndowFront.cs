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

namespace ChromeTabTitles
{
    public class BringWindowFront
    {
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
        public static extern bool SetWindowPos(
              IntPtr hWnd,
              IntPtr hWndInsertAfter,
              int X,
              int Y,
              int cx,
              int cy,
              uint uFlags
            );

        List<IntPtr> selectedProcess;


        public IntPtr LastHandle;

        public static void bringToFront(string title)
        {
            // Get a handle to the application.
            IntPtr handle = FindWindow(null, title);

            // Verify that it is a running process.
            if (handle == IntPtr.Zero)
            {
                return;
            }

            // Make it the foreground application
            SetForegroundWindow(handle);
        }



        private void btnShowTabTitles_Click(object sender, EventArgs e)
        {
            //lstTabTitles.Items.Clear();
            showTabTitles();
        }

        public void showTabTitles()
        {
            //Grab all the Chrome processes
            Process[] chromeProcesses = Process.GetProcessesByName("chrome");

            //Chrome process not found
            if ((chromeProcesses.Length == 0))
            {
                //lstTabTitles.Items.Add("Chrome isn\'t open?");
                //btnShowTabTitles.Enabled = false;
                return;
            }

            //Clear our array of tab titles
            tabTitles = new List<string>();

            //Kick off our search for chrome tab titles
            selectedProcess = new List<IntPtr>();
            EnumWindowsCallback callBackFn = new EnumWindowsCallback(Enumerator);
            EnumWindows(callBackFn, 0);

            //Add to our listbox
            //lstTabTitles.Items.AddRange(tabTitles.ToArray());

            /*
            foreach(var it in tabTitles)
            {
                if ( it.ToLower().Contains("tcs"))
                {
                    bringToFront(it);
                    break;
                }
            }
            */
        }

        //Enums through all visible windows - gets each chrome handle
        private bool Enumerator(IntPtr hwnd, int lParam)
        {
            if (IsWindowVisible(hwnd))
            {
                StringBuilder sClassName = new StringBuilder(256);
                uint processID = 0;
                GetWindowThreadProcessId(hwnd, out processID);
                Process processFromID = Process.GetProcessById((int)processID);
                GetClassName(hwnd, sClassName, sClassName.Capacity);

                //Only want visible chrome windows (not any electron type apps that have chrome embedded!)
                if (((sClassName.ToString() == "Chrome_WidgetWin_1") && (processFromID.ProcessName == "chrome")))
                {
                    if (!FindChromeTabs(hwnd))
                        selectedProcess.Add(hwnd);
                }

            }

            return true;
        }

        //Takes chrome window handle, searches for tabstrip then gets tab titles
        private bool FindChromeTabs(IntPtr hwnd)
        {
            //To find the tabs we first need to locate something reliable - the 'New Tab' button
            AutomationElement rootElement = AutomationElement.FromHandle(hwnd);


            Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, "New Tab");

            //Find the 'new tab' button
            AutomationElement elemNewTab = rootElement.FindFirst(TreeScope.Descendants, condNewTab);

            //No tabstrip found
            if ((elemNewTab == null))
            {
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

    }
}
