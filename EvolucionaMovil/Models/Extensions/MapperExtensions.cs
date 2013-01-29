using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using AutoMapper;
using System.Collections;

namespace AutoMapper
{
    static class MapperExtensions
    {
        public static IEnumerable<TDestination> ToListOfDestination<TDestination>(this IEnumerable ListOfSource) where TDestination:class,new()
        {
            List<TDestination> list = new List<TDestination>();
            if (ListOfSource != null)
            {
                foreach (var item in ListOfSource)
                {
                    TDestination vm = (TDestination)Mapper.Map(item, item.GetType(), typeof(TDestination));
                    list.Add(vm);
                }
            } 
            return list;
        }

        //public static void toEntityCollection<TSource, TEntity>(this EntityCollection<TEntity> EntityCollection, IEnumerable ListOfSource) where TSource : class,new()
        //{
        //    if (EntityCollection != null && ListOfSource != null)
        //    {
        //        foreach (var item in ListOfSource)
        //        {
        //            TSource vm = new TSource();
        //            TEntity entity = (TEntity)Mapper.Map(item, item.GetType(), typeof(TSource));
        //            EntityCollection.Add(entity);
        //        }
        //    }
        //}
    }
}
