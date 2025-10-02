using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using TaskTrackerCLI.Enums;
using TaskTrackerCLI.Model;

namespace TaskTrackerCLI
{
    public class Program
    {
        public static void Main()
        {
            List<TaskModel> tasks = new List<TaskModel>();
            string[] instructions = { "add", "update", "delete", "list", "mark", "help", "quit" };
            string pattern = @"\""(.*?)\"""; // Pattern for description

            // Load file
            string currentDir = Directory.GetCurrentDirectory();
            string path = Path.Combine(currentDir, "TasksFile.json");
            string fileContent = string.Empty;

            if (File.Exists(path)) 
            { 
                fileContent = File.ReadAllText(path);
                tasks = JsonConvert.DeserializeObject<List<TaskModel>>(fileContent) ?? new List<TaskModel>();
            }
            else
            {
                File.Create(path).Close();
            }

            // Open Intro
            Intro();

            // Read and execute inputs
            string param = string.Empty;
            string instruction = string.Empty;
            string inputs = string.Empty;

            while (instruction.ToLower() != "quit")
            {
                Console.Write("task-cli > ");
                param = Console.ReadLine();
                int firstSpaceIndex = param.IndexOf(' ');
                if(firstSpaceIndex != -1)
                {
                    instruction = param.Substring(0, firstSpaceIndex).ToLower();
                    inputs = param.Substring(firstSpaceIndex + 1);
                }
                else
                {
                    instruction = param.ToLower();
                    inputs = string.Empty;
                }

                if (!instructions.Contains(instruction)) 
                {
                    ErrorCommand();
                }

                // Check the latest id
                int lastId = 0;
                if (tasks.Count > 0)
                    lastId = tasks.LastOrDefault().Id;

                // Execute job
                if (instruction == "add")
                {
                    Match descMatch = Regex.Match(inputs, pattern);
                    if (descMatch.Success)
                    {
                        TaskModel task = new TaskModel
                        {
                            Id = lastId + 1,
                            Description = descMatch.Groups[1].Value,
                            Status = Enums.TaskStatus.ToDo,
                            CreatedAt = DateTime.Now
                        };
                        tasks.Add(task);
                        SaveToFile(tasks, path);
                        Console.WriteLine($"Task ID: {task.Id}. Successfully added!");
                    }
                    else
                    {
                        ErrorNoQuotes();
                    }
                }
                else if (instruction == "update")
                {
                    int inputSpaceIndex = inputs.IndexOf(" ");
                    if (inputSpaceIndex != -1) 
                    {
                        string updId = inputs.Substring(0, inputSpaceIndex);

                        Match descMatch = Regex.Match(inputs, pattern);
                        if (descMatch.Success) 
                        {
                            string desc = descMatch.Groups[1].Value;
                            int updatedId = 0;
                            try
                            {
                                int.TryParse(updId, out updatedId);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            var idxTask = tasks.FindIndex(x => x.Id == updatedId);
                            if (idxTask == -1)
                            {
                                Console.WriteLine("Task not found!");
                            }
                            else
                            {
                                tasks[idxTask].Description = desc;
                                tasks[idxTask].UpdatedAt = DateTime.Now;
                                SaveToFile(tasks, path);
                                Console.WriteLine($"Task ID: {updatedId}. Successfully updated!");
                            }
                        }
                        else
                        {
                            ErrorNoQuotes();
                        }
                    }
                    else
                    {
                        ErrorCommand();
                    }
                }
                else if (instruction == "delete")
                {
                    int deletedId = 0;
                    try
                    {
                        int.TryParse(inputs, out deletedId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                    int idxDeleted = tasks.FindIndex(x => x.Id == deletedId);
                    if (idxDeleted == -1) 
                    {
                        Console.WriteLine("Task not found!");
                    }
                    else
                    {
                        tasks.RemoveAt(idxDeleted);
                        SaveToFile(tasks, path);
                        Console.WriteLine($"Task ID: {deletedId}. Successfully deleted!");
                    }
                }
                else if (instruction == "list")
                {
                    int status = 0;

                    if (!string.IsNullOrEmpty(inputs))
                        int.TryParse(inputs, out status);

                    ListTasks(tasks, status);
                }
                else if (instruction == "mark")
                {

                }
                else if (instruction == "help")
                {
                    Intro();
                }
                else { }
            }
            Console.WriteLine("Exiting program! Press enter to continue!");
            Console.Read();
        }

        public static void Intro()
        {
            string intro = "==== Welcome to Task Tracker CLI v1.0 ====\n" +
                "| 1. To add task, use command \"add [\"Task Name\"]\" (ex: add \"Buy groceries\")\n" +
                "| 2. To update task, use command \"update [ID] [\"Task Name\"] (ex: update 1 \"Buy groceries and cook dinner\")\n" +
                "| 3. To delete task, use command \"delete [ID] (ex: delete 1)\n" +
                "| 4. To mark task status, use command \"mark [Status] [ID] (Status: ToDo(1), InProgress(2), Done(3)) (ex: mark 2 1)\n" +
                "| 5. To list tasks, use command \"list [Status]\" (status is optional filter) (ex: list 3)\n" +
                "| To bring this instructions, use command \"help\".\n" +
                "=====================================================\n\n";

            Console.WriteLine(intro);
        }

        public static void SaveToFile(List<TaskModel> o, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(o));
        }

        public static void ListTasks(List<TaskModel> tasks, int status)
        {
            Console.WriteLine("-----------------------------------");
            if (status == 0)
            {
                foreach(var task in tasks)
                {
                    Console.WriteLine($"ID\t\t: {task.Id}");
                    Console.WriteLine($"Description\t: {task.Description}");
                    Console.WriteLine($"Status\t\t: {task.Status.ToString()}");
                    Console.WriteLine("-----------------------------------");
                }
            }
            else
            {
                Enums.TaskStatus stat = (Enums.TaskStatus)status;
                foreach (var task in tasks.Where(x => x.Status == stat))
                {
                    Console.WriteLine($"ID\t\t: {task.Id}");
                    Console.WriteLine($"Description\t: {task.Description}");
                    Console.WriteLine($"Status\t\t: {task.Status.ToString()}");
                    Console.WriteLine("-----------------------------------");
                }
            }
        }

        public static void ErrorCommand()
        {
            Console.WriteLine("Incorrect command, please use command \"help\" for more detail.\n");
        }

        public static void ErrorNoQuotes()
        {
            Console.WriteLine("Please use double qoutes for the description!");
        }
    }
}
