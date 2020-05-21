using HTML_Parser_Framework.MVVM_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace HTML_Parser_Framework.MODEL
{
    public class PageInfo: Model
    {
        public PageInfo(string url)
        {
            URL = url;
        }

        #region Variables
        private string _url;
        private int _Count;
        #endregion

        #region Properties
        public string URL
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        public int Count
        {
            get => _Count;
            set
            {
                _Count = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
