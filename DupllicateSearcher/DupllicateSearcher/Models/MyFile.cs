using System;

namespace DupllicateSearcher.Models
{
    public class MyFile : ViewModelBase
    {
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }
        private string md5sum;
        public string MD5sum
        {
            get { return md5sum; }
            set
            {
                md5sum = value;
                RaisePropertyChanged("MD5sum");
            }
        }
        private bool clone;
        public bool Clone
        {
            get {  return clone; }

            set
            {
                clone = value;
                RaisePropertyChanged("Clone");
            }
        }
    }


}
