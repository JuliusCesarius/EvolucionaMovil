using System;
using System.Collections.Generic;
namespace EvolucionaMovil.Models
{
    public class SimpleGridResult<T>
    {
        public int CurrentPage;
        public int PageSize;
        public int TotalRows;
        public IEnumerable<T> Result;
    }
}
