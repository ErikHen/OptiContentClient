using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptiContent.Models.Properties
{
    public class PropertyBlob : PropertyBase
    {
        public BlobData? Value { get; set; }

        public override string ToString()
        {
            return Value?.Url ?? string.Empty;
        }
    }

    public class BlobData
    {
        public string? Url { get; set; }
    }
}
