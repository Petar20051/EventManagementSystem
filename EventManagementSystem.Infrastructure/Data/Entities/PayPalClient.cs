using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayPalCheckoutSdk.Core;
using PayPalHttp;

namespace EventManagementSystem.Infrastructure.Data.Entities
{
    public class PayPalClient
    {
        public static PayPalEnvironment Environment()
        {
            return new SandboxEnvironment("your-client-id", "your-client-secret");
        }

        public static PayPalHttpClient Client()
        {
            return new PayPalHttpClient(Environment());
        }
    }

}
