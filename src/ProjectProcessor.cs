using MGSC;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics.Contracts;

namespace QM_RemoveProjectCommand
{

    public class ProjectProcessor
    {
        /// <summary>
        /// The projects that can be removed.
        /// </summary>
        private HashSet<string> AllowedProjectTypes { get; set; } =
            new HashSet<string>()
            {

                MagnumProjectType.Helmet.ToString(),
                MagnumProjectType.Leggings.ToString(),
                MagnumProjectType.Armor.ToString(),
                MagnumProjectType.Boots.ToString(),
                MagnumProjectType.MeleeWeapon.ToString(),
                MagnumProjectType.RangeWeapon.ToString(),
            };

        /// <summary>
        /// Removes a project from a save file and reverts all items that used that custom
        /// item back to the original versions.
        /// </summary>
        /// <param name="slotNumber">Save number.  Starts at zero.</param>
        /// <param name="targetProjectId">The id of the project.  Ex: army_knife</param>
        /// <returns></returns>
        public bool RemoveProject(int slotNumber, string targetProjectId)
        {

            try
            {
                if (!Plugin.SaveManager.IsSaveExist(slotNumber))
                {

                    DevConsoleUI.Instance.PrintText($"No save exists for slot {slotNumber}");
                    return false;
                }

                if (!GetProject(
                    targetProjectId,
                    slotNumber,
                    out string slotFileName,
                    out JObject doc,
                    out JObject project)) return false;

                if (!CreateBackupZip(slotNumber)) return false;

                //Remove the project and replace the custom item with a regular item.
                project.Remove();
                string json = JsonConvert.SerializeObject(doc);
                json = json.Replace($@"""{targetProjectId}_custom""", $@"""{targetProjectId}""");

                File.WriteAllText(Path.Combine(Application.persistentDataPath,slotFileName), json);

                //---- Replace any items that are on the floor in a dungeon.
                ReplaceDungeonItems(slotNumber, targetProjectId);


                return true;
            }
            catch (Exception ex)
            {
                DevConsoleUI.Instance.PrintText("Error Removing the project.  See Player.log for more info");
                DevConsoleUI.Instance.PrintText(ex.Message);
                Debug.LogError(ex);
            }

            return false;
        }

        /// <summary>
        /// Replacement for items on the dungeon floors
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <param name="targetProjectId"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ReplaceDungeonItems(int slotNumber, string targetProjectId)
        {
            //Only the dungeon data, not the decals.
            List<string> dungeonFiles = Directory.GetFiles(Application.persistentDataPath, $"slot_{slotNumber}_dung_*.dat")
                .Where(x => !x.EndsWith("_decals.dat"))
                .ToList();

            foreach (string file in dungeonFiles)
            {
                string newText = File.ReadAllText(file);
                newText = newText.Replace($"\"{targetProjectId}_custom\"", $"\"{targetProjectId}\"");

                File.WriteAllText(file,newText);
            }
        }


        /// <summary>
        /// Returns the list of item strings for each project in the save slot.
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <returns></returns>
        public List<string> GetProjectIds(int slotNumber)
        {
            return GetProjects(slotNumber, out _, out _)
                .Select(x => (string)x["DevelopId"])
                .ToList();

        }

        /// <summary>
        /// Gets the list of projects nodes that are in the save slot.
        /// Note - this writes messages direct to the console
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <param name="slotFileName"></param>
        /// <param name="doc"></param>
        /// <returns>The projects' JSON nodes.  Null on error</returns>
        public List<JObject> GetProjects(int slotNumber, out string slotFileName,
            out JObject doc)
        {
            slotFileName = null;
            doc = null;

            slotFileName = $"slot_{slotNumber}_session.dat";

            string slotFilePath = Path.Combine(Application.persistentDataPath, slotFileName);

            //Deserialize
            doc = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(slotFilePath));

            //Get the list of projects 
            List<JObject> projects = doc
                ["Components"]
                .FirstOrDefault(x => x.Value<string>("Type") == "MGSC.MagnumProjects")
                ?["Content"]
                ?["Values"]
                ?.Values<JObject>()
                ?.Where(x => AllowedProjectTypes.Contains((string)x["ProjectType"]))
                ?.ToList();

            if (projects == null)
            {
                DevConsoleUI.Instance.PrintText("Save file format has changed. Unable to find the list of projects. This mod will need to be updated");
                return null;
            }

            return projects;
        }

        /// <summary>
        /// Gets a project by id.  
        /// Note - this writes messages direct to the console
        /// </summary>
        /// <param name="targetProjectId"></param>
        /// <param name="slotNumber"></param>
        /// <param name="slotFileName"></param>
        /// <param name="doc"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        private bool GetProject(string targetProjectId, int slotNumber, out string slotFileName,
            out JObject doc,
            out JObject project)
        {
            List<JObject> projects = GetProjects(slotNumber, out slotFileName, out doc);

            project = projects
                .FirstOrDefault(x => (string)x["DevelopId"] == targetProjectId);

            if (project == null)
            {
                DevConsoleUI.Instance.PrintText($"Unable to find project '{targetProjectId}'");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Creates a backup file in the format of "slot_number_date.zip"
        /// Note - this writes directly to the console and returns false on error.
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <returns></returns>
        private bool CreateBackupZip(int slotNumber)
        {
            try
            {
                string backupZipPath = Path.Combine(Application.persistentDataPath,
                    $"slot_{slotNumber}_{DateTime.Now.ToString("yyyyMMdd_HHmmss_")}{DateTime.Now.Millisecond.ToString("000")}.zip");

                DevConsoleUI.Instance.PrintText($"Backing up save slot files to '{backupZipPath}'");

                FileInfo[] files = new DirectoryInfo(Application.persistentDataPath)
                    .GetFiles($"slot_{slotNumber}*");

                //Remove any previous archives.
                files = files
                    .Where(x => x.Extension != ".zip")
                    .ToArray();

                using (ZipArchive zipArchive = ZipFile.Open(backupZipPath, ZipArchiveMode.Create))
                {
                    foreach (FileInfo file in files)
                    {
                        zipArchive.CreateEntryFromFile(file.FullName,
                            file.Name);
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                DevConsoleUI.Instance.PrintText($"Error: Unable to create zip backup file.  See Player.log for more info.");
                return false;
            }
        }

    }
}
