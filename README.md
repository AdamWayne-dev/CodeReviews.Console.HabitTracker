# Bad Habit Tracker (C# Console App)

This is a small console application for tracking "bad habits" using a
local SQLite database. A habit can be anything you want to monitor
(e.g., biting nails, oversleeping, procrastination), and each habit can
have multiple entries recorded against it.

The goal of the project was to practise CRUD operations, basic data
modelling, and working with SQLite through C#.

## What the app does

-   Create habits and choose how each one is measured (Times, Minutes,
    Hours)
-   Add entries for a habit
-   View all habits and their entries
-   Update or delete habits
-   Update or delete individual entries
-   Generate simple reports over common date ranges (day, week, month,
    year)

The database consists of two tables:

**BadHabits**\
`Id`, `HabitName`, `HabitType`

**HabitEntries**\
`Id`, `HabitId`, `Quantity`, `EntryDate`

Entries use a foreign key to link back to their habit.

## Running the project

Clone the repository and run:

``` bash
dotnet run
```

The app will:

-   Create the database file if it doesn't already exist
-   Create the tables
-   Insert some dummy records (only if the tables are empty)

Everything is handled automatically on startup.

## How the menu works

The entry point (`Main`) sets up the database and then loads a
loop-based menu. Each option (add, update, view, delete, report) is
backed by a dedicated method, and the helpers are written to keep
repetitive code out of the main program flow.

Dates are entered in **DD-MM-YY** format. Habit types control how entry
quantities are labelled in the UI.

## Reports

The reporting feature totals up all entries for each habit within a
selected date range. Currently available ranges:

-   Previous day
-   Previous 7 days
-   Previous 31 days
-   Previous year

The output lists each habit and the total quantity recorded in that
range.

## Project purpose

This was built as a learning exercise to explore:

-   SQLite with C#
-   Parameterised SQL commands
-   Basic data validation
-   Console UI design
-   Joining tables and aggregating data

It's intentionally simple and self-contained, but the structure makes it
easy to extend (e.g., exporting data, charts, streak tracking, or
turning it into a GUI/web version).
