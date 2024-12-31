using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmaTrack.Shared.APIModels
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = default!;
    }

}
