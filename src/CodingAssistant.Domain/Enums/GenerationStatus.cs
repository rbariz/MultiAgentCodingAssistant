using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Domain.Enums
{
    public enum GenerationStatus
    {
        Draft = 0,
        Queued = 1,
        Planning = 2,
        Generating = 3,
        Reviewing = 4,
        Completed = 5,
        Failed = 6,
        Cancelled = 7
    }


}
