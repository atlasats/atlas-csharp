using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.WebSockets
{
    public interface AccountListener
    {
        void Update(String json);
    }
}
