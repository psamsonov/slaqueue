using SLAQueue.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SLAQueue.Services
{
    public class DocumentService
    {
        private const int MaxNumberOfProcessors = 1000;

        private static Dictionary<SLAClass, Queue<Document>> Queues { get; set; } = new Dictionary<SLAClass, Queue<Document>>();
        private static Dictionary<SLAClass, List<DocumentProcessor>> Processors { get; set; } = new Dictionary<SLAClass, List<DocumentProcessor>>();
        private static Timer OrchestrationTimer = new Timer(Orchestrate, null, 0, 1000);
        private static List<Document> Finished = new List<Document>();
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        static DocumentService()
        {
            Queues.Add(SLAClass.BestEffort, new Queue<Document>());
            Queues.Add(SLAClass.TenMinutes, new Queue<Document>());
            Queues.Add(SLAClass.OneHour, new Queue<Document>());


            Processors.Add(SLAClass.TenMinutes, new List<DocumentProcessor>());
            Processors.Add(SLAClass.OneHour, new List<DocumentProcessor>());
        }

        //Avoid issues with several processors accessing this list concurrently
        public static void FinishDocument(Document document)
        {
            semaphore.Wait();
            Finished.Add(document);
            semaphore.Release();
        }

        /// <summary>
        /// Returns a document from the requested SLA queue. If that queue is empty, returns a document from the best effort queue instead.
        /// TODO: draw from all lower priority queues before hitting the best effort
        /// </summary>
        /// <param name="slaClass">SLA class to get first from</param>
        /// <returns></returns>
        public static Document GetDocument(SLAClass slaClass)
        {
            if (Queues.TryGetValue(slaClass, out Queue<Document> queue))
            {
                if (queue.Any())
                {
                    return queue.Dequeue();
                }
                else if (Queues.GetValueOrDefault(SLAClass.BestEffort).Any())
                {
                    return Queues.GetValueOrDefault(SLAClass.BestEffort).Dequeue();
                }
            }

            return null;
        }

        /// <summary>
        /// Return a collapsed list of all enqueued documents
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Document> GetFinishedDocuments()
        {
            return Finished;
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

        //Adjusts the number of processors to meet the SLA 
        private static void Orchestrate(object state)
        {
            OrchestarateQueue(SLAClass.TenMinutes);
            OrchestarateQueue(SLAClass.OneHour);
        }

        private static void OrchestarateQueue(SLAClass queueClass)
        {
            //There are no processors dedicated to best effort
            if (queueClass == SLAClass.BestEffort)
                return;

            //Find the minimum number of processors you need to make, round up
            int numProcessorsRunning = Processors.GetValueOrDefault(queueClass).Count(x => x.Status != ProcessorStatus.ShutDown  && x.Status != ProcessorStatus.ShuttingDown);
            int workInQueue = Queues.GetValueOrDefault(queueClass).Count * Constants.TimeToProcessOneDocument;
            int numberOfProcessorsWanted = (int)Math.Ceiling( workInQueue / (double)queueClass);
            //Can't exceed our max in each queue
            int numberOfProcessorsNeeded = Math.Min(numberOfProcessorsWanted, MaxNumberOfProcessors - numProcessorsRunning);
            int delta = numberOfProcessorsNeeded - numProcessorsRunning;

            Console.WriteLine("There are " + numProcessorsRunning  + " processors active for the " + queueClass + " and " + Queues.GetValueOrDefault(queueClass).Count + " documents pending");
            Console.WriteLine("Delta for service level " + queueClass + " is " + delta);
            //If you need more, spin up more
            if (delta > 0)
            {
                //TODO: find a way to reuse processors that are shut down, or just purge them

                for (int i = 0; i < delta; i++)
                {
                    var newProcessor = new DocumentProcessor(queueClass);
                    newProcessor.SpinUp();
                    Processors.GetValueOrDefault(queueClass).Add(newProcessor);
                }
            }
            //If you need fewer, shut down all the processors that aren't executing for free
            else if (delta < 0)
            {
                Processors.GetValueOrDefault(queueClass).ForEach(x => {
                    if (!x.HasFreeCycles())
                        x.ShutDown();
                });
            }

            //Finally, get everyone some work to do!
            Processors.GetValueOrDefault(queueClass).ForEach(x => {
                if (x.Status == ProcessorStatus.Idle)
                    x.Process(GetDocument(x.slaClass));
            });

        }
      
    }
}
 