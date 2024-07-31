using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QM_RemoveProjectCommand
{
    public class RemoveProjectCommand
    {

        public static ProjectProcessor Processor { get; private set; } = new ProjectProcessor();
        public static string CommandName { get; set; } = "remove-project";


        private static void Write(string text)
        {
            DevConsoleUI.Instance.PrintText(text);
        }

        public static string Help(string command, bool verbose)
        {
            Write("Removes an armor or weapon project from a save");
            return "";
        }

        /// <summary>
        /// The project and save to remove the project from.
        /// </summary>
        /// <param name="tokens">Save slot number (0-2) and project id.</param>
        /// <returns></returns>
        public static string Execute(string[] tokens)
        {
            try
            {

                if (tokens.Length != 2)
                {
                    return $"Expected <save number> <projectId>";
                }

                if (!int.TryParse(tokens[0], out int slotNumber))
                {
                    return $"Slot number must be a number";
                }

                return Processor.RemoveProject(slotNumber, tokens[1].Trim()) ? "Success" : "Failed";

            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                return $"Command failed.  See Player.log for more information.  {ex.Message}";
            }
        }

        /// <summary>
        /// Returns the list of projects in the file if the slot is saved and then tab is pressed.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
        {

            try
            {
                if (tokens.Length == 1)
                {
                    int slotNumber;

                    if (!int.TryParse(tokens[0], out slotNumber)) return new List<string>();


                    List<string> projectIds = Processor.GetProjectIds(slotNumber);

                    if (projectIds.Count == 0)
                    {
                        return new List<string>() { $"No projects found in save number {slotNumber}" };
                    }

                    List<string> text = new List<string>();

                    foreach (string projectId in projectIds)
                    {
                        text.Add($"{command} {slotNumber} {projectId}");
                    }

                    return text;
                }
            }
            catch (Exception ex)
            {
                Write($"Auto complete failure: {ex.Message}");
                Debug.Log(ex);
            }

            return new List<string>();
        }

        public static bool IsAvailable()
        {
            return SingletonMonoBehaviour<MainMenuUI>.Instance?.isActiveAndEnabled ?? false;
        }

        public static bool ShowInHelpAndAutocomplete()
        {
            return true;
        }
    }
}
