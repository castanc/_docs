                 string strCmdText = "\"" + @"C:\Program Files (x86)\Google\Chrome\Application\chrome_proxy.exe" + "\"";
                /trCmdText = strCmdText + " --profile-directory=Default --app-id=andoncijbhadleiidjpjdcdjhnainali";
                Console.WriteLine("Launching process...");
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + commandLine);
                Process process = new Process();
                process.StartInfo = procStartInfo;
                process.Start();
