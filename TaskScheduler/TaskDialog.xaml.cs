    using System;
    using System.Windows;
    using System.Windows.Controls;

    namespace TaskScheduler
    {
        public partial class TaskDialog : Window
        {
            public TaskItem Task { get; private set; }
            public TaskDialog() => InitializeComponent();
            private void SaveButton_Click(object sender, RoutedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    MessageBox.Show("Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DueDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Due Date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Task = new TaskItem
                {
                    Title = TitleTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    DueDate = DueDatePicker.SelectedDate.Value,
                    Priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Status = "Planned" 
                };

                DialogResult = true; 
                Close();
            }
            private void CancelButton_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = false; 
                Close();
            }
        }
    }