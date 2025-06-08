using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;

namespace VA.TMP.Integration.Common
{
    public static class Schema
    {
        /// <summary>
        /// Validate schema with namespaces.
        /// </summary>
        /// <param name="area">Functional area of schema being validated.</param>
        /// <param name="schemaPath">The file path to the schema.</param>
        /// <param name="namespaces">The list of namespaces for the schema.</param>
        /// <param name="schemaFileNames">The list of schema files.</param>
        /// <param name="xml">XML represenation of the class instance.</param>
        public static void ValidateSchema(string area, string schemaPath, List<string> namespaces, List<string> schemaFileNames, string xml)
        {
            var schemas = new XmlSchemaSet();

            for (var i = 0; i < namespaces.Count; i++)
            {
                schemas.Add(namespaces[i], Path.Combine(schemaPath, schemaFileNames[i]));
            }

            var examStatusUpdateXml = XDocument.Parse(xml);
            var schemaValidationMessage = string.Empty;
            examStatusUpdateXml.Validate(schemas, (o, err) => { schemaValidationMessage = err.Message; });

            if (string.IsNullOrEmpty(schemaValidationMessage)) return;

            var validationError = $"{area} Schema Validation Error: {schemaValidationMessage}";

            throw new SchemaValidationException(validationError);
        }
    }
}