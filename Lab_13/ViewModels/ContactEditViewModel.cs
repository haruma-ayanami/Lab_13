using Lab_13.Data;
using Lab_13.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Lab_13.ViewModels;

public class ContactEditViewModel : ObservableObject
{
    private readonly ApplicationContext _context;

    private int _editingContactId;
    private bool _isEditMode;

    public Action? OnSaved { get; set; }
    public Action? OnCanceled { get; set; }

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    private string _title = "Новый контакт";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    private string _nameInput = string.Empty;
    public string NameInput
    {
        get => _nameInput;
        set
        {
            _nameInput = value;
            OnPropertyChanged();
        }
    }

    private string _phoneInput = string.Empty;
    public string PhoneInput
    {
        get => _phoneInput;
        set
        {
            _phoneInput = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public ContactEditViewModel(ApplicationContext context)
    {
        _context = context;

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public void StartCreate()
    {
        _isEditMode = false;
        _editingContactId = 0;

        Title = "Добавление контакта";
        NameInput = string.Empty;
        PhoneInput = string.Empty;
        IsVisible = true;
    }

    public void StartEdit(Contact contact)
    {
        _isEditMode = true;
        _editingContactId = contact.Id;

        Title = "Редактирование контакта";
        NameInput = contact.Name;
        PhoneInput = contact.Phone;
        IsVisible = true;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(NameInput))
        {
            MessageBox.Show("Введите имя контакта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneInput))
        {
            MessageBox.Show("Введите номер телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_isEditMode)
            {
                SaveExistingContact();
            }
            else
            {
                SaveNewContact();
            }

            IsVisible = false;
            OnSaved?.Invoke();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show(
                "Ошибка сохранения данных в базе.\n" + ex.Message,
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

    private void SaveNewContact()
    {
        bool duplicateExists = _context.Contacts
            .Any(c => c.Phone == PhoneInput);

        if (duplicateExists)
        {
            MessageBox.Show(
                "Контакт с таким номером телефона уже существует.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        Contact newContact = new Contact
        {
            Name = NameInput,
            Phone = PhoneInput
        };

        _context.Contacts.Add(newContact);
        _context.SaveChanges();

        MessageBox.Show(
            "Контакт успешно добавлен.",
            "Информация",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void SaveExistingContact()
    {
        Contact? contact = _context.Contacts
            .FirstOrDefault(c => c.Id == _editingContactId);

        if (contact == null)
        {
            MessageBox.Show(
                "Контакт не найден в базе данных.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        bool duplicateExists = _context.Contacts
            .Any(c => c.Phone == PhoneInput && c.Id != _editingContactId);

        if (duplicateExists)
        {
            MessageBox.Show(
                "Контакт с таким номером телефона уже существует.",
                "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        contact.Name = NameInput;
        contact.Phone = PhoneInput;

        _context.SaveChanges();

        MessageBox.Show(
            "Контакт успешно изменён.",
            "Информация",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void Cancel()
    {
        IsVisible = false;
        OnCanceled?.Invoke();
    }
}