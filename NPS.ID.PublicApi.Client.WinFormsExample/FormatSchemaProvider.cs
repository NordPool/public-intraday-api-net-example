using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    public class FormatSchemaProvider : Newtonsoft.Json.Schema.Generation.JSchemaGenerationProvider
    {
        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            // customize the generated schema for these types to have a format
            if (context.ObjectType == typeof(int))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "int32");
            }
            if (context.ObjectType == typeof(long) || context.ObjectType == typeof(long?))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "int64");
            }
            if (context.ObjectType == typeof(float))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "float");
            }
            if (context.ObjectType == typeof(double))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "double");
            }
            if (context.ObjectType == typeof(byte))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "byte");
            }
            if (context.ObjectType == typeof(DateTime) || context.ObjectType == typeof(DateTimeOffset))
            {
                return CreateSchemaWithFormat(context.ObjectType, context.Required, "date-time");
            }

            // use default schema generation for all other types
            return null;
        }
        
        private JSchema CreateSchemaWithFormat(Type type, Required required, string format)
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(type, required != Required.Always);
            schema.Format = format;

            return schema;
        }
    }
}
