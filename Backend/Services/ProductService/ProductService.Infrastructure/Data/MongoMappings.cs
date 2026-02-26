using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Data;

public static class MongoMappings
{
    public static void RegisterMappings()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Product)))
        {
            BsonClassMap.RegisterClassMap<Product>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(c => c.Id)
                  .SetElementName("_id")
                  .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }
    }
}