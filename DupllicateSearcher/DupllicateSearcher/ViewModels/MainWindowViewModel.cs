using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DupllicateSearcher.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

namespace DupllicateSearcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() { }

        private ObservableCollection<MyFile> filesCollection = new ObservableCollection<MyFile>();
        public ObservableCollection<MyFile> FilesCollection
        {
            get { return filesCollection; }
            set
            {
                filesCollection = value;
                RaisePropertyChanged("FilesCollection");
            }
        }

        private string collectionPath;
        public string CollectionPath
        {
            get { return collectionPath; }
            set
            {
                collectionPath = value;
                RaisePropertyChanged("CollectionPath");
            }
        }

        public ICommand OpenFolder
        {
            get
            {
                return new ActionCommand(() =>
                {
                    //открываем диалоговое окно для выбора папки
                    var dlg = new FolderBrowserDialog();
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CollectionPath = dlg.SelectedPath;
                    }
                });

            }
        }

        public ICommand Start
        {
            get
            {
                return new ActionCommand( () => { LoadCollection(); });
            }
        }

        public ICommand Delete
        {
            get
            {
                return new ActionCommand( () => { DeleteClones(); });
            }
        }

        public ICommand CheckUpAll
        {
            get
            {
                return new ActionCommand( () => { CheckUpClones(); });
            }
        }

        // метод для отмены удаления на всех документах
        private void CheckUpClones()
        {
            for (var i = 0; i < FilesCollection.Count; ++i)
            {
                if (FilesCollection[i].Clone)
                {
                    FilesCollection[i].Clone = false;
                }
            }
        }

        // метод для загрузки файлов в коллекцию
        private void LoadCollection()
        {
            FilesCollection.Clear();

            // создаем список файлов, находящихся в заданной директории
            var fileList = GetFilesListFrom(CollectionPath);

            if (fileList.Count != 0)
            {
                // заполняем коллекцию объектами
                foreach (var file in fileList)
                {
                    FilesCollection.Add(new MyFile()
                    {
                        FilePath = file,
                        Name = Path.GetFileName(file),
                        MD5sum = ComputeMD5Checksum(file),
                        Clone = false
                    });
                }

                //сортируем по MD5-сумме
                var sortedFileColl = FilesCollection.OrderBy(x => x.MD5sum).ToList();

                //проверяем наличие одинаковых файлов
                var count = 0; //количество дубликатов
                for (var i = 0; i < sortedFileColl.Count - 1; ++i)
                {
                    if (sortedFileColl[i].MD5sum == sortedFileColl[i + 1].MD5sum &&
                        CompareFilesByByte(sortedFileColl[i].FilePath, sortedFileColl[i + 1].FilePath) &&
                        sortedFileColl[i].Name == sortedFileColl[i + 1].Name)
                    {
                        sortedFileColl[i + 1].Clone = true;
                        sortedFileColl[i].Clone = true;
                        ++count;
                    }
                }

                // передаем обработанный список
                FilesCollection = new ObservableCollection<MyFile>(sortedFileColl);

                //информируем о результате
                if (count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(
                    "Одинаковых файлов не обнаружено.",
                    "Инфо",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                    string.Format("Обнаружено дубликатов: {0}.", count,
                    "Инфо",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk));
                }
            }
        }

        // метод для удаления помеченных файлов
        private void DeleteClones()
        {
            int count = 0; // количество удалений
            int inumerator = 0; // номер файла в списке
            List<int> resultList = new List<int>();

            // поиск отмеченных на удаление файлов
            MyFile current = null;
            foreach (var item in filesCollection)
            {
                current = item as MyFile;
                if (current.Clone == true)
                {
                    resultList.Add(inumerator);
                    ++count;
                }
                ++inumerator;
            }

            //информируем о рузультате
            if (resultList.Count == 0)
            {
                System.Windows.MessageBox.Show(
                "Нет файлов для удаления.",
                "Инфо", MessageBoxButton.OK);
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите удалить все выделенные файлы?",
                "ВНИМАНИЕ!!!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) //Если нажал Да
                {
                    var shift = 0;
                    // удаляем выбранные елементы из коллекции
                    foreach (var n in resultList)
                    {
                        File.Delete(FilesCollection[n - shift].FilePath);
                        FilesCollection.RemoveAt(n - shift);
                        ++shift;
                    }
                    System.Windows.MessageBox.Show(
                            string.Format("Файлы успешно удалены.\n" +
                            "Количество удаленных файлов: {0}.", resultList.Count,
                            "Готово",
                            MessageBoxButton.OK,
                            MessageBoxImage.Asterisk));
                }
            }
        }

        // метод для нахождения MD5-суммы файла
        private string ComputeMD5Checksum(string _filePath)
        {
            using (var fs = File.OpenRead(_filePath))
            {
                var md5 = new MD5CryptoServiceProvider();
                var fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                var checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", string.Empty);
                return result;
            }
        }

        // метод для сравнения двух файлов побайтно
        private bool CompareFilesByByte(string _file_1, string _file_2)
        {
            int file1Bytes, file2Bytes;
            FileStream fs1 = null, fs2 = null;

            try
            {
                fs1 = new FileStream(_file_1, FileMode.Open);
                fs2 = new FileStream(_file_2, FileMode.Open);

                do
                {
                    file1Bytes = fs1.ReadByte();
                    file2Bytes = fs2.ReadByte();
                }
                while ((file1Bytes == file2Bytes) && (file1Bytes != -1));

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    string.Format(ex.Message),
                    "Инфо",
                    MessageBoxButton.OK,
                   MessageBoxImage.Asterisk);
                return false;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
                if (fs2 != null)
                    fs2.Close();
            }

            return ((file1Bytes - file2Bytes) == 0);
        }

        // метод для создания списка файлов из заданной папки
        private List<string> GetFilesListFrom(string _path)
        {
            var result = new List<string>();

            try
            {
                foreach (var item in Directory.GetFileSystemEntries(_path))
                {
                    // проверяем наличие файлов
                    if (File.Exists(item))
                    {
                        result.Add(item);
                    }
                    // проверяем наличие других папок
                    else if (Directory.Exists(item))
                    {
                        foreach (var el in GetFilesListFrom(item))
                        {
                            result.Add(el);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                System.Windows.MessageBox.Show(
                "Указан неверный путь.",
                "Ошибка!",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show(
                "Укажите папку назначения.",
                "Ошибка!",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                string.Format("Ошибка: {0}", ex.Message,
                "Ошибка!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error));
            }

            return result;
        }

    }
}
