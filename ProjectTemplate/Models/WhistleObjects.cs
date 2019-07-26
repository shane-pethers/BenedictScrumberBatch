using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectTemplate.Models
{
    public class WhistleObjects
    {
        public int PostID { get; set; }
        public string Desc { get; set; }
        public string PostText { get; set; }
        public int PostVotes {get; set;}
        public bool Flag { get; set; }
        public string Category { get; set; }
        public int UserID { get; set; }
        public string Password { get; set; }
    }
}