using Lab_13.ViewModels;
using System.Windows;

namespace Lab_13;

public partial class MainWindow : Window
{
    public MainWindow(ContactsListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}