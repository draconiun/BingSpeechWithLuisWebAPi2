using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication4.Models
{
    public class BingLuisModel
    {
        public BingLuisModel()
        {
            topScoringIntent = new topScoringIntent();
            intents = new List<Models.intents>();
        }

        public string query { get; set; }
        public topScoringIntent topScoringIntent { get; set; }
        public List<intents> intents { get; set; }
    }

    public class topScoringIntent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class intents
    {
        public string intent { get; set; }
        public float score { get; set; }
    }


}