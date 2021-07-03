using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Dommel.Mapping;
using Dapper.FluentMap.Mapping;
using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Resolver;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Faaast
{
    public static partial class ApplicationBuilderExtensions
    {
        //public static IDatabase WithDommelFluentMap(this IServiceProvider services, ConnectionSettings connection, Action<FluentMapConfiguration> builder)
        //{
        //    Database db = new Database(connection);
        //    FluentMapper.Initialize(config =>
        //    {
        //        builder(config);

        //        IObjectMapper mepper = services.GetRequiredService<IObjectMapper>();
        //        foreach (var item in FluentMapper.EntityMaps)
        //        {
        //            Type type = item.Key;
        //            if (item.Value is IEntityMap dommel)
        //            {
        //                var dommelMap = dommel as IDommelEntityMap;
        //                Table table = new Table
        //                {
        //                    Name = dommelMap?.TableName
        //                };

        //                Type objInterface = item.Value.GetType().GetInterfaces().FirstOrDefault(x => x.Name.Contains("IEntityMap") && x.GetGenericArguments().Any());
        //                Type objectType = objInterface.GetGenericArguments().First();
        //                var definition = mepper.Get(objectType);

        //                table.Set(Meta.PocoObject, definition);
        //                definition.Set(Meta.MappedTable, table);

        //                foreach (var propertyMap in dommel.PropertyMaps)
        //                {
        //                    if (!propertyMap.Ignored)
        //                    {
        //                        Column column = new Column(propertyMap.ColumnName);
        //                        if (propertyMap is DommelPropertyMap dommelProp)
        //                        {
        //                            column.PrimaryKey = dommelProp.Key;
        //                            column.Identity = dommelProp.Identity || dommelProp.GeneratedOption == System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed;
        //                        }
        //                        MapColumn(table, definition, propertyMap, column);
        //                    }
        //                }
        //                db.Tables.Add(table.Name, table);
        //            }
        //        }
        //    });

        //    services.GetRequiredService<IDatabaseStore>()[connection.Name] = db;
        //    return db;
        //}

        //private static void MapColumn(Table table, DtoClass dto, IPropertyMap dommelProp, Column column)
        //{
        //    var mappedProperty = dto[dommelProp.PropertyInfo.Name];
        //    mappedProperty.Set(Meta.MappedColumn, column);
        //    column.Set(Meta.PocoProperty, mappedProperty);
        //    table.Columns.Add(column.Name, column);
        //}
    }
}
