using Phonebook.Interfaces;
using Phonebook.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Phonebook.Helpers
{
    public class XmlManager<T> : IFileManager<T> where T : INotifyPropertyChanged
    {
        private readonly string path;
        ObservableCollection<T> _collection;

        public XmlManager()
        {
            path = "C:/Users/Shai/Source/Repos/PhoneBook/PhoneBook/Models/Contacts.xml";
        }

        public void SetCollection(ObservableCollection<T> collection)
        {
            foreach (var item in collection)
            {
                item.PropertyChanged -= PropertyChangedItem_PropertyChanged;
                item.PropertyChanged += PropertyChangedItem_PropertyChanged;
            }
            _collection = collection;
            _collection.CollectionChanged -= CollectionChanged;
            _collection.CollectionChanged += CollectionChanged;
        }

        public ObservableCollection<T> ReadContactsFromFile()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<T>));
            using (StreamReader reader = new StreamReader(path))
            {
                return xmlSerializer.Deserialize(reader) as ObservableCollection<T>;
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    this.AddToFile(item);
                    INotifyPropertyChanged propertyChangedItem = item as INotifyPropertyChanged;
                    if (propertyChangedItem != null)
                    {
                        propertyChangedItem.PropertyChanged -= PropertyChangedItem_PropertyChanged;
                        propertyChangedItem.PropertyChanged += PropertyChangedItem_PropertyChanged;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    this.RemoveFromFile(item);
                    INotifyPropertyChanged propertyChangedItem = item as INotifyPropertyChanged;
                    if (propertyChangedItem != null)
                    {
                        propertyChangedItem.PropertyChanged -= PropertyChangedItem_PropertyChanged;
                        propertyChangedItem.PropertyChanged += PropertyChangedItem_PropertyChanged;
                    }
                }
            }
        }

        public void AddToFile<T>(T item)
        {
            Contact contact = item as Contact;
            XElement xml = XElement.Load(path);
            xml.Add(new XElement("Contact", new XAttribute("FullName", contact.FirstName + " " + contact.LastName),
            new XElement("FirstName", contact.FirstName),
            new XElement("LastName", contact.LastName),
            new XElement("PhoneNumber", contact.PhoneNumber),
            new XElement("IsMale", contact.IsMale)));
            xml.Save(path);
        }

        public void RemoveFromFile<T>(T item)
        {
            Contact contact = item as Contact;
            XDocument xdoc = XDocument.Load(path);
            xdoc.Descendants("Contact").Where(p => p.Attribute("FullName").Value == contact.FullName).FirstOrDefault().Remove();
            xdoc.Save(path);
        }

        private async void PropertyChangedItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Contact.FullName))
            {
                await WriteAllContactsToXMLAsync();
            }
        }

        private async Task WriteAllContactsToXMLAsync()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode lastChild;
            while ((lastChild = doc.DocumentElement.LastChild) != null)
            {
                doc.DocumentElement.RemoveChild(lastChild);
            }
            doc.Save(path);

            foreach (var contact in _collection)
            {
                await Task.Run(() => AddToFile(contact));
            }
        }
    }
}