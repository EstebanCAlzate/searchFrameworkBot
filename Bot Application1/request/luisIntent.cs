namespace Bot_Application1.request
{
    public class luisIntent
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }

        public class Topscoringintent
        {
            public string intent { get; set; }
            public float score { get; set; }
        }

        public class Intent
        {
            public string intent { get; set; }
            public float score { get; set; }
        }

        public class Entity
        {
            public string entity { get; set; }
            public string type { get; set; }
            public int startIndex { get; set; }
            public int endIndex { get; set; }
            public Resolution resolution { get; set; }
        }

        public class Resolution
        {
            public string[] values { get; set; }
        }

    }
}