using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace SoapClientUI.ViewModels.Commands
{
    public class FetchEventsCommand : AbstractCommand
    {

        private readonly EventLogQuery EventLogQuery;
        private readonly int Pagination;
        private readonly Action<ICollection<EventRecord>> Action;
        private EventBookmark EventBookmark;

        public FetchEventsCommand(EventLogQuery EventLogQuery, int Pagination, Action<ICollection<EventRecord>> Action)
        {
            this.EventLogQuery = EventLogQuery;
            this.Pagination = Pagination;
            this.Action = Action;
        }

        public override bool CanExecute(object parameter)
        {
            return EventLogQuery is object && Pagination > 0 && Action is object;
        }

        public override void Execute(object parameter)
        {
            using (EventLogReader EventLogReader = new EventLogReader(EventLogQuery) { BatchSize = Pagination })
            {
                if (EventBookmark is object)
                {
                    EventLogReader.Seek(EventBookmark, 1);
                }
                ICollection<EventRecord> Enumeration = new Collection<EventRecord>();
                for (int i = Pagination; i > 0 && EventLogReader.ReadEvent() is EventRecord EventRecord; i--)
                {
                    Enumeration.Add(EventRecord);
                    EventBookmark = EventRecord.Bookmark;
                }
                Action.Invoke(Enumeration);
            };
        }
    }
}
