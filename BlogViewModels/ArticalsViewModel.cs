using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogViewModels
{
    public class ArticlesViewModel
    {
        public string ArticleId { get; set; }
        public string UserId { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }

        //public List<ClassAll> ClassAllList { get; set; }
        //public List<Tags> ArticalTagsList { get; set; }

        public string TagsDetail { get; set; }

        public string State { get; set; }
    }
}
