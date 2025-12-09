using Microsoft.Data.Sqlite;
using System.Globalization;


public class Program
{
    static string connectionString = @"Data Source=bad_habits_tracker.db";
    static void Main(string[] args)
    {
        InitialiseDatabase();
        CreateDummyData();
        GetUserInput();
    }

    #region Main Menu

    private static void GetUserInput()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n\nBad Habit Tracker - Main menu");
            Console.WriteLine("\n Please select an option\n");
            Console.WriteLine("[0] - Exit");
            Console.WriteLine("[1] - Add a bad habit");
            Console.WriteLine("[2] - Add an entry to an existing habit");
            Console.WriteLine("[3] - View all habits and entries");
            Console.WriteLine("[4] - Update a habit");
            Console.WriteLine("[5] - Update an entry for an existing habit");
            Console.WriteLine("[6] - Delete a habit");
            Console.WriteLine("[7] - Delete an entry for an existing habit");
            Console.WriteLine("[8] - View habit report");

            string userInput = Console.ReadLine();

            switch (userInput)
            {
                case "0":
                    return;

                case "1":
                    AddBadHabit();
                    break;

                case "2":
                    AddHabitEntry(existingHabitEntry: false);
                    break;

                case "3":
                    ViewHabitsAndEntries();
                    break;

                case "4":
                    UpdateBadHabit();
                    break;

                case "5":
                    UpdateHabitEntry();
                    break;

                case "6":
                    DeleteBadHabit();
                    break;

                case "7":
                    DeleteHabitEntry();
                    break;

                case "8":
                    ShowReport();
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    Console.ReadKey();
                    break;
            }
        }
    }
    #endregion

    #region Database Initisialisation

    private static void InitialiseDatabase()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS BadHabits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitName TEXT NOT NULL,
                    HabitType TEXT NOT NULL DEFAULT 'Times'
                );

                CREATE TABLE IF NOT EXISTS HabitEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitId INTEGER NOT NULL,
                    Quantity REAL NOT NULL,
                    EntryDate TEXT NOT NULL,
                    FOREIGN KEY(HabitId) REFERENCES BadHabits(Id)
                );";
            tableCmd.ExecuteNonQuery();
        }
    }
    #endregion

    #region Helper Methods

    private static int ChooseHabitId()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "SELECT Id, HabitName, HabitType FROM BadHabits";

            List<BadHabits> tableData = new();
            using (SqliteDataReader reader = tableCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tableData.Add(new BadHabits
                    {
                        Id = reader.GetInt32(0),
                        HabitName = reader.GetString(1),
                        HabitType = reader.GetString(2)
                    });
                }
            }

            Console.Clear();
            Console.WriteLine("Bad Habits:\n");
            foreach (var habit in tableData)
            {
                Console.WriteLine($"ID: {habit.Id} | Habit: {habit.HabitName} | Habit Type: {habit.HabitType}");
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Please type the ID of a habit, or type '0' to return to the main menu:");
            string input = Console.ReadLine();

            if (input == "0")
                return -1;

            if (int.TryParse(input, out int habitId))
                return habitId;

            Console.WriteLine("Invalid input. Press any key to return.");
            Console.ReadKey();
            return -1;
        }
    }


    private static int ChooseHabitEntryId(int habitId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Get habit info (name & type)
            var habitCmd = connection.CreateCommand();
            habitCmd.CommandText = "SELECT HabitName, HabitType FROM BadHabits WHERE Id = @id";
            habitCmd.Parameters.AddWithValue("@id", habitId);

            string habitName = null;
            string habitType = null;
            using (var habitReader = habitCmd.ExecuteReader())
            {
                if (habitReader.Read())
                {
                    habitName = habitReader.GetString(0);
                    habitType = habitReader.GetString(1);
                }
            }

            if (habitName == null)
            {
                Console.WriteLine("Habit not found.");
                Console.ReadKey();
                return -1;
            }

            // Get entries
            var entryCmd = connection.CreateCommand();
            entryCmd.CommandText = "SELECT Id, Quantity, EntryDate FROM HabitEntries WHERE HabitId = @habitId";
            entryCmd.Parameters.AddWithValue("@habitId", habitId);

            List<HabitEntries> entries = new();
            using (SqliteDataReader entryReader = entryCmd.ExecuteReader())
            {
                while (entryReader.Read())
                {
                    entries.Add(new HabitEntries
                    {
                        Id = entryReader.GetInt32(0),
                        HabitId = habitId,
                        Quantity = entryReader.GetFloat(1),
                        EntryDate = entryReader.GetString(2)
                    });
                }
            }

            Console.Clear();
            Console.WriteLine($"Entries for {habitName}:\n");
            if (entries.Count == 0)
            {
                Console.WriteLine("No entries found for this habit.");
                Console.WriteLine("Press any key to return.");
                Console.ReadKey();
                return -1;
            }

            foreach (var entry in entries)
            {
                Console.WriteLine($"Entry ID: {entry.Id} | {habitType}: {entry.Quantity} | Date: {entry.EntryDate}");
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Enter the Entry ID to select it, or '0' to cancel:");
            string input = Console.ReadLine();

            if (input == "0")
                return -1;

            if (int.TryParse(input, out int entryId))
                return entryId;

            Console.WriteLine("Invalid input. Press any key to return.");
            Console.ReadKey();
            return -1;
        }
    }

    private static void ViewHabitsAndEntries()
    {
        int habitId = ChooseHabitId();
        if (habitId == -1)
            return;

        ShowEntriesForHabit(habitId, waitForKey: true);
    }

    private static void ShowEntriesForHabit(int habitId, bool waitForKey)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            
            var habitCmd = connection.CreateCommand();
            habitCmd.CommandText = "SELECT HabitName, HabitType FROM BadHabits WHERE Id = @id";
            habitCmd.Parameters.AddWithValue("@id", habitId);

            string habitName = null;
            string habitType = null;
            using (var habitReader = habitCmd.ExecuteReader())
            {
                if (habitReader.Read())
                {
                    habitName = habitReader.GetString(0);
                    habitType = habitReader.GetString(1);
                }
            }

            if (habitName == null)
            {
                Console.WriteLine("Habit not found.");
                if (waitForKey)
                {
                    Console.ReadKey();
                }
                return;
            }

            var entryCmd = connection.CreateCommand();
            entryCmd.CommandText = "SELECT Id, Quantity, EntryDate FROM HabitEntries WHERE HabitId = @habitId";
            entryCmd.Parameters.AddWithValue("@habitId", habitId);

            List<HabitEntries> entries = new();
            using (SqliteDataReader entryReader = entryCmd.ExecuteReader())
            {
                while (entryReader.Read())
                {
                    entries.Add(new HabitEntries
                    {
                        Id = entryReader.GetInt32(0),
                        HabitId = habitId,
                        Quantity = entryReader.GetFloat(1),
                        EntryDate = entryReader.GetString(2)
                    });
                }
            }

            Console.Clear();
            Console.WriteLine($"Entries for {habitName}:\n");
            if (entries.Count == 0)
            {
                Console.WriteLine("No entries found for this habit.");
            }
            else
            {
                foreach (var entry in entries)
                {
                    Console.WriteLine($"Entry ID: {entry.Id} | {habitType}: {entry.Quantity} | Date: {entry.EntryDate}");
                    Console.WriteLine("-------------------------------------------------------");
                }
            }

            Console.WriteLine("----------------------------------------");
            if (waitForKey)
            {
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
        }
    }

    static void ShowReport()
    {
        Console.Clear();
        Console.WriteLine("--- Habit Report ---");

        Console.WriteLine("Please choose a date range for your report:\n");
        Console.WriteLine("[1] - Previous day\n[2] - Previous 7 days\n[3] - Previous 31 days\n[4] - Previous year\n");

        string choice = Console.ReadLine();

        DateTime endDate = DateTime.Today;

        (DateTime startDate, string label) = choice switch
        {
            "1" => (endDate.AddDays(-1), "Previous day"),
            "2" => (endDate.AddDays(-6), "Previous 7 days"),
            "3" => (endDate.AddDays(-30), "Previous 31 days"),
            "4" => (endDate.AddYears(-1), "Previous year"),
            _ => (endDate.AddDays(-7), "Previous 7 days (default)"),
        };

        GenerateReport(startDate, endDate, label);
    }

    private static void GenerateReport(DateTime startDate, DateTime endDate, string label)
    {
        var culture = new CultureInfo("en-GB");

        var totals = new Dictionary<int, (string Name, string Type, float Total)>();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT h.Id, h.HabitName, h.HabitType, e.Quantity, e.EntryDate
                                FROM HabitEntries e
                                JOIN BadHabits h ON e.HabitId = h.Id;";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int habitId = reader.GetInt32(0);
                    string habitName = reader.GetString(1);
                    string habitType = reader.GetString(2);
                    float quantity = reader.GetFloat(3);
                    string entryDateText = reader.GetString(4);

                    if(!DateTime.TryParseExact(entryDateText, "dd-MM-yy", culture, DateTimeStyles.None, out var entryDate))
                    {
                        continue;
                    }

                    if (entryDate < startDate || entryDate > endDate)
                        continue;

                    if (totals.TryGetValue(habitId, out var existing))
                    {
                        totals[habitId] = (existing.Name, existing.Type, existing.Total + quantity);
                    }
                    else
                    {
                        totals[habitId] = (habitName, habitType, quantity);
                    }
                }
            }
        }

        Console.Clear();
        Console.WriteLine($"--- Habit Report: {label} ---");
        Console.WriteLine($"From {startDate:dd-MM-yy} to {endDate: dd-MM-yy}\n");

        if (totals.Count == 0)
        {
            Console.WriteLine("No entries found in that date range.");
        }
        else
        {
            foreach (var kvp in totals)
            {
                var info = kvp.Value;
                Console.WriteLine($"{info.Name}: {info.Total} {info.Type}");
            }
        }

        Console.WriteLine("\n--------------------------------");
        Console.WriteLine("Press any key to return to the main menu");
        Console.ReadKey();
    }
    #endregion

    #region CRUD Operations

    private static void AddBadHabit()
    {
        Console.Clear();
        Console.WriteLine("Enter the name of the bad habit:");
        string name = Console.ReadLine();

        Console.WriteLine("How would you like to track your habit?\n [1] - Times\n [2] - Minutes\n [3] - Hours");
        string typeAnswer = Console.ReadLine();

        string habitType = typeAnswer switch
        {
            "2" => "Minutes",
            "3" => "Hours",
            _ => "Times"
        };

        long newHabitId;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "INSERT INTO BadHabits(HabitName, HabitType) VALUES (@habitName, @habitType)";
            AddParams(tableCmd,
               ("@habitName", name),
               ("@habitType", habitType));
            tableCmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            newHabitId = (long)idCmd.ExecuteScalar();
        }

        Console.WriteLine("Would you like to make an entry for this habit now?\n");
        Console.WriteLine("[1] - Yes\n[2] - No");
        string makeEntry = Console.ReadLine();
        if (makeEntry == "1")
            AddHabitEntry(existingHabitEntry: true, habitId: newHabitId);
    }

    private static void UpdateBadHabit()
    {
        Console.Clear();

        int habitId = ChooseHabitId();
        if (habitId == -1)
            return;

        Console.WriteLine("Please enter the new name of your bad habit (leave blank to keep current):");
        string name = Console.ReadLine();

        Console.WriteLine("How would you like to track your habit?\n [1] - Times\n [2] - Minutes\n [3] - Hours\n [Enter] - keep current");
        string typeAnswer = Console.ReadLine();

        string? newHabitType = null;
        if (!string.IsNullOrWhiteSpace(typeAnswer))
        {
            newHabitType = typeAnswer switch
            {
                "2" => "Minutes",
                "3" => "Hours",
                _ => "Times"
            };
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // Get current values
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT HabitName, HabitType FROM BadHabits WHERE Id = @id";
            selectCmd.Parameters.AddWithValue("@id", habitId);

            string currentName = null;
            string currentType = null;
            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    currentName = reader.GetString(0);
                    currentType = reader.GetString(1);
                }
            }

            if (currentName == null)
            {
                Console.WriteLine("Habit not found.");
                Console.ReadKey();
                return;
            }

            string finalName = string.IsNullOrWhiteSpace(name) ? currentName : name;
            string finalType = newHabitType ?? currentType;

            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE BadHabits SET HabitName = @name, HabitType = @type WHERE Id = @id";
            AddParams(updateCmd,
                ("@name", finalName),
                ("@type", finalType),
                ("@id", habitId));

            updateCmd.ExecuteNonQuery();
        }

        Console.WriteLine("Habit updated successfully.");
        Console.ReadKey();
    }

    private static void DeleteBadHabit()
    {
        Console.Clear();

        int habitId = ChooseHabitId();
        if (habitId == -1)
            return;

        Console.WriteLine("Are you sure you want to delete this habit and all its entries? [y/N]");
        string confirm = Console.ReadLine();
        if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
            return;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            // First delete entries
            var deleteEntriesCmd = connection.CreateCommand();
            deleteEntriesCmd.CommandText = "DELETE FROM HabitEntries WHERE HabitId = @habitId";
            deleteEntriesCmd.Parameters.AddWithValue("@habitId", habitId);
            deleteEntriesCmd.ExecuteNonQuery();

            // Then delete the habit
            var deleteHabitCmd = connection.CreateCommand();
            deleteHabitCmd.CommandText = "DELETE FROM BadHabits WHERE Id = @id";
            deleteHabitCmd.Parameters.AddWithValue("@id", habitId);

            int rowsAffected = deleteHabitCmd.ExecuteNonQuery();
            if (rowsAffected > 0)
                Console.WriteLine("Habit deleted!");
            else
                Console.WriteLine("No habit found with that ID.");
        }

        Console.ReadKey();
    }

    private static void AddHabitEntry(bool existingHabitEntry, long? habitId = null)
    {
        Console.Clear();

        int chosenHabitId;
        if (!existingHabitEntry || habitId == null)
        {
            chosenHabitId = ChooseHabitId();
            if (chosenHabitId == -1)
                return;
        }
        else
        {
            chosenHabitId = (int)habitId.Value;
        }

        float quantity = GetNumberInput("Please enter the amount of times / minutes / hours you performed this habit:\n");
        string entryDate = GetDateInput();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "INSERT INTO HabitEntries(HabitId, Quantity, EntryDate) VALUES (@habitId, @quantity, @entryDate)";

            AddParams(tableCmd,
                ("@habitId", chosenHabitId),
                ("@quantity", quantity),
                ("@entryDate", entryDate));
            tableCmd.ExecuteNonQuery();
        }

        Console.WriteLine("Entry added successfully.");
        Console.ReadKey();
    }

    private static void UpdateHabitEntry()
    {
        Console.Clear();

        int habitId = ChooseHabitId();
        if (habitId == -1)
            return;

        int entryId = ChooseHabitEntryId(habitId);
        if (entryId == -1)
            return;

        float quantity = GetNumberInput("Please enter the new quantity for this entry:\n");
        string entryDate = GetDateInput();

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "UPDATE HabitEntries SET Quantity = @quantity, EntryDate = @entryDate WHERE Id = @entryId";
            AddParams(tableCmd,
                ("@quantity", quantity),
                ("@entryDate", entryDate),
                ("@entryId", entryId));
            tableCmd.ExecuteNonQuery();
        }

        Console.WriteLine("Entry updated successfully.");
        Console.ReadKey();
    }

    private static void DeleteHabitEntry()
    {
        Console.Clear();

        int habitId = ChooseHabitId();
        if (habitId == -1)
            return;

        int entryId = ChooseHabitEntryId(habitId);
        if (entryId == -1)
            return;

        Console.WriteLine("Are you sure you want to delete this entry? [y/N]");
        string confirm = Console.ReadLine();
        if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
            return;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "DELETE FROM HabitEntries WHERE Id = @entryId";
            tableCmd.Parameters.AddWithValue("@entryId", entryId);

            int rowsAffected = tableCmd.ExecuteNonQuery();
            if (rowsAffected > 0)
                Console.WriteLine("Entry deleted!");
            else
                Console.WriteLine("No entry found with that ID.");
        }

        Console.ReadKey();
    }
    #endregion

    #region Input Helpers

    private static string GetDateInput()
    {
        Console.WriteLine("Enter the date (DD-MM-YY) - If you would like to add today's date, please type '1':");
        string dateInput = Console.ReadLine();

        if (dateInput == "1")
            return DateTime.Now.ToString("dd-MM-yy");

        while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out _))
        {
            Console.WriteLine("Invalid date format. Please enter the date in DD-MM-YY format:\n");
            dateInput = Console.ReadLine();
        }

        return dateInput;
    }

    private static float GetNumberInput(string message)
    {
        float quantity;

        Console.WriteLine(message);

        while (!float.TryParse(Console.ReadLine(), out quantity))
        {
            Console.WriteLine("Invalid input. Please enter a valid number:\n");
            Console.Write(message);
        }

        return quantity;
    }

    static void AddParams(SqliteCommand cmd, params (string key, object value)[] parameters)
    {
        foreach (var p in parameters)
        {
            cmd.Parameters.AddWithValue(p.key, p.value);
        }
    }

    #endregion

    #region Dummy Data

    static void CreateDummyData()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var checkHabitsCmd = connection.CreateCommand();
            checkHabitsCmd.CommandText = "SELECT COUNT(*) FROM BadHabits;";
            int habitCount = Convert.ToInt32(checkHabitsCmd.ExecuteScalar());

            if (habitCount == 0)
            {
                var insertHabitsCmd = connection.CreateCommand();
                insertHabitsCmd.CommandText = @"
                    INSERT INTO BadHabits (HabitName, HabitType) VALUES
                    ('Biting nails', 'Times'),
                    ('Oversleeping', 'Minutes'),
                    ('Procrastination', 'Hours');
                ";
                insertHabitsCmd.ExecuteNonQuery();
            }

            var checkEntriesCmd = connection.CreateCommand();
            checkEntriesCmd.CommandText = "SELECT COUNT(*) FROM HabitEntries;";
            int entryCount = Convert.ToInt32(checkEntriesCmd.ExecuteScalar());

            if (entryCount == 0)
            {
                var insertEntryCmd = connection.CreateCommand();
                string[] randomDates =
                {
                    "01-10-24","02-10-24","03-10-24","04-10-24","05-10-24","06-10-24",
                    "07-10-24","08-10-24","09-10-24","10-10-24","11-10-24","12-10-24"
                };

                Random rng = new Random();

                for (int i = 0; i < 100; i++)
                {
                    int habitId = rng.Next(1, 4);
                    int value = rng.Next(1, 20);
                    string date = randomDates[rng.Next(randomDates.Length)];

                    insertEntryCmd.CommandText = @"
                        INSERT INTO HabitEntries (HabitId, Quantity, EntryDate)
                        VALUES (@hId, @qty, @date)";
                    insertEntryCmd.Parameters.Clear();
                    insertEntryCmd.Parameters.AddWithValue("@hId", habitId);
                    insertEntryCmd.Parameters.AddWithValue("@qty", value);
                    insertEntryCmd.Parameters.AddWithValue("@date", date);
                    insertEntryCmd.ExecuteNonQuery();
                }
            }
        }
    }
}

#endregion

#region Models

public class BadHabits
{
    public int Id { get; set; }
    public string HabitName { get; set; }
    public string HabitType { get; set; }
}

public class HabitEntries
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public float Quantity { get; set; }
    public string EntryDate { get; set; }
}
#endregion