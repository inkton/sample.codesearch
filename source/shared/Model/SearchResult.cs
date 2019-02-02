using System;
using Inkton.Nest.Cloud;

namespace Codesearch.Model
{
    public class SearchResult : CloudObject
    {
        private int _id;
        private int _searchQueryid;
        private string _service;
        private string _data;
        private string _handledBy;

        public SearchResult()
        {
        }

        public int Id
        {
            get { return _id; } 
            set { SetProperty(ref _id, value); }
        }

        public int SearchQueryId
        {
            get { return _searchQueryid; }
            set { SetProperty(ref _searchQueryid, value); }
        }

        public string Service
        {
            get { return _service; }
            set { SetProperty(ref _service, value); }
        }

        public string HandledBy
        {
            get { return _handledBy; }
            set { SetProperty(ref _handledBy, value); }
        }

        public string Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        public override string CloudKey
        {
            get { return _id.ToString(); }
        }
    }
}