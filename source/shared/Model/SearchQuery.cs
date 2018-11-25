using System;
using Inkton.Nest.Cloud;

namespace Codesearch.Model
{
    public class SearchQuery : CloudObject
    {
        private string _id;
        private int _maxResults = 10;
        private string _service;
        private string _text;

        public SearchQuery()
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

        public int MaxResults
        {
            get { return _maxResults; }
            set { SetProperty(ref _maxResults, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public override string CloudKey
        {
            get { return _id; }
        }
    }
}