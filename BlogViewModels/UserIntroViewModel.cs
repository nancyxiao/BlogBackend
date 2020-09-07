using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogViewModels
{
    public class UserIntroViewModel
    {
        public string UserId { get; set; }
        public string Introduction { get; set; }
        public string Photo { get; set; }
        public string BlobName { get; set; }
        public string LocalPath { get; set; }
        public IFormFile ImageFile { get; set; }

    }
}
