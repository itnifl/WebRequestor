using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebRequestor {
   public enum Status {
      pending = 0,
      working = 1,
      done = 2,
      needapproval = 3,
      denied = 4,
      started = 5,
      error = 6
   }
}
