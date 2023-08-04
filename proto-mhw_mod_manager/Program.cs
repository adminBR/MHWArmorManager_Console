using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Logging;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Xml.Linq;

namespace proto_mhw_mod_manager // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static string DOWNLOAD_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\downloads\\..og";
        static string INSTALL_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods";
        static string NATIVEPC_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\gamefolder";
        static string MASTER_LIST_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods\\masterlist.txt";
        static string SYMLINK_LIST_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods\\symlinklist.txt";

        static string BACKUP_MASTER_LIST_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods\\backup_masterlist.txt";


        static public List<string> MasterListInMemory = new List<string>();
        static public List<String> GlobalFoldersList =new List<string>();
        static public List<String> GlobalFilesList = new List<string>();
        [STAThread]
        static void Main(string[] args)
        {
            while (true)
            {
                readMasterList();
                menu();
            }
        }

        static void menu()
        {
            Console.Clear();
            Console.WriteLine("Choose and option\n" +
                "1- reload masterList (check if the masterlist and the instaled mods are synced)\n" +
                "2- create masterList\n" +
                "3- install mod\n" +
                "4- delete mod\n" +
                "5- reposition mod\n" +
                "6- deploy mods\n");

            printMasterList();

            int sel = int.Parse(Console.ReadLine());

            switch (sel)
            {
                case 1:
                    reloadMasterList();
                    createMasterList();
                    break;
                case 2:
                    createMasterList();
                    break;
                case 3:
                    installMod();
                    createMasterList();
                    break;
                case 4:
                    deleteMod();
                    createMasterList();
                    break;
                case 5:
                    repositionMod();
                    createMasterList();
                    break;
                case 6:
                    deployMods();
                    break;
            }
        }
        static void reloadMasterList()
        {
            bool isErrorActive = false;
            Console.Clear();
            List<String> tempdir = Directory.GetDirectories(INSTALL_PATH).ToList();
            if(tempdir.Count < MasterListInMemory.Count)
            {
                Console.WriteLine("[ERROR] 'MasterList' has more mods than the 'Mods folder', one or more mods got removed from the game folder");
                List<String> toRemove = new List<String>();
                for (int i = 0; i < MasterListInMemory.Count; i++)
                {
                    bool success = false;
                    for (int j = 0; j < tempdir.Count; j++)
                    {
                        string name = tempdir[j].Split("\\mods\\")[1];
                        if (name == MasterListInMemory[i])
                        {
                            success = true;
                        }

                    }
                    if(success == false)
                    {
                        //Console.WriteLine("removed " + MasterListInMemory[i]);
                        toRemove.Add(MasterListInMemory[i]);
                    }
                }

                for(int i = 0; i < toRemove.Count; i++)
                {
                    Console.WriteLine("removed:: "+toRemove[i]);
                    MasterListInMemory.Remove(toRemove[i]);
                }
                isErrorActive = true;


            }else if (tempdir.Count > MasterListInMemory.Count)
            {
                Console.WriteLine("[ERROR] 'Mods folder' has more mods than the 'MasterList', one or more files got added externally, remove the following folders and install with the program:");
                for(int i = 0; i < tempdir.Count; i++)
                {
                    bool success = false;
                    for (int j = 0;j < MasterListInMemory.Count; j++)
                    {

                        string name = tempdir[i].Split("\\mods\\")[1];
                        if (name == MasterListInMemory[j])
                        {
                            success = true;
                            break;
                        }
                    }
                    if (success == false)
                    {
                        Console.WriteLine(tempdir[i]);
                    }
                }
                isErrorActive = true;
            }else if (tempdir.Count == MasterListInMemory.Count)
            {
                Console.WriteLine("[OK] No problems found... press enter to continue...");
                isErrorActive = false;
                
            }
            Console.ReadLine();
        }

        static void readMasterList()
        {
            if (!File.Exists(MASTER_LIST_PATH))
            {
                Console.WriteLine("No masterList Found");
                return;
            }
            else
            {
                Console.WriteLine("masterList found, reading...");
                MasterListInMemory = File.ReadAllLines(MASTER_LIST_PATH).ToList();
                for(int i = 0; i < MasterListInMemory.Count; i++)
                {
                    Console.WriteLine(i + ": " + MasterListInMemory[i]);
                }
            }
        }
        static void createMasterList()
        {
            if (File.Exists(MASTER_LIST_PATH))//managing the txt files, main and backup of the previous save
            {
                if (File.Exists(BACKUP_MASTER_LIST_PATH))
                {
                    File.Delete(BACKUP_MASTER_LIST_PATH);
                }
                Console.WriteLine("backup created...");
                File.Copy(MASTER_LIST_PATH, BACKUP_MASTER_LIST_PATH);
                File.Delete(MASTER_LIST_PATH);
            }
            FileStream fs = File.Create(MASTER_LIST_PATH);
            fs.Close(); //the create function opens a filestream that needs to be closed to be able to use the file again
            File.WriteAllLines(MASTER_LIST_PATH, MasterListInMemory);
            Console.WriteLine("Created masterlist with :"+MasterListInMemory.Count+" entries");
        }

        static void installMod()
        {
            GlobalFilesList = new List<String>();
            GlobalFoldersList = new List<String>();

            Console.Clear();
            Console.WriteLine("[MAIN Files] Select the nativePC folder containing the main files"); //check path for the main files of the mod
            string path = pathselection();
            path = path.Replace("\\nativePC", "");
            if (!Directory.Exists(path + "\\nativePC"))
            {
                finishInstal("", "nativePC not found, Ending install...", 0);
                return;
            }
            string folderName = path.Split('\\').Last();  //get the name of the folder

            if (MasterListInMemory.IndexOf(folderName) != -1) // check and remove entry in the master list if exists and then remove the folder
            {
                MasterListInMemory.RemoveAt(MasterListInMemory.IndexOf(folderName));
            }
            if(Directory.Exists(INSTALL_PATH + "\\" + folderName))
            {
                Directory.Delete(INSTALL_PATH + "\\" + folderName, true);
            }

            ///Creating folders and files and adding the entry to the master list
            ///
            Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName +"\\mainFiles"); //create the main folder and the main files and extra files folder
            Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName +"\\extraFiles");
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(path, INSTALL_PATH + "\\" + folderName + "\\mainFiles", UIOption.AllDialogs); //copy the files and add the entry in the masterlist
            Console.WriteLine("Installed main mod [" + folderName + "] in the folder");
            MasterListInMemory.Insert(0,folderName);

            ///Creating managing files for the future deploy
            ///
            Install_CreateFolderPathFile(new List<String>() { INSTALL_PATH + "\\" + folderName + "\\mainFiles\\nativePC" });
            FileStream fs = File.Create(INSTALL_PATH + "\\" + folderName + "\\FoldersList.txt");
            fs.Close(); //the create function opens a filestream that needs to be closed to be able to use the file again
            File.WriteAllLines(INSTALL_PATH + "\\" + folderName + "\\FoldersList.txt", GlobalFoldersList);

            Console.WriteLine("Do you want to install extra file for the mod? [y,n]");
            if(Console.ReadLine().ToLower() == "n")
            {

                finishInstal(INSTALL_PATH + "\\" + folderName, "Instalation complete...", 2);

                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine(folderName);
                Console.WriteLine("[EXTRA Files] Now select the complete path of the folder with the nativePC of the extra files, if theres none just press enter.");
                path = pathselection();
                path = path.Replace("\\nativePC", "");
                if (!Directory.Exists(path + "\\nativePC")) //check if the path is valid, if its not, end instalation 
                {
                    finishInstal(INSTALL_PATH + "\\" + folderName, "nativePC not found, Ending install...",1);
                    return;
                }

                string extrafolderName = path.Split('\\').Last();  //get the name of the folder
                Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName + "\\extraFiles\\" + extrafolderName);

                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(path, INSTALL_PATH + "\\" + folderName + "\\extraFiles\\" + extrafolderName, UIOption.AllDialogs);

                ///Creating managing files for the future deploy
                ///
                Install_CreateFolderPathFile(new List<String>() { INSTALL_PATH + "\\" + folderName + "\\extraFiles\\" + extrafolderName + "\\nativePC" });
                File.WriteAllLines(INSTALL_PATH + "\\" + folderName + "\\FoldersList.txt", GlobalFoldersList);

                Console.WriteLine("Installed extra :" + extrafolderName + " in the extrafolder");
                Console.WriteLine("Do you want to install extra files for the mod? [y,n]");
                if (Console.ReadLine().ToLower() == "n")
                {
                    finishInstal(INSTALL_PATH + "\\" + folderName, "Instalation complete...",2);
                    return;
                }
            }
        }

        static void finishInstal(string modPath, String endingMessage, int successLevel)
        {
            
            ///successLevel = 0 means complete fail, no files copied
            /// = 1 means partial fail, main files worked but failed to get extra files, same for level 2, letting this here for maybe future use of level 2
            if(successLevel == 0)
            {
                Console.WriteLine("\n" + endingMessage);
                Console.ReadLine();
            }
            else if (successLevel == 1 || successLevel == 2)
            {
                Install_CreateFilesPathFile();
                FileStream fs = File.Create(modPath + "\\FilesList.txt");
                fs.Close(); //the create function opens a filestream that needs to be closed to be able to use the file again
                File.WriteAllLines(modPath + "\\FilesList.txt", GlobalFilesList);
            }

        }

        static void deleteMod ()
        {
            while(true)
            {
                Console.Clear();
                printMasterList();

                if(MasterListInMemory.Count <= 0)
                {
                    Console.WriteLine("No Mods Installed... press enter to continue");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("\n[DELETE] Type the index of the mod you want to delete.");
                int tempindex = int.Parse(Console.ReadLine());
                string modname = MasterListInMemory[tempindex];
                MasterListInMemory.RemoveAt(tempindex);
                if(Directory.Exists(INSTALL_PATH + "\\" + modname))
                {
                    Directory.Delete(INSTALL_PATH + "\\" + modname, true);
                }


                Console.WriteLine("\n "+modname+" deleted...");

                Console.WriteLine("\n \n Do you want to delete another mod? [y,n]");
                if (Console.ReadLine().ToLower() == "n")
                {
                    return;
                }
            }

        }

        static void repositionMod()
        {
            while (true)
            {

                Console.Clear();
                printMasterList();

                Console.WriteLine("[REPOSITION] Type the index of the mod you want to change.");
                int sourceindex = int.Parse(Console.ReadLine());
                Console.WriteLine("[REPOSITION] Type the new position index. (The replaced mod will be under");
                int newindex = int.Parse(Console.ReadLine());

                string temp = MasterListInMemory[sourceindex];

                MasterListInMemory.RemoveAt(sourceindex);
                if (newindex > sourceindex)
                {
                    //newindex;
                }
                MasterListInMemory.Insert(newindex, temp);
                Console.WriteLine("\n new order: \n");

                printMasterList();

                Console.WriteLine("\n \n Do you want to reposition another mod? [y,n]");
                if (Console.ReadLine().ToLower() == "n")
                {
                    return;
                }

            }
        }

        static void deployMods()
        {
            if(MasterListInMemory.Count <= 0)
            {
                Console.WriteLine("Error, No mods installed, press enter to continue...");
                Console.ReadLine();
                return;
            }

            List<String> symlinkDeployed = new List<string>();
            if (File.Exists(SYMLINK_LIST_PATH))
            {
                symlinkDeployed = File.ReadAllLines(SYMLINK_LIST_PATH).ToList();
                if (symlinkDeployed.Count > 0)
                {
                    for (int i = 0; i < symlinkDeployed.Count(); i++)
                    {
                        if (File.Exists(symlinkDeployed[i]))
                        {
                            File.Delete(symlinkDeployed[i]);
                        }
                    }
                }
                File.WriteAllText(SYMLINK_LIST_PATH,"");
            }
            else
            {
                FileStream fs = File.Create(SYMLINK_LIST_PATH);
                fs.Close();
            }

            symlinkDeployed = new List<string>();
            for (int i = 0; i < MasterListInMemory.Count; i++)
            {
                //C:\Users\Luis\Desktop\mhr modding\mods\Skimpy Summer Diver - Main Files\mainFiles\nativePC\pl
                GlobalFoldersList = File.ReadAllLines(INSTALL_PATH + "\\" + MasterListInMemory[i]+ "\\FoldersList.txt").ToList();
                GlobalFilesList = File.ReadAllLines(INSTALL_PATH + "\\" + MasterListInMemory[i] + "\\FilesList.txt").ToList();
                for (int j = 0; j<GlobalFoldersList.Count; j++)
                {
                    string tempFolderPath = GlobalFoldersList[j].Split("nativePC")[1];
                    if (!Directory.Exists(NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath))
                    {
                        Directory.CreateDirectory(NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath);
                        Console.WriteLine("[NEWFOLDER] "+NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath);
                    }
                    if(!File.Exists(NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath + "\\__Folder_managed_by_Hunter_Chest__"))
                    {
                        File.Create(NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath + "\\__Folder_managed_by_Hunter_Chest__");
                        //symlinkDeployed.Add(NATIVEPC_PATH + "\\nativePC\\" + tempFolderPath + "\\__Folder_managed_by_Hunter_Chest__");
                    }
                }
                for (int j = 0; j < GlobalFilesList.Count; j++)
                {
                    string tempFilePath = GlobalFilesList[j].Split("nativePC")[1];
                    if (File.Exists(NATIVEPC_PATH + "\\nativePC\\" + tempFilePath))
                    {
                        File.Delete(NATIVEPC_PATH + "\\nativePC\\" + tempFilePath);
                    }
                    File.CreateSymbolicLink(NATIVEPC_PATH + "\\nativePC" + tempFilePath, GlobalFilesList[j]);
                    symlinkDeployed.Add(NATIVEPC_PATH + "\\nativePC" + tempFilePath);
                    Console.WriteLine("[NEWFILE] " + NATIVEPC_PATH + "\\nativePC" + tempFilePath);
                }

            }
            File.WriteAllLines(SYMLINK_LIST_PATH, symlinkDeployed);
            Console.WriteLine("Finished Deploy, press enter to continue...");
            Console.ReadLine();
        }


        static void Install_CreateFolderPathFile(List<String> rootPath)
        {
            if (rootPath.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < rootPath.Count; i++)
            {
                GlobalFoldersList.Add(rootPath[i]);
                List<String> subfolders = Directory.GetDirectories(rootPath[i]).ToList();

                if (subfolders.Count > 0) //check if has subfolders
                {
                    Install_CreateFolderPathFile(subfolders);
                }
            }
        }
        static void Install_CreateFilesPathFile()
        {
            if(GlobalFoldersList.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < GlobalFoldersList.Count; i++)
            {
                List<String> subfiles = Directory.GetFiles(GlobalFoldersList[i]).ToList();

                if (subfiles.Count > 0) //check if has subfolders
                {
                    for(int j = 0; j < subfiles.Count; j++)
                    {
                        GlobalFilesList.Add(subfiles[j]);
                    }
                }
            }
        }

        static void printMasterList()
        {
            Console.WriteLine("Installed Mods:");
            for (int i = 0; i < MasterListInMemory.Count; i++)
            {
                Console.WriteLine(i + "- " + MasterListInMemory[i]);
            }
        }

        static string pathselection()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(fbd.SelectedPath);
                return fbd.SelectedPath;
            }
            else
            {
                return "error";
            }
        }

    }
}