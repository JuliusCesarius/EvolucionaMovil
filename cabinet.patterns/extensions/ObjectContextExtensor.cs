using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Data;
using System.Data.Metadata.Edm;
using System.Diagnostics.Contracts;
//using System.Data.Entity.Infrastructure;

namespace cabinet.patterns.extensions
{
    // Summary:
    //     Provides facilities for querying and working with entity data as objects.
    static class ObjectContextExtension
    {
        public static void LoadMetadataFromAssembly(this ObjectContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(context.GetType().Assembly);
        }

        public static void AttachUpdated(this ObjectContext context, EntityObject detachedEntity)
        {
            #region EntityKey is null
            if (detachedEntity.EntityKey == null)
            {
                //String entitySetName = GetEntitySetFullName(context, detachedEntity);
                //int objectId = (int)detachedEntity.GetType().GetProperty("Id").GetValue(detachedEntity, null);
                ////detachedEntity.EntityKey = new System.Data.EntityKey(entitySetName, "Id", objectId);
                //detachedEntity.EntityKey = new EntityKey(context.GetEntitySetName(detachedEntity.GetType()));
                ////  create a new primary key
                //Guid newPk = Guid.NewGuid();

                ////  create new entity key
                //EntityKey entityKey = new EntityKey(context.GetType().Name + "." + detachedEntity.ToString(), pkName, newPk);

                ////  get type name of new entity
                //String typeName = oc.GetType().Namespace + "." + entityKey.EntitySetName;

                ////  create a new entity

            }
            #endregion
            if (detachedEntity.EntityState == EntityState.Detached)
            {
                object currentEntity = null;

                if (context.TryGetObjectByKey(detachedEntity.EntityKey, out currentEntity))
                {
                    context.ApplyCurrentValues(detachedEntity.EntityKey.EntitySetName, detachedEntity);

                    var newEntity = detachedEntity as IEntityWithRelationships;
                    var oldEntity = currentEntity as IEntityWithRelationships;

                    if (newEntity != null && oldEntity != null)
                    {
                        context.ApplyReferencePropertyChanges(newEntity, oldEntity);
                    }
                }
                else
                {
                    throw new ObjectNotFoundException();
                }
            }
        }

        public static string GetEntitySetFullName(this ObjectContext context, EntityObject entity)
        {
            // If the EntityKey exists, simply get the Entity Set name from the key
            if (entity.EntityKey != null)
            {
                return entity.EntityKey.EntitySetName;
            }
            else
            {
                string entityTypeName = entity.GetType().Name;
                var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
                string entitySetName = (from meta in container.BaseEntitySets
                                        where meta.ElementType.Name == entityTypeName
                                        select meta.Name).First();

                return container.Name + "." + entitySetName;
            }
        }

        //public static string GetEntitySetName(this ObjectContext context, Type entityType)
        //{
        //    return EntityHelper.GetEntitySetName(entityType, context);
        //}

        private static void ApplyReferencePropertyChanges(this ObjectContext context, IEntityWithRelationships newEntity, IEntityWithRelationships oldEntity)
        {
            foreach (var relatedEnd in oldEntity.RelationshipManager.GetAllRelatedEnds())
            {
                var oldReference = relatedEnd as EntityReference;

                if (oldReference != null)
                {
                    var newReference = newEntity.RelationshipManager.GetRelatedEnd(oldReference.RelationshipName,
                        oldReference.TargetRoleName) as EntityReference;

                    if (newReference != null)
                    {
                        oldReference.EntityKey = newReference.EntityKey;
                    }
                }
            }
        }

        //public static IEnumerable<string> PrimayKeysFor(this ObjectContext context, EntityObject entity)
        //{
        //    Contract.Requires(context != null);
        //    Contract.Requires(entity != null);

        //    return context.PrimayKeysFor(entity.GetType());
        //}

        //public static IEnumerable<string> PrimayKeysFor(this ObjectContext context,Type entityType)
        //{
        //    Contract.Requires(context != null);
        //    Contract.Requires(entityType != null);

        //    var metadataWorkspace = context.MetadataWorkspace;
        //    var objectItemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);
        //    var ospaceTypes = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);

        //    var ospaceType = ospaceTypes
        //        .FirstOrDefault(t => objectItemCollection.GetClrType(t) == entityType);

        //    if (ospaceType == null)
        //    {
        //        throw new ArgumentException(
        //            string.Format("The type '{0}' is not mapped as an entity type.", entityType.Name), "entityType");
        //    }

        //    return ospaceType.KeyMembers.Select(k => k.Name);
        //}

        //public static IEnumerable<string> PrimayKeyValuesFor(this ObjectContext context, EntityObject entity)
        //{
        //    Contract.Requires(context != null);
        //    Contract.Requires(entity != null);

        //    return context.PrimayKeysFor(entity);
        //}
    }
}



