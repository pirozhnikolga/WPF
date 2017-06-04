using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using DataObject;

namespace kursovoi_proekt_net_framework
{
    public partial class MainWindow : Window
    {
        FilesPool fileCollection; // объект с коллекцией файлов

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_chooseFolder_Click(object sender, RoutedEventArgs e)
        {
            //создаем диалоговое окно для выбора папки
            var dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tb_path.Text = dlg.SelectedPath;
                // разрешаем запуск проверки
                btn_start.IsEnabled = true;
            }
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            // блокируем кнопку старта
            btn_start.IsEnabled = false;

            // чистим поля от предыдущих рузультатов;
            if (lb_delete.Content != null) lb_delete.Content = null;
            if(lb_fin.Content != null) lb_fin.Content = null;

            // создаем коллекцию
            fileCollection = new FilesPool(tb_path.Text);
            fileCollection.Load();

            if (fileCollection.Collection.Count == 0)
            {
                btn_uncheck.IsEnabled = false;
            }
            else
            {
                // порверяем наличие клонов
                fileCollection.CheckClones();
                // активируем кнопку "Снять все выделения"
                btn_uncheck.IsEnabled = true;
            }

            // передаем результат в ListView
            lstw.ItemsSource = fileCollection.Collection;
            lb_total.Content = "Просмотренно файлов: " + lstw.Items.Count;
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            int count = 0; // количество удалений
            int inumerator = 0; // номер файла в списке

            List<int> resultList = new List<int>();

            // поиск отмеченных на удаление файлов
            MyFile current = null;
            foreach (var item in lstw.Items)
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
                System.Windows.Forms.MessageBox.Show(
                    "Нет файлов для удаления.",
                    "Инфо",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
            else
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show(
                       "Вы уверены, что хотите удалить все выделенные файлы?",
                       "ВНИМАНИЕ!!!",
                       MessageBoxButtons.YesNo,
                       MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes) //Если нажал Да
                {
                    var times = 0;
                    // удаляем выбранные елементы
                    foreach (var n in resultList)
                    {
                        File.Delete(fileCollection.Collection[n - times].FilePath);
                        fileCollection.Collection.RemoveAt(n - times);
                        ++times;
                    }
                    System.Windows.Forms.MessageBox.Show(
                               "Файлы успешно удалены.",
                               "Готово",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Asterisk);
                }
                lb_delete.Content = "Из них удалено файлов: " + count;
                lb_fin.Content = "Осталось файлов: " + (lstw.Items.Count);
            }
          
            // обновляем список
            lstw.ItemsSource = fileCollection.Collection;

            if(lstw.Items.Count == 0)
            {
                btn_uncheck.IsEnabled = false;
            }
        }

        private void tb_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            // проверяем наличие пути и активируем/блокируем кнопку старта
            if (!string.IsNullOrEmpty(tb_path.Text))
                btn_start.IsEnabled = true;
            else
                btn_start.IsEnabled = false;
        }

        private void btn_uncheck_Click(object sender, RoutedEventArgs e)
        {
            // снимаем все отметки на удаление
            for(var i = 0; i < fileCollection.Collection.Count; ++i)
            {
                if(fileCollection.Collection[i].Clone)
                {
                    fileCollection.Collection[i].Clone = false;
                }
            }
            // обновляем лист
            lstw.ItemsSource = fileCollection.Collection;
            lstw.Items.Refresh();
        }
    }
}




