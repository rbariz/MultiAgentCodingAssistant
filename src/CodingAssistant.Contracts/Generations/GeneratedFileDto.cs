using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Contracts.Generations
{
    public sealed class GeneratedFileDto
    {
        public string RelativePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
    }

}
