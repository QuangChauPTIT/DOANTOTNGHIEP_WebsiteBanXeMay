using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Utils
{
    [Serializable]
    public class JMessage
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool error { get; set; }
        public object obj { get; set; }
        public IEnumerable<dynamic> list { get; set; }
        public JMessage(int id, string title, bool error, object obj, IEnumerable<dynamic> list)
        {
            this.id = id;
            this.title = title;
            this.error = error;
            this.obj = obj;
            this.list = list;
        }
        public JMessage()
        {

        }
    }
}