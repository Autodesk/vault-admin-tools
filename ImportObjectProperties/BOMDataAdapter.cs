using System;
using System.Collections.Generic;
using System.Linq;
using AWS = Autodesk.Connectivity.WebServices;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportObjectProperties
{
    class BOMDataAdapter
    {
        private static Dictionary<VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType, AWS.PropertyTypeEnum> s_dataTypeMapping;

        static BOMDataAdapter()
        {
            s_dataTypeMapping = new Dictionary<VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType, AWS.PropertyTypeEnum>();
            s_dataTypeMapping.Add(VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType.Bool, AWS.PropertyTypeEnum.Boolean);
            s_dataTypeMapping.Add(VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType.DateTime, AWS.PropertyTypeEnum.Date);
            s_dataTypeMapping.Add(VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType.Numeric, AWS.PropertyTypeEnum.Number);
            s_dataTypeMapping.Add(VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType.String, AWS.PropertyTypeEnum.Text);
        }

        public BOMDataAdapter(AWS.BOM bom)
        {
            BOM = bom;
            PropertyDefinitions = new List<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping>();
        }

        public List<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping> PropertyDefinitions { get; private set; }
        private AWS.BOM BOM { get; set;}

        public AWS.BOM UpdateProperties(IEnumerable<AWS.PropWriteReq> fileProperties)
        {
            if (BOM == null)
            {
                return null;
            }
            if ((BOM.CompAttrArray == null) || (BOM.PropArray == null))
            {
                return BOM;
            }
            AWS.BOMComp comp = BOM.CompArray.FirstOrDefault(c => c.XRefTyp == AWS.XRefTypeEnum.Internal);

            if (comp == null)
            {
                return BOM;
            }
            if (fileProperties.Any() == false)
            {
                return BOM;
            }
            List<AWS.BOMProp> properties = new List<AWS.BOMProp>(BOM.PropArray);
            List<AWS.BOMCompAttr> attributes = new List<AWS.BOMCompAttr>(BOM.CompAttrArray);

            foreach (AWS.PropWriteReq req in fileProperties)
            {
                AWS.BOMProp bomProp = properties.FirstOrDefault(p => string.Equals(p.Moniker, req.Moniker));

                if (bomProp == null)
                {
                    VDF.Vault.Currency.Properties.ContentSourcePropertyMapping definition = PropertyDefinitions.FirstOrDefault(d => string.Equals(d.ContentPropertyDefinition.Moniker, req.Moniker));

                    AWS.BOMProp newProp = new AWS.BOMProp
                    {
                        Id = properties.Count + 1,
                        DispName = definition.ContentPropertyDefinition.DisplayName,
                        Name = definition.ContentPropertyDefinition.DisplayName,
                        Moniker = definition.ContentPropertyDefinition.Moniker,
                        Typ = DataTypeToPropertyType(definition.ContentPropertyDefinition.DataType),
                    };

                    properties.Add(newProp);
                    bomProp = newProp;
                }
                AWS.BOMCompAttr attr = BOM.CompAttrArray.FirstOrDefault(a => a.PropId == bomProp.Id);

                if (attr == null)
                {
                    AWS.BOMCompAttr newAttr = new AWS.BOMCompAttr
                    {
                        Id = attributes.Count + 1,
                        CompId = comp.Id,
                        PropId = bomProp.Id,
                        Val = Convert.ToString(req.Val),
                    };

                    attributes.Add(newAttr);
                    attr = newAttr;
                }
                attr.Val = Convert.ToString(req.Val);
            }
            BOM.CompAttrArray = attributes.ToArray();
            BOM.PropArray = properties.ToArray();
            return BOM;
        }

        private static AWS.PropertyTypeEnum DataTypeToPropertyType(VDF.Vault.Currency.Properties.PropertyDefinition.PropertyDataType type)
        {
            return s_dataTypeMapping[type];
        }
    }
}
