using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Utils
{
    public class PageUtil
    {
        public IEnumerable<dynamic> Data { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount()
        {
            return Convert.ToInt32(Math.Ceiling(Data.Count() / (double)PageSize));
        }

        public IEnumerable<dynamic> PaginatedResult()
        {
            int start = (CurrentPage - 1) * PageSize;
            return Data.Skip(start).Take(PageSize);
        }
    }
}