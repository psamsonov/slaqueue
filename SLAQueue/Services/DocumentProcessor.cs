using SLAQueue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SLAQueue.Services
{
    public class DocumentProcessor
    {

        private DateTime CreatedOn { get; set; }
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public SLAClass slaClass { get; private set; }
        public ProcessorStatus Status { get; private set; }
        
        /// <summary>
        /// Creates a new document processor
        /// </summary>
        /// <param name="slaClass">The SLA class that this processor processes for</param>
        public DocumentProcessor(SLAClass slaClass)
        {
            this.slaClass = slaClass;
            Status = ProcessorStatus.NotStarted;
            
        }

        /// <summary>
        /// If we're paying by the hour, can we do any free cycles before this processor has to shut down?
        /// </summary>
        /// <returns></returns>
        public bool HasFreeCycles()
        {
            var ttl = Constants.BillingCycle - (DateTime.Now - CreatedOn).TotalMilliseconds % Constants.BillingCycle;
            if (Status == ProcessorStatus.Idle)
                Console.WriteLine("Processor " + slaClass + " is idle and has " + ttl + " ms until we pay for it again");
            return ttl > Constants.TimeToProcessOneDocument;
        }

        public async Task SpinUp()
        {
            semaphore.Wait();
            Status = ProcessorStatus.Starting;
            await Task.FromResult(false);
            Status = ProcessorStatus.Idle;
            CreatedOn = DateTime.Now;
            semaphore.Release();
        }

        /// <summary>
        /// "Processes" a document for the preset length of time.
        /// A semaphore prevents the processor from being shut down while this is happening, or from processing documents while it's spinning up.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task Process(Document document)
        {
            await Task.Run(() => { 
                semaphore.Wait();
                if (document != null)
                {
                    Status = ProcessorStatus.Processing;
                    Console.WriteLine("Starting processing document " + document.Id);
                    Thread.Sleep(Constants.TimeToProcessOneDocument);
                    document.Processed = DateTime.Now;
                    Status = ProcessorStatus.Idle;
                    DocumentService.FinishDocument(document);
                    Console.WriteLine("Finished processing document " + document.Id);
                }
                semaphore.Release();
            });
        }

        public async Task ShutDown()
        {
            semaphore.Wait();
            Status = ProcessorStatus.ShuttingDown;
            await Task.FromResult(false);
            Status = ProcessorStatus.ShutDown;
            semaphore.Release();
        }
    }

    public enum ProcessorStatus
    {
        NotStarted,
        Starting,
        Processing,
        Idle,
        ShuttingDown,
        ShutDown
    }
}
