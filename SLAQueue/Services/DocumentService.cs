using SLAQueue.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLAQueue.Services
{
    public class DocumentService
    {
        private static Dictionary<SLAClass, Queue<Document>> Queues { get; set; } = new Dictionary<SLAClass, Queue<Document>>();

        static DocumentService()
        {
            Queues.Add(SLAClass.BestEffort, new Queue<Document>());
            Queues.Add(SLAClass.TenMinutes, new Queue<Document>());
            Queues.Add(SLAClass.OneHour, new Queue<Document>());
        }

        public static IEnumerable<Document> GetQueuedDocuments()
        {
            return Queues.Select(x => x.Value).Select(x => x.ToList()).SelectMany(x => x);
        }

        public static Guid Enqueue(Document document, SLAClass slaClass)
        {
            if (Queues.TryGetValue(slaClass, out Queue<Document> queue))
            {
                document.Id = Guid.NewGuid();
                document.Enqueued = DateTime.Now;
            
                queue.Enqueue(document);

                Console.WriteLine("Enqueued document " + document.Filename);
                return document.Id;
            }
            else
            {
                Console.WriteLine("Error: no queue found for " + slaClass);
                return Guid.Empty;
            }
        }


    }
}
