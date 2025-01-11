using System;
using System.ComponentModel;

namespace TaskScheduler
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        public string Category { get; set; }
        private DateTime _dueDate;
        private string _priority;
        private string _status;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                if (_dueDate != value)
                {
                    _dueDate = value;
                    OnPropertyChanged(nameof(DueDate));
                    UpdateStatus(); // После изменения даты обновляем статус
                }
            }
        }
        public string Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        private void UpdateStatus()
        {
            // Проверяем, просрочена ли задача
            if (_dueDate < DateTime.Now)
                Status = "Просрочено";
            else if (Status != "Completed")
                Status = "Planned"; // Если дата не прошла, устанавливаем статус как "Planned"
        }
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

}
