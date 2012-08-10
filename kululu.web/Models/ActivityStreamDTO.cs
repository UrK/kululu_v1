using System;

namespace Kululu.Web.Models
{
    public class ActivityStreamDTO
    {
        public DateTime Time { get; set; }
        public long ActorId { get; set; }
        public string ActorName { get; set; }
        public string Verb { get; set; }
        public long ObjectId { get; set; }
        public string ObjectName { get; set; }
    }
}