using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;

namespace DataObject
{
    public class FilesPool
    {
        public ObservableCollection<MyFile> Collection { get; set; }
        public string From { get; set; }

        public FilesPool()
        {
            Collection = new ObservableCollection<MyFile>();
            From = null;
        }
        public FilesPool(string _path)
        {
            Collection = new ObservableCollection<MyFile>();
            From = _path;
        }

        // метод наполнения коллекции
        public void Load()
        {
            // создаем список файлов, находящихся в папке
            var fileList = getFilesFrom(From);

            if (fileList.Count != 0)
            {
                // заполняем коллекцию объектами
                foreach (var file in fileList)
                {
                    Collection.Add(new MyFile()
                    {
                        FilePath = file,
                        Name = Path.GetFileName(file),
                        MD5sum = computeMD5Checksum(file),
                        Clone = false
                    });
                }
            }else // нет файлов
            {
                MessageBox.Show(
                "Папка пуста.",
                "Инфо",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);

                Collection.Clear();
            }
        }

        // метод поиска одинаковых файлов
        public void CheckClones()
        {
            var count = 0; //количество дубликатов

            //сортировка по MD5-сумме
            var sortedFileColl = Collection.OrderBy(x => x.MD5sum).ToList();

            // поиск повторов
            for (var i = 0; i < sortedFileColl.Count - 1; ++i)
            {
                if (sortedFileColl[i].MD5sum == sortedFileColl[i + 1].MD5sum &&
                    compareFilesByByte(sortedFileColl[i].FilePath, sortedFileColl[i + 1].FilePath) &&
                    sortedFileColl[i].Name == sortedFileColl[i + 1].Name)
                {
                    sortedFileColl[i + 1].Clone = true;
                    sortedFileColl[i].Clone = true;
                    ++count;
                }
            }

            // передаем обработанный список
            Collection = new ObservableCollection<MyFile>(sortedFileColl);

            //информируем о результате
            if (count == 0)
            {
                MessageBox.Show(
                "Одинаковых файлов не обнаружено.",
                "Инфо",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show(
                String.Format("Обнаружено дубликатов: {0}.", count,
                "Инфо",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk));
            }
        }

        // функция для создания списка файлов из заданной папки
        private List<string> getFilesFrom(string _path)
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
                        foreach (var el in getFilesFrom(item))
                        {
                            result.Add(el);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show(
                String.Format("Указан не верный путь: {0}.", _path,
                "Ошибка!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                String.Format("Ошибка: {0}", ex.Message,
                "Ошибка!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error));
            }

            return result;
        }

        // функция для нахождения MD5-суммы файла
        private string computeMD5Checksum(string _filePath)
        {
            using (var fs = File.OpenRead(_filePath))
            {
                var md5 = new MD5CryptoServiceProvider();
                var fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                var checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }

        // функция для сравнения двух файлов побайтно
        private bool compareFilesByByte(string _file_1, string _file_2)
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
                MessageBox.Show(
                    String.Format(ex.Message),
                    "Инфо",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
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
    }
}
