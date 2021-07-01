using System.Collections.ObjectModel;

namespace Phonebook.Interfaces
{
    public interface IFileManager<T>
    {
        void SetCollection(ObservableCollection<T> item);

        ObservableCollection<T> ReadContactsFromFile();
    }
}