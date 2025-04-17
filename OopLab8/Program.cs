using static System.Console;
using TaskScheduler;
using Task = TaskScheduler.Task;
using TaskStatus = TaskScheduler.TaskStatus;

namespace OopLab8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Створюємо проєкт
            Project project = new Project();
            TaskManager manager = new TaskManager();
            Search search = new Search();

            //Створюємо членів команди
            TeamMember mykyta = new TeamMember("Mykyta", "Backend Developer");
            TeamMember olena = new TeamMember("Olena", "Team Lead");
            TeamMember serhii = new TeamMember("Serhii", "Full Stack Developer");
            TeamMember anhelina = new TeamMember("Anhelina", "Designer");
            TeamMember bohdan = new TeamMember("Bohdan", "Backend Developer");
            TeamMember roman = new TeamMember("Roman", "Backend Developer");


            project.AddTeamMember(mykyta);
            project.AddTeamMember(olena);
            project.AddTeamMember(serhii);
            project.AddTeamMember(anhelina);
            project.AddTeamMember(bohdan);
            project.AddTeamMember(roman);

            //Тепер проводемо певні маніпуляції над ними
            project.UpdateTeamMember(mykyta.Id, newRole: "Full Stack Developer");
            project.RemoveTeamMember(olena.Id);

            WriteLine("--- Our team ---");
            List<TeamMember> team = project.GetAllTeamMembers();

            foreach (var teamMember in team)
            {
                WriteLine($"{teamMember.Name} as {teamMember.Role}");
            }
            WriteLine();


            //Тепер створимо завдання
            Task task1 = new Task("Implement login", "Login with email", DateTime.Now.AddDays(3));
            Task task2 = new Task("Create UI mockup", "Design login screen", DateTime.Now.AddDays(1));
            Task task3 = new Task("Fix bug #45", "Resolve critical issue", DateTime.Now.AddDays(5));
            Task task4 = new Task("Create homepage wireframe", "Design a low-fidelity wireframe for the homepage layout", DateTime.Now.AddDays(5));
            Task task5 = new Task("Fix bug #23", "Resolve critical issue", DateTime.Now.AddDays(-4));

            project.AddTask(task1);
            project.AddTask(task2);
            project.AddTask(task3);
            project.AddTask(task4);
            project.AddTask(task5);

            //Тепер проведемо маніпуляції і над завданнями
            project.UpdateTask(task2.Id, newDeadline: DateTime.Now.AddDays(1));
            project.RemoveTask(task4.Id);

            //Призначимо наші завдання членам команди
            manager.AssignTask(task1, mykyta);
            manager.AssignTask(task3, mykyta);
            manager.AssignTask(task2, anhelina);
            manager.AssignTask(task3, serhii);
            manager.AssignTask(task4, anhelina);
            manager.AssignTask(task1 , roman);

            //Встановимо статуси завданням
            manager.UpdateTaskStatus(task3.Id, TaskStatus.Completed);
            manager.UpdateTaskCompletion(task1.Id, completionPercentage: 60);

            //Виведемо інформацію про завдання
            WriteLine("--- Tasks ---");
            List<Task> tasks = project.GetAllTasks();

            foreach (var task in tasks)
            {
                WriteLine($"{task.Title}: {task.Description}");
            }

            WriteLine("\nCompleted Tasks:");
            List<Task> completedTasks = project.GetCompletedTasks();
            foreach (var task in completedTasks)
            {
                WriteLine($"{task.Title}: {task.Description} ({task.Status})");
            }

            WriteLine("\nIncomplete Tasks:");
            List<Task> incompleteTasks = project.GetIncompleteTasks();
            foreach (var task in incompleteTasks)
            {
                WriteLine($"{task.Title}: {task.Description} ({task.Status})");
            }
            WriteLine();

            //Перевіримо термін виконання
            WriteLine("--- Deadlines ---");
            project.CheckDeadlines();
            WriteLine();

            //Перевіримо завантаженість виконавців
            WriteLine("--- Workload ---");
            manager.CheckWorkloadForAllMembers();
            WriteLine();

            //Перевіримо стан виконання проекту
            WriteLine("--- Project status ---");
            project.DisplayProjectStatus();
            WriteLine();

            //Перевіримо функціонал пошуку
            WriteLine("--- Search ---");

            WriteLine("Tasks taken by 'Mykyta':");
            var tasksByMykyta = search.FindTasksByExecutor(manager.GetAllAssignments(), mykyta.Id);
            foreach (var task in tasksByMykyta)
            {
                WriteLine($"- {task.Title}");
            }
            WriteLine();

            WriteLine($"Executors of the task '{task2.Title}':");
            var executorsOfTask1 = search.FindExecutorsByTask(manager.GetAllAssignments(), task1.Id);
            foreach (var task in executorsOfTask1)
            {
                WriteLine($"- {task.Name} ({task.Role})");
            }
            WriteLine();

            WriteLine("Uncompleted tasks with expired deadlines:");
            var overdueNotCompleted = search.FindTasksByStatusAndDeadline(project.GetAllTasks(), TaskStatus.NotStarted, deadlinePassed: true);
            foreach (var task in overdueNotCompleted)
            {
                WriteLine($"- {task.Title}");
            }
            WriteLine();

            WriteLine("Completed tasks that have not yet expired:");
            var completedStillInTime = search.FindTasksByStatusAndDeadline(project.GetAllTasks(), TaskStatus.Completed, deadlinePassed: false);
            foreach (var task in completedStillInTime)
            {
                WriteLine($"- {task.Title}");
            }
        }
    }
}