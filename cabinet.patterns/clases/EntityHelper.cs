using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace System.Data.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityHelper
    {
        private static void LoadAssemblyIntoWorkspace(MetadataWorkspace workspace, Assembly assembly)
        {
            workspace.LoadFromAssembly(assembly);
        }

        #region GetEntitySetName

        public static string GetEntitySetName(Type entityType, ObjectContext context)
        {
            EntityType edmEntityType = GetEntityType(context, entityType);
            EntityContainer container = context.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace).Single<EntityContainer>();
            EntitySet set = (EntitySet)container.BaseEntitySets.Single<EntitySetBase>(delegate(EntitySetBase p)
            {
                return (p.ElementType == edmEntityType);
            });
            return (container.Name + "." + set.Name);
        }

        #endregion

        #region GetEntityType
        public static EntityType GetEntityType(ObjectContext context, Type clrType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (clrType == null)
            {
                throw new ArgumentNullException("clrType");
            }
            EdmType type = null;
            try
            {
                type = context.MetadataWorkspace.GetType(clrType.Name, clrType.Namespace, DataSpace.OSpace);
            }
            catch (ArgumentException)
            {
                LoadAssemblyIntoWorkspace(context.MetadataWorkspace, clrType.Assembly);
                type = context.MetadataWorkspace.GetType(clrType.Name, clrType.Namespace, DataSpace.OSpace);
            }
            return (EntityType)context.MetadataWorkspace.GetEdmSpaceType((StructuralType)type);
        }

        public static bool TryGetEntityType(ObjectContext context, Type clrType, out EntityType entityType)
        {
            entityType = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (clrType == null)
            {
                throw new ArgumentNullException("clrType");
            }
            EdmType type = null;
            bool flag = context.MetadataWorkspace.TryGetType(clrType.Name, clrType.Namespace, DataSpace.OSpace, out type);
            if (!flag)
            {
                LoadAssemblyIntoWorkspace(context.MetadataWorkspace, clrType.Assembly);
                flag = context.MetadataWorkspace.TryGetType(clrType.Name, clrType.Namespace, DataSpace.OSpace, out type);
            }
            if (flag)
            {
                entityType = (EntityType)context.MetadataWorkspace.GetEdmSpaceType((StructuralType)type);
                return true;
            }
            return false;
        } 
        #endregion

        #region GetReferenceProperty

        public static PropertyDescriptor GetReferenceProperty(PropertyDescriptor pd)
        {
            return GetReferenceProperty(pd, TypeDescriptor.GetProperties(pd.ComponentType).Cast<PropertyDescriptor>());
        }

        public static PropertyDescriptor GetReferenceProperty(PropertyDescriptor pd, IEnumerable<PropertyDescriptor> properties)
        {
            string refPropertyName = pd.Name + "Reference";
            return properties.SingleOrDefault<PropertyDescriptor>(delegate(PropertyDescriptor p)
            {
                return (p.Name == refPropertyName);
            });
        }
        #endregion

    }
}