using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cabinet.patterns.interfaces
{
    public interface IRepository<T> 
    {
        bool Save();
        bool Insert(T Obj);
        bool Update(T Obj);
        T LoadById(Int32 Id);
        IEnumerable<T> ListAll();
        bool Delete(T Obj);
    }
}
