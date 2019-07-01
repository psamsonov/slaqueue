using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLAQueue.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public Guid UserId { get; set; }
        public DateTime Enqueued { get; set; }
        public DateTime Processed { get; set; }
    }
}
