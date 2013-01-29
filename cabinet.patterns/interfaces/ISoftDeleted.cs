using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cabinet.patterns.interfaces
{
    public interface ISoftDeleted
    {
        bool IsDeleted {get;set;}
    }
}
