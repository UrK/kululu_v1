using System;

namespace Kululu.Entities
{
    public class ActivityStream
    {
        public DateTime Time { get; set; }
        public IActivity  Actor { get; set; }
        public string Verb { get; set; }
        public IActivity Object { get; set; }
        public ActivityType Type  { get; set; }
    }

    public enum ActivityType
    {
        All,
        AddedSong,
        RatedSong,
        UserJoined
    }
}