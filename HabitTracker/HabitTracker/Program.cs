using Microsoft.Data.Sqlite;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;

namespace HabitTracker
{
    internal class Program
    {
        static string connectionString = @"Data Source=habit_tracker.db";
        static List<string> habitList = new List<string>();

        static bool runApp = true;
        static void Main(string[] args)
        {
            LoadExistingHabits();
            do
            {
                Menu();
            }while(runApp);
            
        }

        private static void LoadExistingHabits()
        {
            Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        habitList.Add(tableName);
                    }
                }
                connection.Close();
            }
            
        }

        private static void Menu()
        {
            Console.WriteLine("\n\nMAIN MENU");
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("\nType 0 to Close Application.");
            Console.WriteLine("Type 1 to create a Habit");
            Console.WriteLine("Type 2 to View All Records.");
            Console.WriteLine("Type 3 to Insert Record.");
            Console.WriteLine("Type 4 to delete Record.");
            Console.WriteLine("Type 5 to Update Record.");
            Console.WriteLine("---------------------------------\n");

            string command = Console.ReadLine();

            switch (command)
            {
                case "0":
                    Console.WriteLine("\nGoodbye!\n");
                    runApp = false;
                    // Environment.Exit(0); no hard exit
                    break;
                case "1":
                    Console.WriteLine("Please Enter a Habit name consisting of at least 4 letters: ");
                    string habitname = "";
                    bool checkHabitName = true;
                    do
                    {
                        habitname = Console.ReadLine().Trim();
                        if (habitname.Count() >= 4 && IsOnlyLetters(habitname))
                        {
                            checkHabitName = true;
                        }
                        else
                        {
                            Console.WriteLine("Please Enter a valid Habit Name: ");
                            checkHabitName= false;
                        }
                    } while (!checkHabitName);
                    CreateHabitTable(habitname);
                    habitList.Add(habitname);
                    Console.Clear();
                    Console.WriteLine($"Successfully created the habit '{habitname}'.");
                    break;

                case "2":
                    if (HabitExist())
                    {
                        string chosenHabit = WhichHabit();
                        GetAllRecords(chosenHabit);
                    }     
                    break;
                case "3":
                    if (HabitExist())
                    {
                        string chosenHabit = WhichHabit();
                        InsertRecord(chosenHabit);
                    }
                    break;
                case "4":
                    if (HabitExist())
                    {
                        string chosenHabit = WhichHabit();
                        DeleteRecord(chosenHabit);
                    }
                    break;
                case "5":
                    if (HabitExist())
                    {
                        string chosenHabit = WhichHabit();
                        UpdateRecord(chosenHabit);
                    }
                    break;
                default:
                    Console.WriteLine("\nInvalid Command. Please type a number from 0 to 5.\n");
                    break;
                
            }
        }

        private static void UpdateRecord(string chosenHabit)
        {
            Console.Clear();
            GetAllRecords(chosenHabit);

            var recordId = GetNumberInput("\n\nPlese type Id of the record woud like to update.\n\n");
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM {chosenHabit} WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    do
                    {
                        Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
                        recordId = GetNumberInput("\n\nPlese type Id of the record woud like to update.\n\n");
                        checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM {chosenHabit} WHERE Id = {recordId})";
                        checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());
                    } while (checkQuery == 0);
                    
                }

                string date = GetDateInput();
                int quantity = GetNumberInput("\n\nPlease insert any number of glasses or other measure of your choice (no decimals allowed)\n\n");
                bool isCompleted = GetBooleanInput();
                int isCompletedSql = isCompleted ? 1 : 0;

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE {chosenHabit} SET date = '{date}', quantity = {quantity}, iscompleted = {isCompletedSql} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static void DeleteRecord(string chosenHabit)
        {
            Console.Clear();
            GetAllRecords(chosenHabit);

            var recordId = GetNumberInput("\n\nPlease type Id of the record you want to delete.\n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"DELETE from {chosenHabit} WHERE Id = '{recordId}'";

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id{recordId} doesn't exist.\n\n");
                }
                connection.Close();
            }
        }

        private static void InsertRecord(string chosenHabit)
        {
            string date = GetDateInput();

            int quantity = GetNumberInput("\n\nPlease insert any number(no decimals allowed)\n\n");

            bool isCompleted = GetBooleanInput();
            int isCompletedSql = isCompleted ? 1 : 0;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"Insert INTO {chosenHabit}(date, quantity, iscompleted) VALUES('{date}', {quantity}, {isCompletedSql})";

                tableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static bool GetBooleanInput()
        {
            Console.WriteLine("Please decide whether the record is completed or not");
            Console.WriteLine("Please enter Boolean(0 for false, 1 for true: ");
            bool isFalse = true;
            while (isFalse)
            {
                string inputBool = Console.ReadLine();
                if(inputBool.Trim() == "1")
                {
                    return true;
                }
                else if(inputBool.Trim() == "0")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Invalid Input, please try again.");
                    isFalse = true;
                }
            }
            return true; //dummy
        }

        private static int GetNumberInput(string message)
        {
            Console.WriteLine(message);
            string numberInput;
            int numberIntput;
            do
            {
                Console.WriteLine("Please decide quantity of record.");
                Console.Write("\nEnter valid number here: ");
                numberInput = Console.ReadLine();
            } while (!int.TryParse(numberInput, out numberIntput));

            return numberIntput;
        }

        private static string GetDateInput()
        {
            Console.WriteLine("\n\nPlease insert the date(Format: dd-mm-yy).");

            string dateInput = Console.ReadLine();

            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n\nInvalid date. Format: dd-mm-yy.");
                dateInput = Console.ReadLine();
            }
            return dateInput;
        }

        private static bool HabitExist()
        {
            if (habitList.Count > 0)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Please create a Habit first!");
                return false;
            }
        }

        private static void GetAllRecords(string habitName)
        {

            Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"SELECT * FROM {habitName}";
                List<Habit> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                            new Habit
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2),
                                IsCompleted = reader.GetBoolean(3)
                            });
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("No rows found");
                }
                connection.Close();

                Console.WriteLine("-----------------------------------\n");
                foreach (var dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantity: {dw.Quantity} IsCompleted: {dw.IsCompleted}");
                }
                Console.WriteLine("-----------------------------------\n");
            }
        }

        private static string WhichHabit()
        {
            bool checkHabit;
            do
            {
                Console.WriteLine("With which habit do you want to proceed?");
                foreach (var item in habitList)
                {
                    Console.WriteLine($"{item}");
                }
                string chosenHabit = Console.ReadLine();
                if (habitList.Contains(chosenHabit))
                {
                    checkHabit = false;
                    return chosenHabit;
                }
                else
                {
                    Console.WriteLine("Invalid Input, Please try again...");
                    checkHabit = true;
                }
            } while (checkHabit);
            return ""; // dummy return
        }

        private static bool IsOnlyLetters(string text)
        {
            foreach (char c in text)
            {
                
                if (!char.IsLetter(c))
                {
                    return false; 
                }
            }
            return true; 
        }

        private static void CreateHabitTable(string habitname)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                // Your database operations here
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    $@"CREATE TABLE IF NOT EXISTS {habitname} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT,
                        Quantity INTEGER,
                        IsCompleted INTEGER
                        )";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
