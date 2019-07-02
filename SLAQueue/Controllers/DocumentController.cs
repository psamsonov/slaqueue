using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SLAQueue.Models;
using SLAQueue.Services;

namespace SLAQueue.Controllers
{
    [Route("api/document")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        //[HttpPost]
        //public ActionResult<Guid> CreateDocument([FromBody]Document document)
        //{
        //    var user = UserService.GetUser(document.UserId);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    DocumentService.Enqueue(document, user.SLAClass);
        //    return Ok(document.Id);
        //}

        [HttpPost]
        public ActionResult<IEnumerable<Guid>> CreateDocument([FromBody]Document[] documents)
        {
            foreach (var document in documents)
            {
                var user = UserService.GetUser(document.UserId);

                if (user == null)
                {
                    return NotFound();
                }
                DocumentService.Enqueue(document, user.SLAClass);
            }
            return Ok(documents.Select(x => x.Id));
        }

        [HttpGet]
        public ActionResult<IEnumerable<Document>> Get()
        {
            return Ok(DocumentService.GetFinishedDocuments());
        }
    }
}