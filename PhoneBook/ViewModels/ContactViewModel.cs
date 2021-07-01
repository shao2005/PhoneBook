using System.Windows.Input;
using System.ComponentModel;
using Phonebook.Models;
using Phonebook.Commands;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Phonebook.Helpers;
using Phonebook.Interfaces;

namespace PhoneBook.ViewModels
{
    class ContactViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Contact> _contactList;
        private string _newFirstName;
        private string _newLastName;
        private Contact _selectedContact;
        public event PropertyChangedEventHandler PropertyChanged;
        private string _searchText;
        private readonly IFileManager<Contact> _xmlManager;

        public ContactViewModel()
        {
            _xmlManager = new XmlManager<Contact>();
            Contacts = _xmlManager.ReadContactsFromFile() as ObservableCollection<Contact>;
            _xmlManager.SetCollection(Contacts);
            ContactsView = (CollectionView)CollectionViewSource.GetDefaultView(Contacts);
            ContactsView.Filter = ContactFilter;

            //_contactList = new ObservableCollection<Contact>
            //{
            //    new Contact{FirstName = "Shai", LastName = "Oz", PhoneNumber = 1111, IsMale = true},
            //    new Contact{FirstName = "Hila", LastName = "Haor", PhoneNumber = 2222, IsMale = false},
            //    new Contact{FirstName = "Amir", LastName = "Tor", PhoneNumber = 3333, IsMale = true},
            //    new Contact{FirstName = "Reut", LastName = "Benisho", PhoneNumber = 4444, IsMale = false},
            //    new Contact{FirstName = "Dudi", LastName = "Bachar", PhoneNumber = 5555, IsMale = true},
            //    new Contact{FirstName = "Yehuda", LastName = "Levental", PhoneNumber = 6666, IsMale = true}
            //};
        }

        public ICollectionView ContactsView { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if(_searchText != value)
                {
                    _searchText = value;
                    ContactsView.Refresh();
                    OnPropertyChanged();
                }
            }
        }

        private bool ContactFilter(object obj)
        {
            Contact contact = obj as Contact;
            if (SearchText == null)
                return true;
            return contact?.FullName?.ToLower().Contains(SearchText?.ToLower()) == true;
        }

        public ObservableCollection<Contact> Contacts
        {
            get { return _contactList; }
            set { _contactList = value; }
        }

        public string NewFirstName
        {
            get { return _newFirstName; }
            set 
            {
                if(_newFirstName != value)
                {
                    _newFirstName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NewLastName
        {
            get { return _newLastName; }
            set
            {
                if (_newLastName != value)
                {
                    _newLastName = value;
                    OnPropertyChanged();
                }
            }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                if (_selectedContact != value)
                {
                    _selectedContact = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ICommand _addCommand;

        public ICommand AddContactCommand
        {
            get
            {
                if(_addCommand == null)
                {
                    _addCommand = new RelayCommand(param => AddContact(),
                        param => !(string.IsNullOrWhiteSpace(NewFirstName) || string.IsNullOrWhiteSpace(NewLastName)));
                }
                return _addCommand;
            }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if(_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => DeleteContact());
                }
                return _deleteCommand;
            }
        }

        private void DeleteContact()
        {
            Contacts.Remove(SelectedContact);
        }

        private void AddContact()
        {
            Contact newContact = new Contact { FirstName = NewFirstName, LastName = NewLastName };
            Contacts.Add(newContact);
        }

        //private void WriteToJson()
        //{
        //    string json = JsonSerializer.Serialize<ObservableCollection<Contact>>(Contacts);
        //    File.WriteAllText(_pathToJsonFile, json);
        //}
    }
}
