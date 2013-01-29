using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using cabinet.patterns.extensions;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace cabinet.patterns.clases
{
    public class RepositoryBase<T, C> : cabinet.patterns.interfaces.IModel<T>
        where T : class
        where C : DbContext, new()
    {
        private C _bdContext;
        private ObjectContext _context;

        private ObjectContext context
        {
            get
            {
                if (_context == null)
                {
                    _context = ((IObjectContextAdapter)bdContext).ObjectContext;
                }
                return _context;
            }
        }
        protected C bdContext
        {
            get
            {
                if (_bdContext == null)
                {
                    _bdContext = new C();
                    //(DbContext)_bdContext.on.OnModelCreatin += this.Context_OnObjectMaterialized;
                }
                return _bdContext;
            }
        }
        public bool save()
        {
            return bdContext.SaveChanges() > 0;
        }

        public bool save(T obj)
        {
            try
            {
                //var entitySet = context.CreateObjectSet<T>();
                //entitySet.AddObject(obj);
                bdContext.Set(typeof(T)).Add(obj);
                return bdContext.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {                
                return false;
            }
        }

        public bool update(T obj)
        {
            //context.LoadMetadataFromAssembly();
            //context.AttachUpdated(obj);
            bdContext.Set(typeof(T)).Attach(obj);

            var entry = bdContext.Entry(obj);
            obj.GetType().GetProperties().Select(pI => pI.Name).ToList().ForEach(
                //Aqui marca error cuando pasa por las colecciones de hijos
                p => entry.Property(p).IsModified = true
                );
            return bdContext.SaveChanges() > 0;
        }

        public IEnumerable<T> listAll()
        {
            //var entitySet = context.CreateObjectSet<T>();
            //return entitySet.ToList();
            return bdContext.Set<T>().ToList();
        }

        public bool delete(T obj)
        {
            bdContext.Set(typeof(T)).Remove(obj);
            bdContext.SaveChanges();
            return true;
        }

        //Se dispara cuando un objeto (o registro) sea cargado de la bd en memoria
        private void Context_OnObjectMaterialized(object sender, ObjectMaterializedEventArgs e){
            //sender.GetType().GetProperties().Where(x => x.GetType() is IEnumerable).First();
        }
    }
}
