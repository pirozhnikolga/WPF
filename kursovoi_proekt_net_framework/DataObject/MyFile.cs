using System;

namespace DataObject
{
    public class MyFile
    {
        public string FilePath { get; set; }

        public string Name { get; set; }

        public string MD5sum { get; set; }

        public bool Clone { get; set; }

        public MyFile()
        {
            FilePath = null;
            Name = null;
            MD5sum = null;
            Clone = false;
        }

    }

    
}
