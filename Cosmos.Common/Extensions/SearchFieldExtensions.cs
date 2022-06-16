using Cosmos.Common.Enums;
using Cosmos.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Extensions
{
    public static class SearchFieldExtensions
    {
        public static List<SearchModel> GetSearchFields<T>(List<SearchFields> model) where T : new()
        {
            List<SearchModel> searchModelList = new List<SearchModel>();
            foreach (SearchFields fields in model)
            {
                if (string.IsNullOrEmpty(fields.FieldName) || fields.FieldValue.Count == 0) throw new Exception("Invalid search field.");

                string fieldName = GetDescriptionValue<T>(fields.FieldName);

                if (string.IsNullOrEmpty(fieldName)) continue;
                SearchModel searchModel = new SearchModel();
                switch (fields.OperatorType)
                {
                    case Operator.EQ:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"{fields.FieldValue.FirstOrDefault()?.ToLower()}\"",
                            FieldOperator = Operator.EQ
                        };
                        break;
                    case Operator.IN:
                        List<string> values = new List<string>();
                        foreach (var field in fields.FieldValue)
                        {
                            values.Add($"\"{field?.ToLower()}\"");
                        }
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldOperator = Operator.IN,
                            FieldValueString = $"({string.Join(",", values)})"
                        };
                        break;
                    case Operator.LT:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"{fields.FieldValue.FirstOrDefault()?.ToLower()}\"",
                            FieldOperator = Operator.LT
                        };
                        break;
                    case Operator.GT:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"{fields.FieldValue.FirstOrDefault()?.ToLower()}\"",
                            FieldOperator = Operator.GT,
                        };
                        break;
                    case Operator.LTE:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"{fields.FieldValue.FirstOrDefault()?.ToLower()}\"",
                            FieldOperator = Operator.LTE,
                        };
                        break;
                    case Operator.GTE:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"{fields.FieldValue.FirstOrDefault()?.ToLower()}\"",
                            FieldOperator = Operator.GTE
                        };
                        break;
                    case Operator.LIKE:
                        searchModel = new SearchModel()
                        {
                            FieldName = fields.FieldName,
                            FieldValueString = $"\"%{fields.FieldValue.FirstOrDefault()?.ToLower()}%\"",
                            FieldOperator = Operator.LIKE
                        };
                        break;
                }
                searchModelList.Add(searchModel);
            }
            return searchModelList;
        }
        public static string GetDescriptionValue<T>(string propertyName)
        {
            string fieldName = string.Empty;
            Type t = typeof(T);
            var _propertyNames = propertyName.Split('.');
            PropertyInfo prop = null;
            for (var i = 0; i < _propertyNames.Length; i++)
            {
                prop = t.GetProperty(_propertyNames[i], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop != null)
                {
                    var pt = prop.PropertyType;
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(List<>))
                        t = pt.GetGenericArguments()[0];
                    else
                        t = prop.GetType();
                }
            }
            if (prop != null)
            {
                var customAttribute = prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                fieldName = customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : string.Empty;
            }

            return fieldName;
        }
    }
}
