using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;



namespace ToDoF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public ICollectionView TodoItemsView { get; set; }

        private string filePath = "todos.json";

        public ObservableCollection<TodoItem> TodoItems { get; set; } = new ObservableCollection<TodoItem>();

        //Window wont open on fullscreen
        private bool isFullScreen = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            TodoItemsView = CollectionViewSource.GetDefaultView(TodoItems);
            TodoItemsView.SortDescriptions.Add(new SortDescription(nameof(TodoItem.Date), ListSortDirection.Ascending));

            TodoItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(OpenTaskCount));
                TodoItemsView.Refresh();
            };

            LoadTasks();
            OnPropertyChanged(nameof(OpenTaskCount));
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            TodoItems.Add(new TodoItem
            {
                Text = "",
                Date = DateTime.Today
            });
        }
        public class TodoItem : INotifyPropertyChanged
        {
            private string _text;
            public string Text
            {
                get => _text;
                set
                {
                    _text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }

            private DateTime? _date;
            public DateTime? Date
            {
                get => _date;
                set
                {
                    _date = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }


        private void DoneTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TodoItem todo)
            {
                TodoItems.Remove(todo);
            }
        }

        public int OpenTaskCount
        {
            get { return TodoItems.Count; }
        }


        //Custom close button on top bar
        private void Close_button(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Custom fullscreen button on top bar
        private void Fullscreen_button(object sender, RoutedEventArgs e)
        {
            if (isFullScreen)
            {
                this.WindowState = WindowState.Normal;
                isFullScreen = false;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                isFullScreen = true;
            }
        }

        private void Minimize_button(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }

        }

        private void SaveTasks_Click(object sender, RoutedEventArgs e)
        {
            SaveConfirmationWindow saveWindow = new SaveConfirmationWindow();
            saveWindow.Owner = this;

            bool? result = saveWindow.ShowDialog();

            if (result == true)
            {
                SaveTasks();
            }
        }

        private void SaveTasks()
        {
            TodoItemsView.Refresh();

            string json = JsonSerializer.Serialize(TodoItems);
            File.WriteAllText(filePath, json);
        }

        private void LoadTasks()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var loadedTasks = JsonSerializer.Deserialize<ObservableCollection<TodoItem>>(json);

                if (loadedTasks != null)
                {
                    TodoItems.Clear();

                    foreach (var task in loadedTasks)
                    {
                        TodoItems.Add(task);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}