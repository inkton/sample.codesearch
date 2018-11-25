using System;
using Inkton.Nest.Cloud;

namespace Codesearch.Model
{
    public class SearchResult : CloudObject
    {
        private string _id;
        private string _service;
        private string _data;
        private string _handledBy;

        public SearchResult()
        {
        }

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
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
            get { return Id; }
        }
    }
}