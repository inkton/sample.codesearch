using System;
using Inkton.Nest.Cloud;

namespace Codesearch.Model
{
    public class SearchQuery : CloudObject
    {
        private int _id;
        private int _maxResults = 10;
        private DateTime _created = DateTime.Now;
        private string _text;

        public SearchQuery()
        {
        }

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public int MaxResults
        {
            get { return _maxResults; }
            set { SetProperty(ref _maxResults, value); }
        }

        public DateTime Created
        {
            get { return _created; }
            set { SetProperty(ref _created, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public override string CloudKey
        {
           get { return _id.ToString(); }
        }
    }
}