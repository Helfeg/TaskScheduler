using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace TaskScheduler
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ICollectionView _tasksView;
        private TaskManager _taskManager;
        public TaskManager TaskManager
        {
            get => _taskManager;
            set
            {
                if (_taskManager != value)
                {
                    _taskManager = value;
                    OnPropertyChanged(nameof(TaskManager));
                }
            }
        }
        private DispatcherTimer _deadlineCheckTimer;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public MainWindow()
        {
            InitializeComponent();
            TaskManager = new TaskManager();
            DataContext = this;
            // Отображение задач для текущей даты
            FilterTasksByDate(DateTime.Today);
            // Настройка таймера
            _deadlineCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _deadlineCheckTimer.Tick += (sender, e) => TaskManager.CheckDeadlines();
            _deadlineCheckTimer.Start();

            _tasksView = CollectionViewSource.GetDefaultView(TaskManager.Tasks);
            TaskDataGrid.ItemsSource = _tasksView;

            // Добавление обработчиков событий (если необходимо)
            // Подписка на события
            PriorityFilterComboBox.SelectionChanged += FilterComboBox_SelectionChanged;
            StatusFilterComboBox.SelectionChanged += FilterComboBox_SelectionChanged;
        }


        private void FilterTasksByDate(DateTime selectedDate)
        {
            var tasksForSelectedDate = TaskManager.Tasks
                .Where(task => task.DueDate.Date == selectedDate.Date)
                .ToList();

            // Преобразуем List<TaskItem> в ObservableCollection<TaskItem>
            TaskDataGrid.ItemsSource = new ObservableCollection<TaskItem>(tasksForSelectedDate);
        }
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var taskDialog = new TaskDialog();
            if (taskDialog.ShowDialog() == true)
            {
                // Проверяем, не просрочена ли дата
                if (taskDialog.Task.DueDate < DateTime.Now)
                    taskDialog.Task.Status = "Просрочено"; // Устанавливаем статус "Просрочено"
                else
                    taskDialog.Task.Status = "Planned"; // Если дата не просрочена, устанавливаем статус как "Planned"

                TaskManager.AddTask(taskDialog.Task);
                TaskManager.UpdateStatistics();
                FilterComboBox_SelectionChanged(null, null);  // Применяем фильтры
            }
        }
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskDataGrid.SelectedItem is TaskItem selectedTask)
            {
                TaskManager.RemoveTask(selectedTask);
                FilterComboBox_SelectionChanged(null, null); // Применяем фильтры
            }
        }
        private void MarkCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskDataGrid.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show("Вы желаете, чтобы задача была архивирована?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    TaskManager.ArchiveTask(selectedTask);
                }
                else
                {
                    selectedTask.Status = "Completed";
                }

                TaskManager.UpdateStatistics();
                FilterComboBox_SelectionChanged(null, null); // Применяем фильтры
            }
        }
        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskManager == null) return;

            string selectedPriority = (PriorityFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все приоритеты";
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все статусы";
            string selectedCategory = (CategoryFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Все категории";

            _tasksView.Filter = item =>
            {
                var task = item as TaskItem;
                bool matchesPriority = selectedPriority == "Все приоритеты" || task.Priority == selectedPriority;
                bool matchesStatus = selectedStatus == "Все статусы" || task.Status == selectedStatus;
                bool matchesCategory = selectedCategory == "Все категории" || task.Category == selectedCategory;
                return matchesPriority && matchesStatus && matchesCategory;
            };
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "JSON Files|*.json" };
            if (saveFileDialog.ShowDialog() == true)
                TaskManager.SaveToFile(saveFileDialog.FileName);
            
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "JSON Files|*.json" };
            if (openFileDialog.ShowDialog() == true)
            {
                TaskManager.LoadFromFile(openFileDialog.FileName);
                _tasksView = CollectionViewSource.GetDefaultView(TaskManager.Tasks); // Обновляем CollectionView
                TaskDataGrid.ItemsSource = _tasksView; // Привязываем CollectionView к DataGrid
                FilterComboBox_SelectionChanged(null, null); // Применяем фильтры
                TaskManager.UpdateStatistics();
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TaskManager == null) return;

            string query = SearchTextBox.Text.ToLower();

            _tasksView.Filter = item =>
            {
                var task = item as TaskItem;
                return task.Title.ToLower().Contains(query) ||
                       task.Description.ToLower().Contains(query) ||
                       task.Priority.ToLower().Contains(query) ||
                       task.Status.ToLower().Contains(query) ||
                       task.Category.ToLower().Contains(query);
            };
        }
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _tasksView.Filter = null; // Сбрасываем фильтр поиска
            FilterComboBox_SelectionChanged(null, null); // Применяем текущие фильтры
        }
        private void TaskDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() != "Status" || e.Row.Item is not TaskItem task) return;
            if (task.Status == "Archived")
                TaskManager.ArchiveTask(task); // Архивация выполненной задачи
                
            TaskManager.UpdateStatistics();
        }
    }

}