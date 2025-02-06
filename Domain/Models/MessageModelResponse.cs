using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MessageModelResponse
    {
        public long Number { get; set; }

        public required string Text { get; set; }

        public DateTime DateTimeCreated { get; set; }
    }
}
