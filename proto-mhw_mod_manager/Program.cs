using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Logging;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace proto_mhw_mod_manager // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static string DOWNLOAD_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\downloads\\..og";
        static string INSTALL_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods";
        static string NATIVEPC_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\gamefolder";
        static string MASTER_LIST_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods\\masterlist.txt";

        static string BACKUP_MASTER_LIST_PATH = "C:\\Users\\Luis\\Desktop\\mhr modding\\mods\\backup_masterlist.txt";


        static public List<string> MasterListInMemory = new List<string>();
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
                "1- read masterList\n" +
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
                    readMasterList();
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
            Console.Clear();
            Console.WriteLine("[MAIN Files] Select the nativePC folder containing the main files"); //check path for the main files of the mod
            string path = pathselection();
            path = path.Replace("\\nativePC", "");
            if (!Directory.Exists(path + "\\nativePC"))
            {
                Console.WriteLine("nativePC not found, install failed...");
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

            Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName +"\\mainFiles"); //create the main folder and the main files and extra files folder
            Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName +"\\extraFiles");

            
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(path, INSTALL_PATH + "\\" + folderName + "\\mainFiles", UIOption.AllDialogs);
            Console.WriteLine("Installed main mod [" + folderName + "] in the folder");
            MasterListInMemory.Insert(0,folderName);

            Console.WriteLine("Do you want to install extra file for the mod? [y,n]");
            if(Console.ReadLine().ToLower() == "n")
            {
                Console.WriteLine("\nEnding install...");
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine(folderName);
                Console.WriteLine("[EXTRA Files] Now select the complete path of the folder with the nativePC of the extra files, if theres none just press enter.");
                path = pathselection();
                path = path.Replace("\\nativePC", "");
                if (!Directory.Exists(path + "\\nativePC"))
                {
                    Console.WriteLine("\nnativePC not found, Ending install...");
                    return;
                }

                string extrafolderName = path.Split('\\').Last();  //get the name of the folder
                Directory.CreateDirectory(INSTALL_PATH + "\\" + folderName + "\\extraFiles\\" + extrafolderName);

                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(path, INSTALL_PATH + "\\" + folderName + "\\extraFiles\\" + extrafolderName, UIOption.AllDialogs);
                Console.WriteLine("Installed extra :" + extrafolderName + " in the extrafolder");
                Console.WriteLine("Do you want to install extra files for the mod? [y,n]");
                if (Console.ReadLine().ToLower() == "n")
                {
                    Console.WriteLine("\nEnding install...");
                    return;
                }
            }
        }

        static void deleteMod ()
        {
            while(true)
            {
                Console.Clear();
                printMasterList();

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
            for(int i = 0; i < MasterListInMemory.Count; i++)
            {
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(INSTALL_PATH + "\\" + MasterListInMemory[i]+ "\\mainFiles", NATIVEPC_PATH, UIOption.AllDialogs); //copy the main files first
                Console.WriteLine("[MAIN]" + INSTALL_PATH + "\\" + MasterListInMemory[i] + "\\mainFiles");
                int extraFilesAmount = Directory.GetDirectories(INSTALL_PATH + "\\" + MasterListInMemory[i] + "\\extraFiles").Length; //check if theres extra files, then copy one by one
                if (extraFilesAmount > 0)
                {
                    for(int j = 0; j < extraFilesAmount; j++)
                    {
                        string temp = Directory.GetDirectories(INSTALL_PATH + "\\" + MasterListInMemory[i] + "\\extraFiles")[j];
                        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(temp, NATIVEPC_PATH, UIOption.AllDialogs);
                        Console.WriteLine("[EXTRA]"+temp);
                        Console.WriteLine("a");
                    }

                }
            }
            Console.WriteLine("press enter to continue...");
            Console.ReadLine();
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