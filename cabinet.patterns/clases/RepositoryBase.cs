using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using cabinet.patterns.extensions;
using System.Collections;
using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data;
using System.Linq.Dynamic;
using System;

namespace cabinet.patterns.clases
{
    public class RepositoryBase<T, C> : cabinet.patterns.interfaces.IRepository<T>,IDisposable
        where T : EntityObject
        where C : ObjectContext, new()
    {
        private C _context;

        public C context
        {
            get
            {
                if (_context == null)
                {
                    _context = new C();
                }
                return _context;
            }
        }

        public RepositoryBase(C context)
        {
            _context = context;
        }

        public RepositoryBase()
        {
        }

        public bool Save()
        {
            return context.SaveChanges() > 0;
        }

        public bool Add(T Obj)
        {
            try
            {
                //Creo el entityset de objetos del tipo
                var entitySet = context.CreateObjectSet<T>();
                //Obtengo la llave y el valor si es que tiene
                string identityName = GetIdentityName();
                //verifico que tenga llave
                if (identityName != null)
                {
                    //Obtengo su valor
                    string identityValue = Obj.GetType().GetProperty(identityName).GetValue(Obj, null).ToString();
                    //verifico si existen en la BD                    
                    if (entitySet.AsQueryable().Where(identityName + "==" + identityValue).Count() == 0)
                    {
                        //Verifico si tiene campo FechaCreacion para agregar el default
                        if (Obj.GetType().GetProperties().Any(x => x.Name == "FechaCreacion"))
                        {
                            Obj.GetType().GetProperty("FechaCreacion").SetValue(Obj, DateTime.Now, null);
                        }
                        //Verifico si tiene campo GUID para agregar el default
                        if (Obj.GetType().GetProperties().Any(x => x.Name == "ID"))
                        {
                            Obj.GetType().GetProperty("ID").SetValue(Obj, Guid.NewGuid(), null);
                        }
                        //*****************INSERT******************//
                        entitySet.AddObject(Obj);
                    }
                    else
                    {
                        //*****************UPDATE******************//
                        entitySet.Attach(Obj);
                        context.ObjectStateManager.ChangeObjectState(Obj, EntityState.Modified);
                    }
                }
                else
                {
                    entitySet.AddObject(Obj);
                }
                return true;
            }
            catch (DbEntityValidationException ex)
            {
                return false;
            }
        }

        public bool Insert(T Obj)
        {
            try
            {
                var entitySet = context.CreateObjectSet<T>();
                entitySet.AddObject(Obj);
                return context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                return false;
            }
        }

        public bool Update(T Obj)
        {
            var entitySet = context.CreateObjectSet<T>();
            entitySet.Attach(Obj);
            context.ObjectStateManager.ChangeObjectState(Obj, EntityState.Modified);
            return context.SaveChanges() > 0;
        }

        public IEnumerable<T> ListAll()
        {
            var entitySet = context.CreateObjectSet<T>();
            if (typeof(T).GetProperties().Any(x => x.Name == "Baja"))
            {
                return entitySet.AsQueryable().Where("Baja==false").ToList();
            }
            else
            {
                return entitySet.ToList();
            }
        }

        public bool Delete(T Obj)
        {
            if (Obj.GetType().GetProperties().Any(x => x.Name == "Baja"))
            {
                Obj.GetType().GetProperty("Baja").SetValue(Obj, true, null);
                context.ObjectStateManager.ChangeObjectState(Obj, EntityState.Modified);
            }
            else
            {
                context.DeleteObject(Obj);
            }
            //no hago el save aquí, para poder hacer el delete recursivo y guardar hasta el final
            return true;
        }


        public T LoadById(Int32 Id)
        {
            var entitySet = context.CreateObjectSet<T>();
            string identityName = GetIdentityName();
            if (identityName != null)
            {
                return entitySet.AsQueryable().Where(identityName + "==" + Id).FirstOrDefault();
            }
            else
            {
                throw new Exception("La entidad no cuenta con un Identity");
            }
        }

        public T LoadByGUID(Guid GUID)
        {
            var entitySet = context.CreateObjectSet<T>();
            if (typeof(T).GetProperties().Any(x => x.Name == "ID"))
            {
                return entitySet.AsQueryable().Where("ID.Equals(@0)",  GUID ).FirstOrDefault();
            }
            else
            {
                throw new Exception("La entidad no posee un campo GUID");
            }
        }

        protected string GetIdentityName()
        {
            var entitySet = context.CreateObjectSet<T>();
            var keyMembers = entitySet.EntitySet.ElementType.KeyMembers;
            if (keyMembers.Count() > 0)
            {
                var identity = keyMembers.Where(x => x.MetadataProperties.Any(y => y.Value is string && Convert.ToString(y.Value) == "Identity")).FirstOrDefault();
                if (identity != null)
                {
                    string identityName = identity.Name;
                    return identityName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public void Dispose()
        {
            //TODO:Implementar dispose
        }
    }
}
