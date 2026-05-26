using Lab_13.Data;
using Lab_13.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Lab_13.ViewModels;

public class ContactsListViewModel : ObservableObject
{
    private readonly ApplicationContext _context;

    public ContactEditViewModel Editor { get; }

    private ObservableCollection<Contact> _allContacts = new();

    private ObservableCollection<Contact> _contacts = new();
    public ObservableCollection<Contact> Contacts
    {
        get => _contacts;
        set
        {
            _contacts = value;
            OnPropertyChanged();
        }
    }

    private Contact? _selectedContact;
    public Contact? SelectedContact
    {
        get => _selectedContact;
        set
        {
            _selectedContact = value;
            OnPropertyChanged();
        }
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ContactsListViewModel(
        ApplicationContext context,
        ContactEditViewModel editor)
    {
        _context = context;
        Editor = editor;

        Editor.OnSaved = LoadContacts;
        Editor.OnCanceled = LoadContacts;

        RefreshCommand = new RelayCommand(_ => LoadContacts());
        AddCommand = new RelayCommand(_ => AddContact());
        EditCommand = new RelayCommand(_ => EditContact());
        DeleteCommand = new RelayCommand(_ => DeleteContact());

        LoadContacts();
    }

    private void LoadContacts()
    {
        try
        {
            var contactsFromDb = _context.Contacts
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToList();

            _allContacts = new ObservableCollection<Contact>(contactsFromDb);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ошибка загрузки контактов из базы данных.\n" + ex.Message,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            Contacts = new ObservableCollection<Contact>(_allContacts);
            return;
        }

        string filter = FilterText.ToLower();

        var filteredContacts = _allContacts
            .Where(c =>
                c.Name.ToLower().Contains(filter) ||
                c.Phone.ToLower().Contains(filter))
            .ToList();

        Contacts = new ObservableCollection<Contact>(filteredContacts);
    }

    private void AddContact()
    {
        Editor.StartCreate();
    }

    private void EditContact()
    {
        if (SelectedContact == null)
        {
            MessageBox.Show(
                "Выберите контакт для редактирования.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        Editor.StartEdit(SelectedContact);
    }

    private void DeleteContact()
    {
        if (SelectedContact == null)
        {
            MessageBox.Show(
                "Выберите контакт для удаления.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Удалить контакт \"{SelectedContact.Name}\"?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Contact? contactFromDb = _context.Contacts
                .FirstOrDefault(c => c.Id == SelectedContact.Id);

            if (contactFromDb == null)
            {
                MessageBox.Show(
                    "Контакт не найден в базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            _context.Contacts.Remove(contactFromDb);
            _context.SaveChanges();

            _allContacts.Remove(SelectedContact);
            Contacts.Remove(SelectedContact);
            SelectedContact = null;

            MessageBox.Show(
                "Контакт успешно удалён.",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadContacts();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show(
                "Ошибка удаления контакта из базы данных.\n" + ex.Message,
                "Ошибка базы данных",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Произошла ошибка.\n" + ex.Message,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}