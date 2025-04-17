namespace TaskScheduler
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public class TeamMember
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public TeamMember(string name, string role)
        {
            Id = Guid.NewGuid();
            Name = name;
            Role = role;
        }
    }

    public interface IProgressUpdatable
    {
        int CompletionPercentage { get; }
    }

    public class Task : IProgressUpdatable
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public TaskStatus Status { get; set; }
        public int CompletionPercentage { get; private set; }

        public Task(string title, string description, DateTime deadline)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            Deadline = deadline;
            Status = TaskStatus.NotStarted;
            CompletionPercentage = 0;
        }

        internal void UpdateProgress(int percent)
        {
            CompletionPercentage = Math.Clamp(percent, 0, 100);
            Status = percent == 100 ? TaskStatus.Completed :
                     percent > 0 ? TaskStatus.InProgress :
                     TaskStatus.NotStarted;
        }
    }

    public class TaskAssignment
    {
        public Task Task { get; set; }
        public TeamMember Executor { get; set; }
    }

    public class Project
    {
        private List<Task> tasks = new();
        private List<TeamMember> teamMembers = new();

        public void AddTask(Task task) => tasks.Add(task);
        public void RemoveTask(Guid id) => tasks.RemoveAll(t => t.Id == id);
        public void UpdateTask(Guid id, string? newTitle = null, string? newDescription = null, DateTime? newDeadline = null)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) throw new Exception("Task not found.");

            if (newTitle != null) task.Title = newTitle;
            if (newDescription != null) task.Description = newDescription;
            if (newDeadline != null) task.Deadline = (DateTime)newDeadline;
        }

        public List<Task> GetAllTasks() => tasks;

        public void AddTeamMember(TeamMember member) => teamMembers.Add(member);
        public void RemoveTeamMember(Guid id) => teamMembers.RemoveAll(m => m.Id == id);
        public void UpdateTeamMember(Guid id, string? newName = null, string? newRole = null)
        {
            var member = teamMembers.FirstOrDefault(m => m.Id == id);
            if (member == null) throw new Exception("Team member not found.");

            if (newName != null) member.Name = newName;
            if (newRole != null) member.Role = newRole;
        }

        public List<TeamMember> GetAllTeamMembers() => teamMembers;

        public List<Task> GetCompletedTasks() => tasks.Where(t => t.Status == TaskStatus.Completed).ToList();
        public List<Task> GetIncompleteTasks() => tasks.Where(t => t.Status != TaskStatus.Completed).ToList();

        public void CheckDeadlines()
        {
            foreach (var task in tasks)
            {
                string status = task.Deadline < DateTime.Now ? "deadline passed!" : "within deadline.";
                Console.WriteLine($"Task '{task.Title}' is {status}");
            }
        }

        public void DisplayProjectStatus()
        {
            foreach (var task in tasks)
            {
                Console.WriteLine($"- {task.Title}: {task.Status} ({task.CompletionPercentage}%)");
            }
        }
    }

    public class TaskManager()
    {
        private List<TaskAssignment> assignments = new();

        public void AssignTask(Task task, TeamMember member)
        {
            assignments.Add(new TaskAssignment { Task = task, Executor = member });
            task.Status = TaskStatus.InProgress;
        }

        public void UpdateTaskStatus(Guid taskId, TaskStatus status)
        {
            var assignment = assignments.FirstOrDefault(a => a.Task.Id == taskId);
            if (assignment == null) throw new Exception("Task not assigned.");

            assignment.Task.Status = status;
            if (status == TaskStatus.Completed) assignment.Task.UpdateProgress(100);
            if (status == TaskStatus.NotStarted) assignment.Task.UpdateProgress(0);
        }

        public void UpdateTaskCompletion(Guid taskId, int completionPercentage)
        {
            var assignment = assignments.FirstOrDefault(a => a.Task.Id == taskId);
            if (assignment == null) throw new Exception("Task not assigned.");

            assignment.Task.UpdateProgress(completionPercentage);
        }

        public void CheckWorkloadForAllMembers()
        {
            var grouped = assignments.GroupBy(a => a.Executor);

            foreach (var group in grouped)
            {
                Console.WriteLine($"{group.Key.Name} ({group.Key.Role}) - Tasks: {group.Count()}");
            }
        }

        public List<TaskAssignment> GetAllAssignments() => assignments;
    }

    public abstract class SearchBase
    {
        public abstract List<Task> FindTasksByExecutor(List<TaskAssignment> assignments, Guid executorId);
        public abstract List<TeamMember> FindExecutorsByTask(List<TaskAssignment> assignments, Guid taskId);
        public abstract List<Task> FindTasksByStatusAndDeadline(List<Task> tasks, TaskStatus status, bool deadlinePassed);
    }

    public class Search : SearchBase
    {
        public override List<Task> FindTasksByExecutor(List<TaskAssignment> assignments, Guid executorId)
        {
            return assignments
                .Where(a => a.Executor.Id == executorId)
                .Select(a => a.Task)
                .ToList();
        }

        public override List<TeamMember> FindExecutorsByTask(List<TaskAssignment> assignments, Guid taskId)
        {
            return assignments
                .Where(a => a.Task.Id == taskId)
                .Select(a => a.Executor)
                .ToList();
        }

        public override List<Task> FindTasksByStatusAndDeadline(List<Task> tasks, TaskStatus status, bool deadlinePassed)
        {
            return tasks
                .Where(t => t.Status == status && (deadlinePassed ? t.Deadline < DateTime.Now : t.Deadline >= DateTime.Now))
                .ToList();
        }
    }
}