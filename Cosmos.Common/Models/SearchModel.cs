using Cosmos.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Models
{
    public class SearchModel
    {
        public string FieldName { get; set; }
        public Operator FieldOperator { get; set; } = Operator.EQ;
        public string FieldValueString { get; set; }
    }
}
