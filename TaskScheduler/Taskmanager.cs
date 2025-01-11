using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json;

namespace TaskScheduler
{
    public class TaskManager : INotifyPropertyChanged
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public ObservableCollection<TaskItem> CompletedTasks => new ObservableCollection<TaskItem>(Tasks.Where(t => t.Status == "Completed" || t.Status == "Archived"));
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<bool> OverdueTasksFound;
        private int _totalTasksCount;
        public int TotalTasksCount
        {
            get => _totalTasksCount;
            private set
            {
                _totalTasksCount = value;
                OnPropertyChanged(nameof(TotalTasksCount));
            }
        }
        private int _overdueTasksCount;
        public int OverdueTasksCount
        {
            get => _overdueTasksCount;
            private set
            {
                _overdueTasksCount = value;
                OnPropertyChanged(nameof(OverdueTasksCount));
            }
        }

        private int _highPriorityTasksCount;
        public int HighPriorityTasksCount
        {
            get => _highPriorityTasksCount;
            private set
            {
                _highPriorityTasksCount = value;
                OnPropertyChanged(nameof(HighPriorityTasksCount));
            }
        }

        private int _completedTasksCount;
        public int CompletedTasksCount
        {
            get => _completedTasksCount;
            private set
            {
                _completedTasksCount = value;
                OnPropertyChanged(nameof(CompletedTasksCount));
            }
        }
        public TaskManager() => Tasks = new ObservableCollection<TaskItem>();
        public void AddTask(TaskItem task)
        {
            Tasks.Add(task);
            UpdateStatistics();
        }
        public void RemoveTask(TaskItem task)
        {
            if (Tasks.Contains(task))
            {
                Tasks.Remove(task);
                UpdateStatistics();
            }
        }
        public void UpdateStatistics()
        {
            TotalTasksCount = Tasks.Count;
            OverdueTasksCount = Tasks.Count(t => t.Status == "Просрочено");
            HighPriorityTasksCount = Tasks.Count(t => t.Priority == "High");
            CompletedTasksCount = Tasks.Count(t => t.Status == "Completed");
        }
        public void CheckDeadlines()
        {
            foreach (var task in Tasks)
            {
                if (task.DueDate < DateTime.Now && task.Status != "Просрочено")
                    task.Status = "Просрочено";
            }

            // Проверка на наличие хотя бы одной просроченной задачи
            bool hasOverdue = Tasks.Any(task => task.Status == "Просрочено");

            // Вызываем событие для уведомления
            OverdueTasksFound?.Invoke(hasOverdue);
        }
        public ObservableCollection<TaskItem> GetFilteredView(string priority, string status)
        {
            var filteredTasks = Tasks.AsQueryable();

            if (priority != "Все приоритеты")
                filteredTasks = filteredTasks.Where(task => task.Priority == priority);

            if (status != "Все статусы")
            {
                if (status == "Archived")
                    filteredTasks = filteredTasks.Where(task => task.Status == "Archived");
                else if (status == "Completed")
                    filteredTasks = filteredTasks.Where(task => task.Status == "Completed");
                else
                    filteredTasks = filteredTasks.Where(task => task.Status == status);
            }

            return new ObservableCollection<TaskItem>(filteredTasks);
        }
        public void SaveToFile(string filePath)
        {
            var json = JsonConvert.SerializeObject(Tasks);
            File.WriteAllText(filePath, json);
        }
        public void LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var loadedTasks = JsonConvert.DeserializeObject<ObservableCollection<TaskItem>>(json);
            Tasks.Clear();
            foreach (var task in loadedTasks)
            {
                Tasks.Add(task);
            }

            // Проверить дедлайны после загрузки
            CheckDeadlines();
        }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public List<TaskItem> SearchTasks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Tasks.ToList(); // Преобразуем ObservableCollection в List

            query = query.ToLower();
            return Tasks
                .Where(task =>
                    task.Title.ToLower().Contains(query) ||
                    task.Description.ToLower().Contains(query))
                .ToList(); // Преобразуем результат LINQ-запроса в List
        }
        public void ArchiveTask(TaskItem task)
        {
            // Просто меняем статус задачи на "Archived"
            task.Status = "Archived";
            UpdateStatistics(); // Обновляем статистику
        }
    }
}
